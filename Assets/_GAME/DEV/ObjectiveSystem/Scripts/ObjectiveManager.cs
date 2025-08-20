using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    public List<ObjectiveDataSO> activeObjectives = new();
    public List<ObjectiveDataSO> completedObjectives = new();
    
    public List<ObjectiveDataSO> totalObjectives = new();
    
    public ObjectiveDataSO FinalObjective;

    public ObjectiveUIManager objectiveUIManager;
    
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
            Debug.Log("Reset all objectives");
            gameManager.ResumeGame();
        }
        else
        {
            var saveSystem = FindAnyObjectByType<AutoSaveManager>();
        
            saveSystem.LoadObjectives();
            
            gameManager.RestoreConnectedObjectiveProgress();
            //gameManager.RestoreObjectiveProgress();
            return;
        }
        
        //gameManager.TryStartNextObjective();
    }

    public void Update()
    {
        CheckCompletionOfAllObjectives();
    }
    public void StartObjective(ObjectiveDataSO objective)
    {
        if (objective == null || activeObjectives.Contains(objective) || objective.objectiveStatus == ObjectiveStatus.COMPLETED)
            return;

        Debug.Log($"[ObjectiveManager] Starting Objective: {objective.objectiveName}");

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

        Debug.Log($"[ObjectiveManager] Completed: {completedObjective.objectiveName}");

        // REMOVE from Active Objectives List
        if (activeObjectives.Contains(completedObjective))
            activeObjectives.Remove(completedObjective);

        completedObjectives.Add(completedObjective);

        // Notify Parent Objective to check if it's ready for completion
        if (completedObjective.parentObjective != null ||   completedObjective.objectiveType == ObjectiveType.NORMALOBJECTIVE)
        {
            completedObjective.parentObjective.CheckReadyForCompletion();
        }

        // Handle Unlockables
        if (completedObjective.hasUnlockables)
        {
            foreach (var unlock in completedObjective.UnlockOnComplete)
            {
                //if (unlock.objectiveState == ObjectiveState.LOCKED)
                //{
                    unlock.objectiveState = ObjectiveState.UNLOCKED;
                    unlock.objectiveStatus = ObjectiveStatus.NOTSTARTED;
                    gameManager.QueueObjectiveInLast(unlock);
                //}
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
