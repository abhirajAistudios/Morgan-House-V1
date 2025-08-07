using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Master Objective Flow")]
    public List<ObjectiveDataSO> totalObjectives = new(); // Only Parents here

    private Queue<ObjectiveDataSO> objectiveQueue = new();
    
    public bool isNewGame = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var obj in totalObjectives)
        {
            if (obj.objectiveState == ObjectiveState.UNLOCKED)
                objectiveQueue.Enqueue(obj);
        }
        
    }

    private void Start()
    {
        if (MainGameManager.Instance != null && MainGameManager.Instance.isNewGame)
        {
            ResetAllObjectives(); // Reset only if New Game
            MainGameManager.Instance.isNewGame = false; // Reset the flag so it doesn't happen again on resume
        }
        else
        {
            // This is a Resume, restore active objectives
            foreach (var obj in totalObjectives)
            {
                RestoreActiveObjectivesRecursive(obj);
            }
        }
        
        TryStartNextObjective();
    }

    private void TryStartNextObjective()
    {
        if (objectiveQueue.Count > 0)
        {
            ObjectiveDataSO nextObjective = objectiveQueue.Dequeue();
            ObjectiveManager.Instance.StartObjective(nextObjective);
        }
    }

    public void OnObjectiveCompleted(ObjectiveDataSO completedObjective)
    {
        // If it's a child, do NOT dequeue next. Just check if parent is ready.
        if (completedObjective.parentObjective != null)
        {
            if (!completedObjective.parentObjective.AreChildrenComplete())
            {
                Debug.Log("[GameManager] Waiting for other child objectives to complete.");
                return; // Wait for all children to finish.
            }

            // Parent is ready but WAIT for manual trigger to complete it.
            completedObjective.parentObjective.CheckReadyForCompletion();
            return; // Do NOT dequeue next objective here.
        }

        // If this is a parent and completed, proceed to next.
        TryStartNextObjective();
    }


    public void QueueObjective(ObjectiveDataSO objective)
    {
        if (!objectiveQueue.Contains(objective))
        {
            objectiveQueue.Enqueue(objective);
        }
    }
    
    public void ResetAllObjectives()
    {
        foreach (var objective in totalObjectives)
        {
            ResetObjectiveRecursive(objective);
        }
        Debug.Log("[GameManager] All objectives have been reset to NOTSTARTED.");
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
    
    private void RestoreActiveObjectivesRecursive(ObjectiveDataSO objective)
    {
        if (objective.objectiveStatus == ObjectiveStatus.INPROGRESS && objective.objectiveState == ObjectiveState.UNLOCKED)
        {
            ObjectiveManager.Instance.activeObjectives.Add(objective);
        }

        if (objective.ChildObjectives != null && objective.ChildObjectives.Count > 0)
        {
            foreach (var child in objective.ChildObjectives)
            {
                RestoreActiveObjectivesRecursive(child);
            }
        }
    }

}