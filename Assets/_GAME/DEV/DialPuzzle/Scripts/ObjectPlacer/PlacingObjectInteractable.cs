using System;
using UnityEngine;

public class PlacingObjectInteractable : BaseInteractable, ISaveable
{
    private bool isInteracted = false;
    private DialPuzzleController controller;

    [Header("UI Feedback")]
    public GameObject noObjectText;

    public override bool IsInteractable => !isInteracted;

    [Header("Required Item")]
    public string requiredItemName = "Dial Puzzle Object";
    public GameObject placedObject;

    [Header("Info Text Delay")]
    public float delay = 1.5f;

    private float timer = 0f;
    private bool started = false;

    [Header("Unique ID for Saving")]
    [SerializeField] private string uniqueID;

    private void Awake()
    {
        controller = GetComponentInParent<DialPuzzleController>();
    }
    private void OnValidate()
    {
        // Generate ID automatically if missing
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = Guid.NewGuid().ToString();
    }

    private void Update()
    {
        DisableInfoText();
    }

    public override void OnFocus()
    {
        Debug.Log($"[{name}] OnFocus");
    }

    public override void OnLoseFocus()
    {
        Debug.Log($"[{name}] OnLoseFocus");
    }

    public override void OnInteract()
    {
        base.OnInteract(); // Handles isReusable check

        if (!IsInteractable)
        {
            Debug.Log($"[{name}] Already used or not interactable.");
            return;
        }

        // Check if the required item exists in inventory
        InventoryItem matchingSlot = null;
        foreach (var slot in InventoryManager.Instance.itemSlots)
        {
            if (slot != null && slot.itemData.itemName == requiredItemName)
            {
                matchingSlot = slot;
                break;
            }
        }

        if (matchingSlot == null)
        {
            noObjectText.SetActive(true);
            StartDisableCountdown();
            Debug.LogWarning($"Required item '{requiredItemName}' not found in inventory.");
            return;
        }

        // Use the item from inventory
        InventoryManager.Instance.UseItem(matchingSlot.itemData);

        // Place the object
            GameService.Instance.ObjectPlacer.AddThisObjectInHolder(gameObject,controller);
            Debug.LogWarning("GameService or ObjectPlacer assigned.");
            placedObject.SetActive(true);
            enabled = false;

            if (TryGetComponent<Renderer>(out var renderer))
                renderer.material.color = Color.green;

            isInteracted = true;
    }

    public void DisableInfoText()
    {
        if (!started || noObjectText == null) return;

        timer += Time.deltaTime;
        if (timer >= delay)
        {
            noObjectText.SetActive(false);
            started = false;
        }
    }

    public void StartDisableCountdown()
    {
        timer = 0f;
        started = true;
    }

    // =========================
    // Save / Load Functionality
    // =========================
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        // If already placed, save its unique ID
        if (isInteracted && !data.collectedItems.Contains(uniqueID))
        {
            data.collectedItems.Add(uniqueID);
        }
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        if (data.collectedItems.Contains(uniqueID))
        {
            // Restore as already placed
            isInteracted = true;
            placedObject.SetActive(true);
            enabled = false;

            if (TryGetComponent<Renderer>(out var renderer))
                renderer.material.color = Color.green;

            Debug.Log($"Restored placed object: {name} (ID: {uniqueID})");
        }
    }
}