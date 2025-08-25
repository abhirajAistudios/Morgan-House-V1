using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's inventory: adding, stacking, using, saving, and loading items.
/// </summary>
public class InventoryManager : MonoBehaviour, ISaveable
{
    #region Singleton
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            itemSlots = new InventoryItem[slotCount];
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate instances
        }
    }
    #endregion

    [Tooltip("Number of inventory slots available.")]
    public int slotCount = 5;

    [HideInInspector]
    public InventoryItem[] itemSlots;

    // Event for when inventory changes (UI can subscribe)
    public delegate void OnInventoryChanged();
    public OnInventoryChanged onInventoryChangedCallback;

    #region Inventory Management

    /// <summary>
    /// Adds an item to inventory (stacks if already exists).
    /// </summary>
    public void AddItem(ItemObject itemObject)
    {
        // Try stacking
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].itemData == itemObject)
            {
                itemSlots[i].AddQuantity(1);
                onInventoryChangedCallback?.Invoke();
                return;
            }
        }

        // Place in empty slot
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null)
            {
                itemSlots[i] = new InventoryItem(itemObject);
                onInventoryChangedCallback?.Invoke();
                return;
            }
        }

        Debug.LogWarning("Inventory Full!");
    }

    /// <summary>
    /// Uses an item from inventory (removes if quantity is zero).
    /// </summary>
    public void UseItem(ItemObject itemObject)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].itemData == itemObject)
            {
                itemSlots[i].Use();

                if (itemSlots[i].quantity <= 0)
                    itemSlots[i] = null;

                onInventoryChangedCallback?.Invoke();
                return;
            }
        }
    }

    /// <summary>
    /// Uses the item at a specific slot index.
    /// </summary>
    public void UseItemByIndex(int index)
    {
        if (index < 0 || index >= itemSlots.Length) return;

        var item = itemSlots[index];
        if (item == null) return;

        item.Use();

        if (item.quantity <= 0)
            itemSlots[index] = null;

        onInventoryChangedCallback?.Invoke();
    }

    /// <summary>
    /// Checks if the inventory contains a given item.
    /// </summary>
    public bool HasItem(ItemObject item)
    {
        foreach (var slot in itemSlots)
        {
            if (slot != null && slot.itemData == item && slot.quantity > 0)
                return true;
        }
        return false;
    }

    #endregion

    #region Save & Load

    /// <summary>
    /// Saves current inventory state to SaveData.
    /// </summary>
    public void SaveState(ref SaveData data)
    {
        if (data.inventorySlots == null)
            data.inventorySlots = new List<InventorySlotData>();
        else
            data.inventorySlots.Clear();

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null)
            {
                data.inventorySlots.Add(new InventorySlotData
                {
                    itemName = itemSlots[i].itemData.itemName,
                    quantity = itemSlots[i].quantity,
                    slotIndex = i
                });
            }
        }
    }

    /// <summary>
    /// Restores inventory from SaveData.
    /// </summary>
    public void LoadState(SaveData data)
    {
        itemSlots = new InventoryItem[slotCount]; // Reset slots

        if (data == null || data.inventorySlots == null)
        {
            Debug.LogWarning("SaveData or inventorySlots is null");
            onInventoryChangedCallback?.Invoke();
            return;
        }

        LoadItemDatabase();
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("ItemDatabase could not be loaded!");
            onInventoryChangedCallback?.Invoke();
            return;
        }

        foreach (var slotData in data.inventorySlots)
        {
            if (slotData == null) continue;

            if (slotData.slotIndex < 0 || slotData.slotIndex >= itemSlots.Length)
            {
                Debug.LogWarning($"Invalid slot index: {slotData.slotIndex}");
                continue;
            }

            ItemObject itemObj = ItemDatabase.Instance.GetItemByName(slotData.itemName);
            if (itemObj != null)
                itemSlots[slotData.slotIndex] = new InventoryItem(itemObj, slotData.quantity);
            else
                Debug.LogWarning($"Item '{slotData.itemName}' not found in database");
        }

        onInventoryChangedCallback?.Invoke();
    }

    /// <summary>
    /// Loads ItemDatabase from Resources if not already loaded.
    /// </summary>
    private void LoadItemDatabase()
    {
        if (ItemDatabase.Instance != null) return;

        ItemDatabase database = Resources.Load<ItemDatabase>("ItemDatabase");
        if (database != null)
        {
            var forceInit = database.allItems; // Force init
        }
        else
        {
            Debug.LogError("Could not load ItemDatabase from Resources!");
        }
    }

    #endregion
}
