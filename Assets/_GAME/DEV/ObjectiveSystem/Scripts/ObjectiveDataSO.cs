using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Abstract base class for all objective data stored as ScriptableObjects
public abstract class ObjectiveDataSO : ScriptableObject, ISaveable
{
    // Name of the objective (for internal use or display)
    public string objectiveName;

    // Display text shown to the player
    public string dialogDisplay;

    // Reference to the in-game objective logic (if needed)
    public Objective objective;

    // The type of the objective (e.g., normal, parent, etc.)
    public ObjectiveType objectiveType;

    // The current state of the objective (locked, unlocked)
    public ObjectiveState objectiveState = ObjectiveState.LOCKED;

    // The current progress status of the objective (not started, in progress, completed)
    public ObjectiveStatus objectiveStatus = ObjectiveStatus.NOTSTARTED;

    // If this objective unlocks other objectives when completed
    public bool hasUnlockables;

    // Reference to the parent objective, if any
    public ObjectiveDataSO parentObjective;

    // The objective that must be completed to unlock this one
    public ObjectiveDataSO unlockingObjective;

    // List of objectives that are children of this one
    public List<ObjectiveDataSO> ChildObjectives;

    // List of objectives to unlock when this one is completed
    public List<ObjectiveDataSO> UnlockOnComplete;

    // Starts the objective if it hasn’t been completed yet
    public virtual void StartObjective()
    {
        if (objectiveStatus == ObjectiveStatus.COMPLETED) return;

        objectiveStatus = ObjectiveStatus.INPROGRESS;
        objectiveState = ObjectiveState.UNLOCKED;
        Initialize();
    }

    // Completes the objective and notifies the manager and parent (if any)
    public virtual void CompleteObjective()
    {
        if (objectiveStatus == ObjectiveStatus.COMPLETED || objectiveState == ObjectiveState.LOCKED) return;

        objectiveStatus = ObjectiveStatus.COMPLETED;

        // Notify the objective manager
        ObjectiveManager.Instance.OnObjectiveCompleted(this);
    }

    // Abstract method to initialize the objective; must be implemented in subclasses
    public abstract void Initialize();

    // Checks if all child objectives have been completed
    public bool AreChildrenComplete()
    {
        return ChildObjectives != null && ChildObjectives.All(child => child.objectiveStatus == ObjectiveStatus.COMPLETED);
    }

    // Saves the state of the objective if it is completed
    public void SaveState(ref SaveData data)
    {
        if (objectiveStatus == ObjectiveStatus.COMPLETED && !data.objectives.Contains(this))
        {
            data.objectives.Add(this);
        }
    }

    // Loads the state of the objective from save data
    public void LoadState(SaveData data)
    {
        if (data.objectives.Contains(this))
        {
            objectiveStatus = ObjectiveStatus.COMPLETED;
        }
        // If it's a parent objective but not in saved data, reset it
        else if (objectiveType == ObjectiveType.PARENTOBJECTIVE)
        {
            objectiveStatus = ObjectiveStatus.NOTSTARTED;
        }
    }
}