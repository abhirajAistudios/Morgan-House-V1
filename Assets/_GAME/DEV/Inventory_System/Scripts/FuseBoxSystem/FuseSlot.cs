using UnityEngine;
using System;

public class FuseSlot : BaseInteractable, ISaveable
{
    [SerializeField] private FuseBoxPuzzle parentPuzzle;   // Reference to the parent FuseBoxPuzzle that manages this slot
    [SerializeField] private int slotIndex;                // Index of this slot in the puzzle
    private bool isPlaced = false;                         // Tracks if a fuse has already been placed

    [Header("Unique ID")]
    [SerializeField] private string uniqueID;              // Unique ID for this slot (used in saving/loading)

    // Whether the slot can currently be interacted with (only if fuse not placed)
    public override bool IsInteractable => !isPlaced;

    // Name shown in interaction prompts
    public override string DisplayName => "Fuse Slot";

    // Description shown in interaction prompts
    public override string Description => "Insert fuse here.";

    // Tooltip text shown on hover
    public override string GetTooltipText() => "Insert Fuse";

    // Called in editor whenever script values change (for auto-assigning unique IDs)
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = Guid.NewGuid().ToString(); // Assign a new unique ID if none exists
    }

    // Highlight when player looks at the slot
    public override void OnFocus()
    {
        // Example highlight effect (currently disabled)
        // GetComponent<Renderer>().material.color = Color.yellow;
    }

    // Reset highlight when player looks away
    public override void OnLoseFocus()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }

    // Called when player interacts with this slot
    public override void OnInteract()
    {
        if (IsInteractable == false) return;               // Ignore if slot already has fuse
        parentPuzzle.TryInsertFuseAt(slotIndex, this);     // Ask the parent puzzle to insert a fuse here
    }

    // Marks this slot as solved (fuse placed successfully)
    public void MarkSolved()
    {
        isPlaced = true;
    }

    // ----------------
    // Save / Load System
    // ----------------

    // Save current state into SaveData
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        // If fuse is placed and not already saved, add uniqueID to collected items
        if (isPlaced && !data.collectedItems.Contains(uniqueID))
            data.collectedItems.Add(uniqueID);
    }

    // Load saved state from SaveData
    public void LoadState(AutoSaveManager.SaveData data)
    {
        // If this slot's ID exists in saved data, mark it as solved
        if (data.collectedItems.Contains(uniqueID))
        {
            isPlaced = true;

            // Visually show that the fuse is already placed
            if (TryGetComponent<Renderer>(out var renderer))
                renderer.material.color = Color.white;
        }
    }
}
