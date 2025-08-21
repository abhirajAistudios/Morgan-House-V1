using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A fuse box puzzle where the player must insert a required number of fuses
/// to unlock a linked door and progress.
/// </summary>
public class FuseBoxPuzzle : BaseInteractable, ISaveable
{
    [Header("Fuse Puzzle Setup")]
    [Tooltip("Fuse slot objects that will be activated as fuses are inserted.")]
    public GameObject[] fuseSlots; // Array of fuse slot objects (turned on when fuses are inserted)

    [Tooltip("Renderer for the main fuse indicator light.")]
    public Renderer mainFuseRenderer; // The light/indicator for overall fuse box status

    public Color incompleteColor = Color.red;   // Light color before puzzle is solved
    public Color completeColor = Color.green;   // Light color when puzzle is solved

    [Tooltip("The name of the required item to solve the puzzle.")]
    public string requiredItemName = "Fuse"; // What item is needed to insert into the slots

    [Header("Puzzle Identification")]
    [SerializeField] private string puzzleID = Guid.NewGuid().ToString(); // Unique ID for saving/loading
    private int fusesInserted = 0; // Tracks number of fuses placed
    private bool isSolved = false; // Has the puzzle been completed?

    [Header("Interactable UI")]
    [SerializeField] private string displayName = "Fuse Box"; // Name shown in interaction UI
    [TextArea(3, 6)]
    [SerializeField] private string description = "Insert fuses to power the system."; // Tooltip description
    [SerializeField] private string tooltip = "Insert Fuse"; // Tooltip action text

    // Expose UI values to base class
    public override bool IsInteractable => !isSolved; // Only interactable until solved
    public override string DisplayName => displayName;
    public override string Description => description;
    public override string GetTooltipText() => tooltip;

    [Header("Door to Unlock on Solve (Optional)")]
    public DoorInteraction doorToUnlock; // Optional linked door that unlocks when puzzle is solved

    private void Start()
    {
        // If puzzle not solved yet, disable fuse slot visuals and set light to "incomplete"
        if (!isSolved)
        {
            foreach (var slot in fuseSlots)
                slot.SetActive(false);

            if (mainFuseRenderer != null)
                mainFuseRenderer.material.color = incompleteColor;
        }
    }

    public override void OnFocus() { } // Optional focus highlight (not used here)
    public override void OnLoseFocus() { } // Optional focus removal (not used here)

    public override void OnInteract()
    {
        if (IsInteractable)
            TryInsertFuse(); // Insert fuse when interacted with
    }

    /// <summary>
    /// Attempts to insert a fuse into the next available slot.
    /// </summary>
    private void TryInsertFuse()
    {
        if (fusesInserted >= fuseSlots.Length) return; // No more slots left

        if (TryConsumeFuse()) // Check if player has fuse in inventory
        {
            ActivateFuseSlot(fusesInserted); // Show fuse visually
            fusesInserted++;

            if (fusesInserted == fuseSlots.Length) // All slots filled = puzzle solved
                PuzzleCompleted();
        }
    }

    /// <summary>
    /// Attempts to insert a fuse into a specific slot (for drag/drop slot logic).
    /// </summary>
    public void TryInsertFuseAt(int slotIndex, FuseSlot fuseSlot)
    {
        if (slotIndex < 0 || slotIndex >= fuseSlots.Length) return; // Invalid slot
        if (fuseSlots[slotIndex].activeSelf) return; // Already filled

        if (TryConsumeFuse()) // Consume fuse from inventory
        {
            ActivateFuseSlot(slotIndex); // Show fuse visually
            fuseSlot.MarkSolved(); // Mark slot solved
            fusesInserted++;

            if (fusesInserted == fuseSlots.Length)
                PuzzleCompleted();
        }
    }

    /// <summary>
    /// Consumes a fuse item from inventory if available.
    /// </summary>
    private bool TryConsumeFuse()
    {
        for (int i = 0; i < InventoryManager.Instance.itemSlots.Length; i++)
        {
            var item = InventoryManager.Instance.itemSlots[i];
            if (item != null && item.itemData.itemName == requiredItemName)
            {
                InventoryManager.Instance.UseItem(item.itemData); // Remove fuse
                return true;
            }
        }
        return false; // No fuse found
    }

    /// <summary>
    /// Activates a visual fuse slot.
    /// </summary>
    private void ActivateFuseSlot(int index)
    {
        if (index < 0 || index >= fuseSlots.Length) return;
        fuseSlots[index].SetActive(true);
    }

    /// <summary>
    /// Called when all fuses have been inserted (puzzle complete).
    /// </summary>
    private void PuzzleCompleted()
    {
        if (mainFuseRenderer != null)
            mainFuseRenderer.material.color = completeColor; // Change indicator light to green

        MarkSolved(); // Mark puzzle as solved internally

        // Unlock linked door if assigned
        if (doorToUnlock != null)
            UnlockLinkedDoor();

        // Update game progress + autosave
        GameProgressTracker.ObjectivesCompleted++;
        var playerPos = FindAnyObjectByType<PlayerController>()?.transform;
        FindAnyObjectByType<AutoSaveManager>()?.SaveAfterObjective(playerPos);

        OnLoseFocus(); // Clear interaction highlight
    }

    /// <summary>
    /// Marks the puzzle as solved and triggers event.
    /// </summary>
    private void MarkSolved()
    {
        isSolved = true;
        GameService.Instance.EventService.OnPuzzleSolved.InvokeEvent(displayName);
    }

    /// <summary>
    /// Unlocks a linked door if it’s set to require this puzzle.
    /// </summary>
    private void UnlockLinkedDoor()
    {
        if (doorToUnlock.currentState == DoorState.FuseLockDoor)
            doorToUnlock.currentState = DoorState.Unlocked;
    }

    #region Save/Load
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        data.puzzles.Add(new AutoSaveManager.PuzzleState
        {
            puzzleID = puzzleID,
            isSolved = isSolved
        });
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        foreach (var state in data.puzzles)
        {
            if (state.puzzleID == puzzleID && state.isSolved)
            {
                RestoreSolvedState();
                break;
            }
        }
    }

    /// <summary>
    /// Restores puzzle to solved state on load.
    /// </summary>
    private void RestoreSolvedState()
    {
        MarkSolved();

        if (mainFuseRenderer != null)
            mainFuseRenderer.material.color = completeColor;

        foreach (var slot in fuseSlots)
            slot.SetActive(true); // Show all slots filled

        fusesInserted = fuseSlots.Length; // Match solved state

        if (doorToUnlock != null)
            UnlockLinkedDoor();

        OnLoseFocus();
    }
    #endregion
}

#if UNITY_EDITOR
/// <summary>
/// Custom inspector for FuseBoxPuzzle to regenerate puzzleID manually.
/// </summary>
[CustomEditor(typeof(FuseBoxPuzzle))]
public class FuseBoxPuzzleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var puzzle = (FuseBoxPuzzle)target;

        if (GUILayout.Button("Generate New Puzzle ID"))
        {
            Undo.RecordObject(puzzle, "Generate Puzzle ID");

            // Uses reflection to set private field puzzleID
            puzzle.GetType()
                  .GetField("puzzleID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                  .SetValue(puzzle, Guid.NewGuid().ToString());

            EditorUtility.SetDirty(puzzle); // Mark object as modified
        }
    }
}
#endif
