using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ObjectiveDataSO : ScriptableObject
{
    public string objectiveName;
    public string dialogDisplay;

    public Objective objective;
    public ObjectiveType objectiveType;
    public ObjectiveState objectiveState = ObjectiveState.LOCKED;
    public ObjectiveStatus objectiveStatus = ObjectiveStatus.NOTSTARTED;

    public bool hasUnlockables;
    public ObjectiveDataSO parentObjective;
    public ObjectiveDataSO unlockingObjective;
    public List<ObjectiveDataSO> ChildObjectives;
    public List<ObjectiveDataSO> UnlockOnComplete;

    public virtual void StartObjective()
    {
        if (objectiveStatus == ObjectiveStatus.COMPLETED) return;

        objectiveStatus = ObjectiveStatus.INPROGRESS;
        objectiveState = ObjectiveState.UNLOCKED;
        Initialize();
    }

    public virtual void CompleteObjective()
    {
        if (objectiveStatus == ObjectiveStatus.COMPLETED || objectiveState == ObjectiveState.LOCKED) return;

        objectiveStatus = ObjectiveStatus.COMPLETED;
        Debug.Log("Objective Complete " + objectiveName);
        ObjectiveManager.Instance.OnObjectiveCompleted(this);

        // DO NOT auto-complete parent here!
        // Instead, notify parent to check if it can now be completed.
        if (parentObjective != null)
        {
            parentObjective.CheckReadyForCompletion();
        }
    }

    public virtual void CheckReadyForCompletion()
    {
        if (AreChildrenComplete() && objectiveStatus != ObjectiveStatus.COMPLETED)
        {
            Debug.Log($"[ObjectiveDataSO] {objectiveName} is now ready for completion.");
            // At this point, you should trigger manual player input or auto-complete if desired
            // Example: Show a UI prompt for the player to "Complete" this objective
        }
    }

    public abstract void Initialize();

    public bool AreChildrenComplete()
    {
        return ChildObjectives != null && ChildObjectives.All(child => child.objectiveStatus == ObjectiveStatus.COMPLETED);
    }
}
