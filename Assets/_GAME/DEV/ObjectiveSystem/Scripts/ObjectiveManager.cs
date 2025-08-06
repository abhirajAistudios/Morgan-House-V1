using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    public List<ObjectiveDataSO> activeObjectives = new();
    public List<ObjectiveDataSO> completedObjectives = new();

    public ObjectiveUIManager objectiveUIManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        if (completedObjective.parentObjective != null)
        {
            completedObjective.parentObjective.CheckReadyForCompletion();
        }

        // Handle Unlockables
        if (completedObjective.hasUnlockables)
        {
            foreach (var unlock in completedObjective.UnlockOnComplete)
            {
                if (unlock.objectiveState == ObjectiveState.LOCKED)
                {
                    unlock.objectiveState = ObjectiveState.UNLOCKED;
                    GameManager.Instance.QueueObjective(unlock);
                }
            }
        }

        objectiveUIManager?.OnObjectiveUpdated();

        // Notify GameManager to progress
        GameManager.Instance.OnObjectiveCompleted(completedObjective);
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
}
