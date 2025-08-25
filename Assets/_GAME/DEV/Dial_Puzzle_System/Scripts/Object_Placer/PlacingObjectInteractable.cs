using System;
using UnityEngine;

/// Represents an interactable object that can be placed in the world when the player has the required item.
public class PlacingObjectInteractable : BaseInteractable, ISaveable
{
    private bool isInteracted = false;           // Tracks if this object has been interacted with
    private DialPuzzleController controller;     // Reference to the parent dial puzzle controller

    [Header("UI Feedback")]
    [Tooltip("UI element to show when player doesn't have the required item")]
    public GameObject noObjectText;
    public override bool IsInteractable => !isInteracted;

    [Header("Required Item")]
    [Tooltip("Name of the item required from inventory to place this object")]
    public string requiredItemName = "Dial Puzzle Object";
    
    [Tooltip("The object to be placed when interacting with this item")]
    public GameObject placedObject;

    [Header("Info Text Delay")]
    [Tooltip("How long to display the info text before hiding it (in seconds)")]
    public float delay = 1.5f;

    private float timer = 0f;        // Tracks time for the info text display
    private bool started = false;    // Flag to start the countdown for hiding the info text

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
    
    /// Called when the player looks at this interactable object
    public override void OnFocus()
    {
        //Place your code to run when the player looks at this object
    }
    
    /// Called when the player looks away from this interactable object
    public override void OnLoseFocus()
    {
        //Place your code to run when the player looks away from this object
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
    
    /// Handles the countdown to disable the info text after a delay
    private void DisableInfoText()
    {
        if (!started || noObjectText == null) return;

        timer += Time.deltaTime;
        if (timer >= delay)
        {
            noObjectText.SetActive(false);
            started = false;
        }
    }
    
    /// Starts the countdown to hide the info text
    private void StartDisableCountdown()
    {
        timer = 0f;
        started = true;
    }
    

    /// Saves the current state of this interactable object
    public void SaveState(ref SaveData data)
    {
        // If already placed, save its unique ID
        if (isInteracted && !data.collectedItems.Contains(uniqueID))
        {
            data.collectedItems.Add(uniqueID);
        }
    }
    
    /// Loads the saved state of this interactable object
    public void LoadState(SaveData data)
    {
        if (data.collectedItems.Contains(uniqueID))
        {
            // Restore as already placed
            isInteracted = true;
            placedObject.SetActive(true);
            enabled = false;

            if (TryGetComponent<Renderer>(out var renderer))
                renderer.material.color = Color.green;
        }
    }
}