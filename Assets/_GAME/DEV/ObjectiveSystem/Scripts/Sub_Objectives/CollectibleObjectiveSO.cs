using UnityEngine;

[CreateAssetMenu(menuName = "Objective System/Collectible Objective")]
public class CollectibleObjectiveSO : ObjectiveDataSO
{
    public string ItemId;
    public int RequiredCount;
    private int currentCount = 0;

    public override void Initialize()
    {
        // Recount how many were already collected (e.g., from save file)
        currentCount = ItemTracker.Instance.GetItemCount(ItemId);

        // Listen for future collections
        EventService.Instance.OnObjectCollected.AddListener(OnItemCollected);

        // Check if already done
        if (currentCount >= RequiredCount && AreChildrenComplete())
        {
            CompleteObjective();
            EventService.Instance.OnObjectiveCompleted.InvokeEvent(this);
        }
    }

    public void OnItemCollected(string collectedItemId)
    {
        if (objectiveStatus != ObjectiveStatus.INPROGRESS) return;
        if (collectedItemId != ItemId) return;

        currentCount = ItemTracker.Instance.GetItemCount(ItemId);

        if (currentCount >= RequiredCount && AreChildrenComplete())
        {
            CompleteObjective();
            EventService.Instance.OnObjectiveCompleted.InvokeEvent(this);
        }
    }

    public void CheckImmediateCompletion()
    {
        if (objectiveStatus != ObjectiveStatus.INPROGRESS) return;

        int collectedCount = ItemTracker.Instance.GetItemCount(ItemId);

        if (collectedCount >= RequiredCount && AreChildrenComplete())
        {
            CompleteObjective();
        }
    }
}