using System.Collections.Generic;
using UnityEngine;

public class ItemTracker : MonoBehaviour
{
    public static ItemTracker Instance { get; private set; }

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

    /// Registers an item as collected and updates its count.
    public void RegisterItemCollection(string itemId)
    {
        if (!collectedItemCounts.ContainsKey(itemId))
        {
            collectedItemCounts[itemId] = 0;
        }

        collectedItemCounts[itemId]++;
    }

    /// Returns true if the player has collected at least one of the given item.
    public bool HasCollected(string itemId)
    {
        return GetItemCount(itemId) > 0;
    }
    
    /// Returns the total number of times the specified item has been collected.
    public int GetItemCount(string itemId)
    {
        return collectedItemCounts.TryGetValue(itemId, out int count) ? count : 0;
    }
    
    /// Handles logic when a player collects an item.
    public void OnPlayerCollectedItem(string itemId)
    {
        RegisterItemCollection(itemId);
        
        EventService.Instance.OnObjectCollected.InvokeEvent(itemId);
    }
}