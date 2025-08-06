using System.Collections.Generic;
using UnityEngine;

public class ItemTracker : MonoBehaviour
{
    public static ItemTracker Instance;

    private Dictionary<string, int> collectedItemCounts;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        collectedItemCounts = new Dictionary<string, int>();
    }

    public void RegisterItemCollection(string itemId)
    {
        if (!collectedItemCounts.ContainsKey(itemId))
            collectedItemCounts[itemId] = 0;

        collectedItemCounts[itemId]++;
        Debug.Log($"[ItemTracker] Collected Item: {itemId} (Total: {collectedItemCounts[itemId]})");
    }

    public bool HasCollected(string itemId)
    {
        return collectedItemCounts.ContainsKey(itemId) && collectedItemCounts[itemId] > 0;
    }

    public int GetItemCount(string itemId)
    {
        if (collectedItemCounts.ContainsKey(itemId))
            return collectedItemCounts[itemId];
        return 0;
    }

    public void OnPlayerCollectedItem(string itemId)
    {
        RegisterItemCollection(itemId);

        // Fire global event if needed
        GameService.Instance.EventService.OnObjectCollected.InvokeEvent(itemId);
    }
}