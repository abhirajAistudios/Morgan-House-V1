using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour, ISaveable
{
    #region Singleton
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        itemSlots = new InventoryItem[slotCount];
    }
    #endregion

    [Tooltip("Number of inventory slots you want.")]
    public int slotCount = 5;

    [HideInInspector]
    public InventoryItem[] itemSlots;

    public delegate void OnInventoryChanged();
    public OnInventoryChanged onInventoryChangedCallback;

    private void Update()
    {
        // Debug test: use item in first slot
        if (Input.GetKeyDown(KeyCode.U))
            UseItemByIndex(0);
    }

    #region Public Inventory Methods

    public void AddItem(ItemObject itemObject)
    {
        // Stack if already in inventory
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].itemData == itemObject)
            {
                itemSlots[i].AddQuantity(1);
                onInventoryChangedCallback?.Invoke();
                Debug.Log($"Stacked {itemObject.itemName} in slot {i}");
                return;
            }
        }

        // Otherwise place in empty slot
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null)
            {
                itemSlots[i] = new InventoryItem(itemObject);
                onInventoryChangedCallback?.Invoke();
                Debug.Log($"Added {itemObject.itemName} to slot {i}");
                return;
            }
        }

        Debug.LogWarning("Inventory Full!");
    }

    public void UseItem(ItemObject itemObject)
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].itemData == itemObject)
            {
                itemSlots[i].Use();

                if (itemSlots[i].quantity <= 0)
                {
                    itemSlots[i] = null;
                    Debug.Log($"{itemObject.itemName} removed.");
                }

                onInventoryChangedCallback?.Invoke();
                return;
            }
        }
    }

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

    public bool HasItem(ItemObject item)
    {
        foreach (var slot in itemSlots)
            if (slot != null && slot.itemData == item && slot.quantity > 0)
                return true;
        return false;
    }
    #endregion

    #region Save & Load

    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        data.inventorySlots.Clear();

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null)
            {
                AutoSaveManager.InventorySlotData slotData = new AutoSaveManager.InventorySlotData
                {
                    itemName = itemSlots[i].itemData.itemName,
                    quantity = itemSlots[i].quantity,
                    slotIndex = i
                };
                data.inventorySlots.Add(slotData);
            }
        }
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        itemSlots = new InventoryItem[slotCount];

        foreach (var slotData in data.inventorySlots)
        {
            ItemObject itemObj = ItemDatabase.Instance.GetItemByName(slotData.itemName);
            if (itemObj != null && slotData.slotIndex < itemSlots.Length)
            {
                itemSlots[slotData.slotIndex] = new InventoryItem(itemObj, slotData.quantity);
            }
        }

        // After restoring inventory, hide already collected items in the scene
        var collectibles = FindObjectsOfType<CollectibleItem>(true);
        foreach (var collectible in collectibles)
        {
            if (data.collectedItems.Contains(collectible.ItemName))
                collectible.gameObject.SetActive(false);
        }

        onInventoryChangedCallback?.Invoke();
    }



    #endregion
}
