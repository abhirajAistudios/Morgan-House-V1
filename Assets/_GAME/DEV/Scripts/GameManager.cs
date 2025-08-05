// ===============================
// GameManager.cs
// ===============================
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Master Objective Flow")]
    public List<ObjectiveDataSO> totalObjectives = new(); // Only Parents here

    private Queue<ObjectiveDataSO> objectiveQueue = new();

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
        TryStartNextObjective();
    }

    public void QueueObjective(ObjectiveDataSO objective)
    {
        if (!objectiveQueue.Contains(objective))
        {
            objectiveQueue.Enqueue(objective);
        }
    }
}