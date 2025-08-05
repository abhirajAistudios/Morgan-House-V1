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

        // Check if this item was already collected
        if (ItemTracker.Instance.HasCollected(ItemId))
        {
            currentCount = RequiredCount;
            Debug.Log($"[CollectibleObjectiveSO] Item {ItemId} was pre-collected.");
            CompleteObjective();
            return; // Already done
        }

        // Otherwise, listen for future collection events
        GameService.Instance.EventService.OnObjectCollected.AddListener(OnItemCollected);
    }



    public void OnItemCollected(string collectedItemId)
    {
        if (objectiveStatus != ObjectiveStatus.INPROGRESS) return;
        if (collectedItemId != ItemId) return;

        currentCount++;
        Debug.Log($"[{objectiveName}] Collected {currentCount}/{RequiredCount}");

        if (currentCount >= RequiredCount && AreChildrenComplete())
        {
            CompleteObjective();
        }
    }
}