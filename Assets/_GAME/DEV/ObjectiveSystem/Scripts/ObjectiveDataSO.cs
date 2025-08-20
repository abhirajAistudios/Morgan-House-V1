using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ObjectiveDataSO : ScriptableObject , ISaveable
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

        // Notify parent that a child has completed
        if (parentObjective != null || objectiveType == ObjectiveType.NORMALOBJECTIVE)
        {
            parentObjective.CheckReadyForCompletion();
        }
    }

    public virtual void CheckReadyForCompletion()
    {
        if (AreChildrenComplete() && objectiveStatus != ObjectiveStatus.COMPLETED)
        {
            Debug.Log($"[ObjectiveDataSO] {objectiveName} is now ready for manual completion.");
            // TODO: Trigger UI prompt to inform player to manually complete this objective.
            ObjectiveManager.Instance.objectiveUIManager.ShowParentCompletionPrompt(this);
        }
    }
    
    public abstract void Initialize();

    public bool AreChildrenComplete()
    {
        return ChildObjectives != null && ChildObjectives.All(child => child.objectiveStatus == ObjectiveStatus.COMPLETED);
    }

    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        if (objectiveStatus == ObjectiveStatus.COMPLETED && !data.objectives.Contains(this))
        {
            data.objectives.Add(this);
            Debug.Log("Saved with ID: " + dialogDisplay);
        }
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        if (data.objectives.Contains(this))
        {
            objectiveStatus = ObjectiveStatus.COMPLETED;
            Debug.Log("Restored with ID: " + dialogDisplay);
        }
        else if(objectiveType == ObjectiveType.PARENTOBJECTIVE)
        {
            objectiveStatus = ObjectiveStatus.NOTSTARTED;
        }
    }
}