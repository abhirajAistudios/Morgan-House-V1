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
        // Ensure only one InventoryManager exists
        if (Instance == null)
        {
            Instance = this;
            itemSlots = new InventoryItem[slotCount]; // initialize slots
        }
        else
        {
            Destroy(gameObject); // prevent duplicates
        }
    }
    #endregion

    [Tooltip("Number of inventory slots available.")]
    public int slotCount = 5;

    [HideInInspector]
    public InventoryItem[] itemSlots;

    // Callback invoked whenever the inventory changes (UI can subscribe)
    public delegate void OnInventoryChanged();
    public OnInventoryChanged onInventoryChangedCallback;

    #region Inventory Management

    /// <summary>
    /// Adds an item to the inventory. 
    /// Stacks if it already exists, otherwise places it in an empty slot.
    /// </summary>
    public void AddItem(ItemObject itemObject)
    {
        // Try to stack if the item already exists
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].itemData == itemObject)
            {
                itemSlots[i].AddQuantity(1);
                onInventoryChangedCallback?.Invoke();
                return;
            }
        }

        // Try to put item in an empty slot
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null)
            {
                itemSlots[i] = new InventoryItem(itemObject);
                onInventoryChangedCallback?.Invoke();
                return;
            }
        }

        // Inventory is full
        Debug.LogWarning(" Inventory Full!");
    }

    /// <summary>
    /// Uses the given item (if present in inventory).
    /// </summary>
    public void UseItem(ItemObject itemObject)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].itemData == itemObject)
            {
                itemSlots[i].Use();

                // Remove if quantity reaches 0
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
    /// Saves the current inventory state to the AutoSaveManager's save data.
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
    /// Restores the inventory from saved data.
    /// Also disables collectible items already picked up in the scene.
    /// </summary>
    public void LoadState(SaveData data)
    {
        itemSlots = new InventoryItem[slotCount];

        // Restore items in their saved slots
        foreach (var slotData in data.inventorySlots)
        {
            ItemObject itemObj = ItemDatabase.Instance.GetItemByName(slotData.itemName);
            if (itemObj != null && slotData.slotIndex < itemSlots.Length)
            {
                itemSlots[slotData.slotIndex] = new InventoryItem(itemObj, slotData.quantity);

            }
        }

        
        
    }
    #endregion
}
