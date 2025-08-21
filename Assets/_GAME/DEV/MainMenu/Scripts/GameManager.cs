using System.Collections.Generic;
using UnityEngine;

// This class manages the game's objective system and tracks their progress.
public class GameManager : MonoBehaviour, ISaveable
{
    // Singleton instance
    public static GameManager Instance;

    [Header("Master Objective Flow")]
    public List<ObjectiveDataSO> completedObjectives = new(); // Stores completed objectives
    public LinkedList<ObjectiveDataSO> objectiveQueue = new(); // Queue of objectives to be completed

    [HideInInspector] public bool isNewGame = false; // Used to differentiate between new/resumed games

    // Singleton pattern setup
    private void Awake()
    {
        // Ensures only one instance of GameManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep GameManager across scenes
    }

    // Starts the next objective in the queue if any exist
    public void TryStartNextObjective()
    {
        if (objectiveQueue.Count > 0)
        {
            ObjectiveDataSO nextObjective = objectiveQueue.First.Value;
            ObjectiveManager.Instance.StartObjective(nextObjective);
        }
    }

    // Handles logic when an objective is completed
    public void OnObjectiveCompleted(ObjectiveDataSO completedObjective)
    {
        // If this is a child objective
        if (completedObjective.parentObjective != null)
        {
            // If parent is not ready, wait for other children
            if (!completedObjective.parentObjective.AreChildrenComplete())
            {
                return;
            }

            // If parent is ready, check for manual completion condition
            if (completedObjective.parentObjective.AreChildrenComplete())
            {
                completedObjective.parentObjective.CheckReadyForCompletion();
            }
        }

        // Add to completed list
        completedObjectives.Add(completedObjective);

        // Remove any children from the queue
        RemoveChildObjective(completedObjective);

        // Remove this completed objective from the queue
        objectiveQueue.Remove(completedObjective);

        // Start the next objective
        TryStartNextObjective();
    }

    // Removes all child objectives of a parent from the queue
    public void RemoveChildObjective(ObjectiveDataSO parentObjective)
    {
        foreach (ObjectiveDataSO childObjective in parentObjective.ChildObjectives)
        {
            objectiveQueue.Remove(childObjective);
        }
    }

    // Adds an objective to the end of the queue (lowest priority)
    public void QueueObjectiveInLast(ObjectiveDataSO objective)
    {
        if (!objectiveQueue.Contains(objective))
        {
            objectiveQueue.AddLast(objective);
        }
    }

    // Adds an objective to the start of the queue (highest priority)
    public void QueueObjectiveInFirst(ObjectiveDataSO objective)
    {
        if (!objectiveQueue.Contains(objective))
        {
            objectiveQueue.AddFirst(objective);
        }
    }

    // Resets all objectives in the game
    public void ResetAllObjectives()
    {
        objectiveQueue.Clear();

        // Recursively reset each completed objective and its children
        foreach (var objective in completedObjectives)
        {
            ResetObjectiveRecursive(objective);
        }

        completedObjectives.Clear();
    }

    // Recursively resets objective status to NOTSTARTED
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

    // Recursively restores completed objective states and unlocks any associated objectives
    private void RestoreObjectiveRecursive(ObjectiveDataSO objective)
    {
        if (objective == null) return;

        // If completed and has unlockables, queue the unlockables
        if (objective.objectiveStatus == ObjectiveStatus.COMPLETED && objective.hasUnlockables)
        {
            if (objective.UnlockOnComplete != null)
            {
                foreach (var unlockObjective in objective.UnlockOnComplete)
                {
                    if (unlockObjective.objectiveStatus != ObjectiveStatus.COMPLETED || 
                        !completedObjectives.Contains(unlockObjective))
                    {
                        unlockObjective.objectiveStatus = ObjectiveStatus.NOTSTARTED;
                        QueueObjectiveInLast(unlockObjective);
                    }
                }
            }
        }

        // Add to completed list in ObjectiveManager
        if (objective.objectiveStatus == ObjectiveStatus.COMPLETED)
        {
            ObjectiveManager.Instance.completedObjectives.Add(objective);
            ObjectiveManager.Instance.objectiveUIManager.uiRemovedObjectives.Add(objective);
        }
    }

    // Starts a new set of objectives, resetting their status and updating the UI
    public void StartNewObjective(List<ObjectiveDataSO> objectiveList)
    {
        foreach (var objective in objectiveList)
        {
            ResetObjective(objective);
        }

        ObjectiveManager.Instance.activeObjectives.Clear();
        TryStartNextObjective();
        ObjectiveManager.Instance.objectiveUIManager.OnObjectiveUpdated();
    }

    // Resets an individual objective and its children
    public void ResetObjective(ObjectiveDataSO objective)
    {
        // If already completed, no need to reset
        if (ObjectiveManager.Instance.completedObjectives.Contains(objective)) return;

        // Reset logic only if parent is incomplete
        if (objective.objectiveStatus == ObjectiveStatus.COMPLETED && 
            objective.parentObjective != null &&
            objective.parentObjective.objectiveStatus != ObjectiveStatus.COMPLETED && 
            objective.parentObjective.objectiveStatus != ObjectiveStatus.INPROGRESS)
        {
            objective.objectiveStatus = ObjectiveStatus.NOTSTARTED;
        }

        // Skip if already in queue
        if (objectiveQueue.Contains(objective)) return;

        // Reset the objective status
        objective.objectiveStatus = ObjectiveStatus.NOTSTARTED;

        // Lock and reset unlockable objectives
        if (objective.hasUnlockables)
        {
            foreach (var objectives in objective.UnlockOnComplete)
            {
                objectives.objectiveState = ObjectiveState.LOCKED;
                objectives.objectiveStatus = ObjectiveStatus.NOTSTARTED;
            }
        }

        // Recursively reset children
        if (objective.ChildObjectives != null && objective.ChildObjectives.Count > 0)
        {
            foreach (var child in objective.ChildObjectives)
            {
                ResetObjective(child);
            }
        }

        // Add to queue if it's a valid type
        if (objective.objectiveType == ObjectiveType.PARENTOBJECTIVE || 
            objective.objectiveType == ObjectiveType.NORMALOBJECTIVE)
        {
            objectiveQueue.AddFirst(objective);
        }
    }

    // Restores objective queue based on completed objectives
    public void RestoreConnectedObjectiveProgress()
    {
        objectiveQueue.Clear();

        foreach (var obj in completedObjectives)
        {
            RestoreObjectiveRecursive(obj);
        }

        TryStartNextObjective();
    }

    // Marks game as a new game
    public void StartNewGame()
    {
        isNewGame = true;
    }

    // Resumes game (not a new game)
    public void ResumeGame()
    {
        isNewGame = false;
    }

    // Interface method to save the game state (currently not implemented)
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        // Implementation can be added to store completedObjectives, queue state, etc.
    }

    // Loads saved objectives from previous state
    public void LoadState(AutoSaveManager.SaveData data)
    {
        completedObjectives.Clear();

        foreach (var objectiveDataSo in data.objectives)
        {
            completedObjectives.Add(objectiveDataSo);
        }
    }
}