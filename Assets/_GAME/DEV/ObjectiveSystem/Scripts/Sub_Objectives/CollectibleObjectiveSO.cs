using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Objective System/Collectible Objective")]
public class CollectibleObjectiveSO : ObjectiveDataSO
{
    public string ItemId;
    public int RequiredCount;
    private int currentCount = 0;

    public override void Initialize()
    {
        Debug.Log($"[CollectibleObjectiveSO] Initialized: {objectiveName}");

        // Recount how many were already collected (e.g., from save file)
        currentCount = ItemTracker.Instance.GetItemCount(ItemId);
        Debug.Log($"[CollectibleObjectiveSO] Pre-collected count: {currentCount}/{RequiredCount}");

        // Listen for future collections
        GameService.Instance.EventService.OnObjectCollected.AddListener(OnItemCollected);

        // Check if already done
        if (currentCount >= RequiredCount && AreChildrenComplete())
        {
            CompleteObjective();
            GameService.Instance.EventService.OnObjectiveCompleted.InvokeEvent(this);
        }
        
        Debug.Log(AreChildrenComplete());
    }

    public void OnItemCollected(string collectedItemId)
    {
        if (objectiveStatus != ObjectiveStatus.INPROGRESS) return;
        if (collectedItemId != ItemId) return;

        currentCount = ItemTracker.Instance.GetItemCount(ItemId);
        Debug.Log($"[{objectiveName}] Collected {currentCount}/{RequiredCount}");

        if (currentCount >= RequiredCount && AreChildrenComplete())
        {
            CompleteObjective();
            GameService.Instance.EventService.OnObjectiveCompleted.InvokeEvent(this);
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