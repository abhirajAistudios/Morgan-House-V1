using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Objective System/Collectible Objective")]
public class CollectibleObjectiveSO : ObjectiveDataSO
{
    public string ItemId;
    public int RequiredCount;
    private int currentCount = 0;

    public override void Initialize()
    {
        currentCount = 0;
        Debug.Log($"[Collectible Init] {objectiveName}");
        GameService.Instance.EventService.OnDialPuzzleObjectCollected.AddListener(OnItemCollected);
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