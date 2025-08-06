using UnityEngine;
using System;

public class ObjectPickerInteractable : BaseInteractable, ISaveable
{
    [Header("Pickup Settings")]
    public ItemObject item;

    [Header("Unique ID")]
    [SerializeField] private string uniqueID;

    [SerializeField] private string ItemName;

    private bool isCollected = false;

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = Guid.NewGuid().ToString();
    }

    private void Start()
    {
        if (ItemTracker.Instance.HasCollected(ItemName))
        {
            isCollected = true;
            gameObject.SetActive(false);
        }
    }

    public override string GetTooltipText() => ItemName;
    public override string DisplayName => item != null ? item.itemName : "Unknown";
    public override string Description => item != null ? item.description : "No description";

    public override void OnInteract()
    {
        if (!IsInteractable || isCollected) return;

        InventoryManager.Instance.AddItem(item);
        ItemTracker.Instance.OnPlayerCollectedItem(ItemName); // Track collection FIRST

        isCollected = true;
        gameObject.SetActive(false);
        // Force Objective to update NOW
        ObjectiveManager.Instance.OnObjectiveUpdatedImmediately();

        Debug.Log($"Picked up: {item.itemName}");
    }


    
    // ISaveable Implementation
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        if (isCollected && !data.collectedItems.Contains(uniqueID))
        {
            data.collectedItems.Add(uniqueID);
            Debug.Log(" Saved collected item with ID: " + uniqueID);
        }
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        if (data.collectedItems.Contains(uniqueID))
        {
            isCollected = true;
            gameObject.SetActive(false);
            Debug.Log(" Restored collected item with ID: " + uniqueID);
        }
    }
}
