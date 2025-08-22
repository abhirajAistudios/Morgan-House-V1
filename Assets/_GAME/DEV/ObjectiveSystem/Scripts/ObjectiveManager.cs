using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    public List<ObjectiveDataSO> activeObjectives = new();
    public List<ObjectiveDataSO> completedObjectives = new();
    public ObjectiveUIManager objectiveUIManager;
    
    [SerializeField] private List<ObjectiveDataSO> totalObjectives = new();
    [SerializeField] private ObjectiveDataSO FinalObjective;
    
    private GameManager gameManager = GameManager.Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (gameManager.isNewGame)
        {
            ResetAllObjectives();
            gameManager.ResetAllObjectives(); // only on new game
            gameManager.ResumeGame();
        }
        else
        {
            var saveSystem = FindAnyObjectByType<AutoSaveManager>();
        
            saveSystem.LoadObjectives();
            
            gameManager.RestoreConnectedObjectiveProgress();
        }
    }

    public void Update()
    {
        CheckCompletionOfAllObjectives();
    }
    public void StartObjective(ObjectiveDataSO objective)
    {
        if (objective == null || activeObjectives.Contains(objective) || objective.objectiveStatus == ObjectiveStatus.COMPLETED)
            return;

        activeObjectives.Add(objective);
        objective.StartObjective();

        // Auto-start Child Objectives
        if (objective.ChildObjectives != null && objective.ChildObjectives.Count > 0)
        {
            foreach (var child in objective.ChildObjectives)
            {
                child.objectiveStatus = ObjectiveStatus.NOTSTARTED;
                StartObjective(child);
            }
        }
    }

    public void OnObjectiveCompleted(ObjectiveDataSO completedObjective)
    {
        if (completedObjective == null || completedObjectives.Contains(completedObjective))
            return;

        // REMOVE from Active Objectives List
        if (activeObjectives.Contains(completedObjective))
            activeObjectives.Remove(completedObjective);

        completedObjectives.Add(completedObjective);

        // Handle Unlockables
        if (completedObjective.hasUnlockables)
        {
            foreach (var unlock in completedObjective.UnlockOnComplete)
            {
                    unlock.objectiveState = ObjectiveState.UNLOCKED;
                    unlock.objectiveStatus = ObjectiveStatus.NOTSTARTED;
                    gameManager.QueueObjectiveInLast(unlock);
            }
        }

        objectiveUIManager?.OnObjectiveUpdated();

        // Notify GameManager to progress
        gameManager.OnObjectiveCompleted(completedObjective);
    }

    public void OnObjectiveUpdatedImmediately()
    {
        // Go through all active objectives and check if they can be completed now
        foreach (var objective in activeObjectives)
        {
            if (objective is CollectibleObjectiveSO collectibleObjective)
            {
                collectibleObjective.CheckImmediateCompletion();
            }
        }
    }

    public void CheckCompletionOfAllObjectives()
    {
        if (AllObjectivesCompleted())
        {
            gameManager.QueueObjectiveInFirst(FinalObjective);
            gameManager.TryStartNextObjective();
            objectiveUIManager.OnObjectiveUpdated();
        }
    }

    public bool AllObjectivesCompleted()
    {
        int i = 0;
        
        foreach (var objective in totalObjectives)
        {
            if (objective.objectiveStatus == ObjectiveStatus.COMPLETED)
            {
                i++;
            }
        }

        if (i == totalObjectives.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public void ResetAllObjectives()
    {
        FinalObjective.objectiveStatus = ObjectiveStatus.NOTSTARTED;
        
        foreach (var objective in totalObjectives)
        {
            ResetObjectiveRecursive(objective);
        }
    }
    private void ResetObjectiveRecursive(ObjectiveDataSO objective)
    {
        objective.objectiveStatus = ObjectiveStatus.NOTSTARTED;

        if (objective.ChildObjectives != null && objective.ChildObjectives.Count > 0)
        {
            foreach (var child in objective.ChildObjectives)
            {
                ResetObjectiveRecursive(child);
            }
        }
    }
}