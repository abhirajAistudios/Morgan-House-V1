using UnityEngine;

/// <summary>
/// ScriptableObject that defines an objective for using a specific item.
/// Completes when the specified item is used and all child objectives are complete.
/// </summary>
[CreateAssetMenu(menuName = "Objective System/Use Objective")]
public class UseObjectiveSO : ObjectiveDataSO
{
    [Tooltip("The unique identifier of the item that needs to be used")]
    public string ItemId;
    
    /// Called when any item is used in the game
    public void OnItemUsed(string itemId)
    {
        // Only proceed if this is the correct item and the objective is in progress
        if (itemId != ItemId || objectiveStatus != ObjectiveStatus.INPROGRESS) return;

        // Complete the objective if all child objectives are complete
        if (AreChildrenComplete())
        {
            CompleteObjective();
        }
    }
    
    /// Initializes the objective by subscribing to item used events
    public override void Initialize()
    {
        // Listen for item used events to check if this objective is completed
        GameService.Instance.EventService.OnObjectUsed.AddListener(OnItemUsed);
    }
}