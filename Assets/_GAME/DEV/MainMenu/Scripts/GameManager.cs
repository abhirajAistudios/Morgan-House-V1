using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Master Objective Flow")]
    public List<ObjectiveDataSO> totalObjectives = new(); // Only Parents here

    public LinkedList<ObjectiveDataSO> objectiveQueue = new();
    
    [HideInInspector] public bool isNewGame = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TryStartNextObjective()
    {
        if (objectiveQueue.Count > 0)
        {
            ObjectiveDataSO nextObjective = objectiveQueue.First.Value;
            ObjectiveManager.Instance.StartObjective(nextObjective);
        }
    }

    public void OnObjectiveCompleted(ObjectiveDataSO completedObjective)
    {
        // If it's a child, do NOT dequeue next. Just check if parent is ready.
        if (completedObjective.parentObjective != null || completedObjective.objectiveType == ObjectiveType.NORMALOBJECTIVE)
        {
            Debug.Log("Objective Completed" +  completedObjective.dialogDisplay);
            if (!completedObjective.parentObjective.AreChildrenComplete())
            {
                Debug.Log("[GameManager] Waiting for other child objectives to complete.");
                return; // Wait for all children to finish.
            }

            // Parent is ready but WAIT for manual trigger to complete it.
            if (completedObjective.parentObjective.AreChildrenComplete())
            {
                completedObjective.parentObjective.CheckReadyForCompletion();
            }
            else if(completedObjective.objectiveType == ObjectiveType.NORMALOBJECTIVE)
            {
                completedObjective.parentObjective.CheckReadyForCompletion();
            }
            
            return; // Do NOT dequeue next objective here.
        }
        
        RemoveChildObjective(completedObjective);
        objectiveQueue.Remove(completedObjective);
        
        // If this is a parent and completed, proceed to next.
        TryStartNextObjective();
    }
    
    public void RemoveChildObjective(ObjectiveDataSO parentObjective)
    {
        foreach (ObjectiveDataSO childObjective in parentObjective.ChildObjectives)
        {
            objectiveQueue.Remove(childObjective);
        }
    }
    public void QueueObjectiveInLast(ObjectiveDataSO objective)
    {
        if (!objectiveQueue.Contains(objective))
        {
            objectiveQueue.AddLast(objective);
        }
    }

    public void QueueObjectiveInFirst(ObjectiveDataSO objective)
    {
        if (!objectiveQueue.Contains(objective))
        {
            objectiveQueue.AddFirst(objective);
        }
    }
    
    public void ResetAllObjectives()
    {
        objectiveQueue.Clear();
        
        foreach (var objective in totalObjectives)
        {
            ResetObjectiveRecursive(objective);
        }
        Debug.Log("[GameManager] All objectives have been reset to NOTSTARTED.");
    }

    private void ResetObjectiveRecursive(ObjectiveDataSO objective)
    {
        objective.objectiveStatus = ObjectiveStatus.NOTSTARTED;
        QueueObjectiveInLast(objective);

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
        
        var saveSystem = FindAnyObjectByType<AutoSaveManager>();
        
        saveSystem.LoadObjectives();
        
        foreach (var obj in totalObjectives)
        {
            RestoreObjectiveRecursive(obj);
        }
        
        TryStartNextObjective();
    }
    
    private void RestoreObjectiveRecursive(ObjectiveDataSO objective)
    {
        if (objective == null) return;

        QueueObjectiveInLast(objective);
        
        
        if (objective.objectiveStatus == ObjectiveStatus.COMPLETED && objective.parentObjective != null &&
            objective.parentObjective.objectiveStatus != ObjectiveStatus.COMPLETED)
        {
            objective.objectiveStatus = ObjectiveStatus.INPROGRESS;
        }
        
        else if (objective.objectiveStatus == ObjectiveStatus.COMPLETED)
        {
            ObjectiveManager.Instance.completedObjectives.Add(objective);
            objectiveQueue.Remove(objectiveQueue.Find(objective));
            ObjectiveManager.Instance.objectiveUIManager.uiRemovedObjectives.Add(objective);
        }

        foreach (var child in objective.ChildObjectives)
        {
            RestoreObjectiveRecursive(child);
        }
    }

    public void StartNewObjective(List<ObjectiveDataSO> objectiveList)
    {
        foreach (var objective in objectiveList)
        {
            ResetObjective(objective);
        }
        
        ObjectiveManager.Instance.activeObjectives.Clear();
        TryStartNextObjective();
        ObjectiveManager.Instance.objectiveUIManager.RefreshUI();
    }

    public void ResetObjective(ObjectiveDataSO objective)
    {
        if(ObjectiveManager.Instance.completedObjectives.Contains(objective)) return;
        
        if (objective.objectiveStatus == ObjectiveStatus.COMPLETED && objective.parentObjective != null &&
            objective.parentObjective.objectiveStatus != ObjectiveStatus.COMPLETED && objective.parentObjective.objectiveStatus != ObjectiveStatus.INPROGRESS)
        {
            objective.objectiveStatus = ObjectiveStatus.NOTSTARTED;
        }
        
        if(objectiveQueue.Contains(objective)) return;
        //if(objectiveQueue.Contains(objective) && objective.objectiveStatus == ObjectiveStatus.COMPLETED && objective.objectiveType == ObjectiveType.CHILDOBJECTIVE) return;
        
        objective.objectiveStatus = ObjectiveStatus.NOTSTARTED;

        if (objective.hasUnlockables)
        {
            foreach (var objectives in objective.UnlockOnComplete)
            {
                objectives.objectiveState = ObjectiveState.LOCKED;
                objectives.objectiveStatus = ObjectiveStatus.NOTSTARTED;
            }
        }
        
        if (objective.ChildObjectives != null && objective.ChildObjectives.Count > 0)
        {
            foreach (var child in objective.ChildObjectives)
            {
                ResetObjective(child);
            }
        }

        if (objective.objectiveType == ObjectiveType.PARENTOBJECTIVE ||
            objective.objectiveType == ObjectiveType.NORMALOBJECTIVE)
        {
            objectiveQueue.AddFirst(objective);
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