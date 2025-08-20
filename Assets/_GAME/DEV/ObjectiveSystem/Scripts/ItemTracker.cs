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

    /// <summary>
    /// Registers an item as collected and updates its count.
    /// </summary>
    public void RegisterItemCollection(string itemId)
    {
        if (!collectedItemCounts.ContainsKey(itemId))
        {
            collectedItemCounts[itemId] = 0;
        }

        collectedItemCounts[itemId]++;
        Debug.Log($"[ItemTracker] Collected Item: {itemId} (Total: {collectedItemCounts[itemId]})");
    }

    /// <summary>
    /// Returns true if the player has collected at least one of the given item.
    /// </summary>
    public bool HasCollected(string itemId)
    {
        return GetItemCount(itemId) > 0;
    }

    /// <summary>
    /// Returns the total number of times the specified item has been collected.
    /// </summary>
    public int GetItemCount(string itemId)
    {
        return collectedItemCounts.TryGetValue(itemId, out int count) ? count : 0;
    }

    /// <summary>
    /// Handles logic when a player collects an item.
    /// </summary>
    public void OnPlayerCollectedItem(string itemId)
    {
        RegisterItemCollection(itemId);

        // Fire global event
        GameService.Instance.EventService.OnObjectCollected.InvokeEvent(itemId);
    }
}