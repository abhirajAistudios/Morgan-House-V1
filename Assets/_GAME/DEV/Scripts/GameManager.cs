using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Master Objective Flow")]
    public List<ObjectiveDataSO> totalObjectives = new(); // Only Parents here

    public Queue<ObjectiveDataSO> objectiveQueue = new();
    
    public bool isNewGame = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var obj in totalObjectives)
        {
            if (obj.objectiveState == ObjectiveState.UNLOCKED)
                objectiveQueue.Enqueue(obj);
        }
        
    }

    public void TryStartNextObjective()
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
    
    public void RestoreObjectiveProgress()
    {
        objectiveQueue.Clear();
        
        foreach (var obj in totalObjectives)
        {
            RestoreObjectiveRecursive(obj);
        }
    }
    
    private void RestoreObjectiveRecursive(ObjectiveDataSO objective)
    {
        if (objective == null) return;

        QueueObjective(objective);
        
        if (objective.objectiveStatus == ObjectiveStatus.COMPLETED && objective.parentObjective != null &&
            objective.parentObjective.objectiveStatus != ObjectiveStatus.COMPLETED)
        {
            objective.objectiveStatus = ObjectiveStatus.INPROGRESS;
        }
        
        if (objective.objectiveStatus == ObjectiveStatus.INPROGRESS)
        {
            ObjectiveManager.Instance.StartObjective(objective);
        }
        else if (objective.objectiveStatus == ObjectiveStatus.COMPLETED)
        {
            ObjectiveManager.Instance.completedObjectives.Add(objective);
            objectiveQueue.Dequeue();
            ObjectiveManager.Instance.objectiveUIManager.uiRemovedObjectives.Add(objective);
        }

        foreach (var child in objective.ChildObjectives)
        {
            RestoreObjectiveRecursive(child);
        }
    }

    public void StartNewGame()
    {
        isNewGame = true;
    }

    public void ResumeGame()
    {
        isNewGame = false;
    }
    
}