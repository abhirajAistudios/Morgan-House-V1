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

    private void Start()
    {
        objectiveUIManager = FindAnyObjectByType<ObjectiveUIManager>();

        foreach (var objective in activeObjectives)
        {
            if (objective.objectiveState == ObjectiveState.UNLOCKED &&
                objective.objectiveStatus == ObjectiveStatus.NOTSTARTED)
            {
                StartObjective(objective);
            }
        }
    }

    public void StartObjective(ObjectiveDataSO objective)
    {
        objective.StartObjective();
        objectiveUIManager?.OnObjectiveUpdated();
    }

    public void OnObjectiveCompleted(ObjectiveDataSO completedObjective)
    {

        completedObjectives.Add(completedObjective);

        // DO NOT remove from activeObjectives – we want to keep showing it
        Debug.Log($"[ObjectiveManager] Completed: {completedObjective.objectiveName}");

        // Unlock new ones
        if (completedObjective.hasUnlockables)
        {
            foreach (var unlock in completedObjective.UnlockOnComplete)
            {
                if (unlock.objectiveState == ObjectiveState.LOCKED)
                {
                    unlock.objectiveState = ObjectiveState.UNLOCKED;
                    StartObjective(unlock);
                }
            }
        }

        objectiveUIManager?.OnObjectiveUpdated();
    }

    public void ResetObjectives()
    {
        activeObjectives.Clear();
        completedObjectives.Clear();
        Debug.Log("[ObjectiveManager] Reset all objectives.");
    }
}
