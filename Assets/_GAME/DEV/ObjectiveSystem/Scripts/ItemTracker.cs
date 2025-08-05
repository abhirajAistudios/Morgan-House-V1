using System.Collections.Generic;
using UnityEngine;
public class ItemTracker : MonoBehaviour
{
    public static ItemTracker Instance;

    private HashSet<string> collectedItemIds = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterItemCollection(string itemId)
    {
        collectedItemIds.Add(itemId);
        Debug.Log($"[ItemTracker] Collected Item: {itemId}");
    }

    public bool HasCollected(string itemId)
    {
        return collectedItemIds.Contains(itemId);
    }
    
    public void OnPlayerCollectedItem(string itemId)
    {
        RegisterItemCollection(itemId);

        // Additionally, check if any active objective cares about this
        foreach (var objective in ObjectiveManager.Instance.activeObjectives)
        {
            if (objective is CollectibleObjectiveSO collectibleObjective)
            {
                collectibleObjective.OnItemCollected(itemId);
            }
        }
    }
}