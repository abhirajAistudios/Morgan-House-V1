using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [Tooltip("All items available in the game.")]
    public ItemObject[] allItems;

    public static ItemDatabase Instance { get; private set; }

    // Internal dictionary for fast item lookups
    private Dictionary<string, ItemObject> itemLookup;

    private void OnEnable()
    {
        // Ensure only one active instance
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple ItemDatabase instances found. Using the latest one.");
        }
        Instance = this;

        // Build lookup dictionary
        BuildItemLookup();
    }

    /// <summary>
    /// Builds a dictionary for quick item name -> ItemObject access.
    /// </summary>
    private void BuildItemLookup()
    {
        itemLookup = new Dictionary<string, ItemObject>();

        if (allItems == null) return;

        foreach (var item in allItems)
        {
            if (item == null) continue;

            if (!itemLookup.ContainsKey(item.itemName))
                itemLookup.Add(item.itemName, item);
            else
                Debug.LogWarning($"Duplicate item name '{item.itemName}' found in ItemDatabase.");
        }
    }

    /// <summary>
    /// Gets an item by its name.
    /// </summary>
    public ItemObject GetItemByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        if (itemLookup != null && itemLookup.TryGetValue(name, out var item))
            return item;

        Debug.LogWarning($"⚠ Item '{name}' not found in ItemDatabase.");
        return null;
    }
}
