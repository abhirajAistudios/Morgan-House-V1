using UnityEngine;
using System;

public class DialPuzzleObjectInteractable : BaseInteractable, ISaveable
{
    public ItemObject item;
    public string ItemId;

    [Header("UI Info")]
    [Tooltip("Tooltip text when aiming at the object.")]
    [SerializeField] private string tooltip = "Use";

    [Tooltip("Display name shown in tooltip and UI.")]
    [SerializeField] private string displayName = "Switch";

    [Tooltip("Optional description shown in viewer or logs.")]
    [TextArea(5, 10)]
    [SerializeField] private string description = "A mechanical switch. Might activate something nearby.";

    [Header("Visuals")]
    [SerializeField] private Color focusColor = Color.yellow;
    [SerializeField] private Color unfocusColor = Color.cyan;
    [SerializeField] private Color usedColor = Color.green;

    [Header("Unique ID")]
    [SerializeField] private string uniqueID; // stays the same across saves

    private bool isCollected = false;

    public override string DisplayName => displayName;
    public override string Description => description;
    public override string GetTooltipText() => !string.IsNullOrEmpty(tooltip) ? tooltip : displayName;

    private void OnValidate()
    {
        // Auto-generate ID once in editor
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = Guid.NewGuid().ToString();
    }

    public override void OnFocus()
    {
        base.OnFocus();
        if (TryGetComponent<Renderer>(out var renderer))
            renderer.material.color = focusColor;
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
        if (TryGetComponent<Renderer>(out var renderer))
            renderer.material.color = unfocusColor;
    }
     /// <summary>
     /// umnbu dsaghfdasuihdfasd asdasdas da
     /// 
     /// </summary>
    public override void OnInteract()
    {
        base.OnInteract(); // Handles isReusable and hasBeenUsed

        if (!IsInteractable || isCollected)
        {
            Debug.Log($"[{name}] Already collected or locked.");
            return;
        }

        if (item == null || InventoryManager.Instance == null)
        {
            Debug.LogWarning("Missing item or InventoryManager!");
            return;
        }

        // Add item to inventory
        InventoryManager.Instance.AddItem(item);
        GameService.Instance.EventService.OnDialPuzzleObjectCollected.InvokeEvent(ItemId);
        Debug.Log($"Picked up: {item.itemName}");

        hasBeenUsed = true;
        isCollected = true;
        gameObject.SetActive(false); // Hide instead of destroy (so we can restore state properly)
    }

    // ----------------------
    // Save/Load Integration
    // ----------------------

    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        if (isCollected && !data.collectedItems.Contains(uniqueID))
        {
            data.collectedItems.Add(uniqueID);
            Debug.Log($"[SAVE] DialPuzzle {uniqueID} marked as collected.");
        }
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        if (data.collectedItems.Contains(uniqueID))
        {
            isCollected = true;
            gameObject.SetActive(false);
            Debug.Log($"[LOAD] DialPuzzle {uniqueID} restored as already collected.");
        }
    }
}