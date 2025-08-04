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
        if (objectiveStatus == ObjectiveStatus.COMPLETED) return;

        objectiveStatus = ObjectiveStatus.COMPLETED;
        Debug.Log($"[Objective Completed] {objectiveName}");

        ObjectiveManager.Instance.OnObjectiveCompleted(this);
    }

    public abstract void Initialize();

    public bool AreChildrenComplete()
    {
        return ChildObjectives == null || ChildObjectives.All(child => child.objectiveStatus == ObjectiveStatus.COMPLETED);
    }
}