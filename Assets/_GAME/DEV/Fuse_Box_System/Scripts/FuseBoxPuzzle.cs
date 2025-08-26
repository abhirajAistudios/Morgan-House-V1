using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.Events; // ✅ Needed for UnityEvent

/// <summary>
/// A fuse box puzzle where the player must insert a required number of fuses
/// to unlock a linked door and progress.
/// </summary>
public class FuseBoxPuzzle : BaseInteractable, ISaveable
{
    [Header("Fuse Puzzle Setup")]
    [Tooltip("Fuse slot objects that will be activated as fuses are inserted.")]
    [SerializeField] private GameObject[] fuseSlots;

    [Tooltip("Renderer for the main fuse indicator light.")]
    [SerializeField] private Renderer mainFuseRenderer;

    [SerializeField] private Color incompleteColor = Color.red;
    [SerializeField] private Color completeColor = Color.green;

    [Tooltip("The name of the required item to solve the puzzle.")]
    [SerializeField] private string requiredItemName = "Fuse";

    [Header("Puzzle Identification")]
    [SerializeField] private string puzzleID = Guid.NewGuid().ToString();
    private int fusesInserted = 0;
    private bool isSolved = false;

    [Header("Interactable UI")]
    [SerializeField] private string displayName = "Fuse Box";
    [TextArea(3, 6)]
    [SerializeField] private string description = "Insert fuses to power the system.";
    [SerializeField] private string tooltip = "Insert Fuse";

    // Expose UI values to base class
    public override bool IsInteractable => !isSolved;
    public override string DisplayName => displayName;
    public override string Description => description;
    public override string GetTooltipText() => tooltip;

    [Header("Puzzle Solved Events")]
    [Tooltip("Triggered when puzzle is solved.")]
    [SerializeField] private UnityEvent onPuzzleCompleted; // ✅ NEW generalized event

    private void Start()
    {
        if (!isSolved)
        {
            foreach (var slot in fuseSlots)
                slot.SetActive(false);

            if (mainFuseRenderer != null)
                mainFuseRenderer.material.color = incompleteColor;
        }
    }

    public override void OnFocus() { }
    public override void OnLoseFocus() { }

    public override void OnInteract()
    {
        if (IsInteractable)
            TryInsertFuse();
    }

    private void TryInsertFuse()
    {
        if (fusesInserted >= fuseSlots.Length) return;

        if (TryConsumeFuse())
        {
            ActivateFuseSlot(fusesInserted);
            fusesInserted++;

            if (fusesInserted == fuseSlots.Length)
                PuzzleCompleted();
        }
    }

    public void TryInsertFuseAt(int slotIndex, FuseSlot fuseSlot)
    {
        if (slotIndex < 0 || slotIndex >= fuseSlots.Length) return;
        if (fuseSlots[slotIndex].activeSelf) return;

        if (TryConsumeFuse())
        {
            ActivateFuseSlot(slotIndex);
            fuseSlot.MarkSolved();
            fusesInserted++;

            if (fusesInserted == fuseSlots.Length)
                PuzzleCompleted();
        }
    }

    private bool TryConsumeFuse()
    {
        for (int i = 0; i < InventoryManager.Instance.itemSlots.Length; i++)
        {
            var item = InventoryManager.Instance.itemSlots[i];
            if (item != null && item.itemData.itemName == requiredItemName)
            {
                InventoryManager.Instance.UseItem(item.itemData);
                return true;
            }
        }
        return false;
    }

    private void ActivateFuseSlot(int index)
    {
        if (index < 0 || index >= fuseSlots.Length) return;
        fuseSlots[index].SetActive(true);
    }

    private void PuzzleCompleted()
    {
        if (mainFuseRenderer != null)
            mainFuseRenderer.material.color = completeColor;

        MarkSolved();

        //  Call generalized UnityEvent (drag & drop actions in inspector)
        onPuzzleCompleted?.Invoke();

        // autosave
        var playerPos = FindAnyObjectByType<PlayerController>()?.transform;
        FindAnyObjectByType<AutoSaveManager>()?.SaveGame(playerPos);

        OnLoseFocus();
    }

    private void MarkSolved()
    {
        isSolved = true;
        GameService.Instance.EventService.OnPuzzleSolved.InvokeEvent(displayName);
    }
    
    #region Save/Load
    public void SaveState(ref SaveData data)
    {
        data.puzzles.Add(new PuzzleState
        {
            puzzleID = puzzleID,
            isSolved = isSolved
        });
    }

    public void LoadState(SaveData data)
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

    private void RestoreSolvedState()
    {
        MarkSolved();

        if (mainFuseRenderer != null)
            mainFuseRenderer.material.color = completeColor;

        foreach (var slot in fuseSlots)
            slot.SetActive(true);

        fusesInserted = fuseSlots.Length;
        onPuzzleCompleted?.Invoke();

        OnLoseFocus();
    }
    #endregion
}
