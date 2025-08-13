using UnityEngine;
using static AutoSaveManager;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FuseBoxPuzzle : BaseInteractable, ISaveable
{
    [Header("Fuse Puzzle Setup")]
    public GameObject[] fuseSlots;
    public Renderer mainFuseRenderer;
    public Color incompleteColor = Color.red;
    public Color completeColor = Color.green;
    public string requiredItemName = "Fuse";
    public Behaviour stopInteract;

    private int fusesInserted = 0;
    public bool isSolved = false;

    [Header("Puzzle Identification")]
    [SerializeField] private string puzzleID = Guid.NewGuid().ToString();

    [Header("Interactable UI")]
    [SerializeField] private string displayName = "Fuse Box";
    [TextArea(5, 10)][SerializeField] private string description = "Insert fuses to power the system.";
    [SerializeField] private string tooltip = "Insert Fuse";
    public override bool IsInteractable => !isSolved;

    public override string DisplayName => displayName;
    public override string Description => description;
    public override string GetTooltipText() => tooltip;

    [Header("Player Reference")]
    public Transform player;

    [Header("Door to Unlock on Solve")]
    public DoorInteraction doorToUnlock; // Optional reference to a door

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
        if (!IsInteractable) return;
        TryInsertFuse();
    }

    private void TryInsertFuse()
    {
        if (fusesInserted >= fuseSlots.Length) return;

        for (int i = 0; i < InventoryManager.Instance.itemSlots.Length; i++)
        {
            var item = InventoryManager.Instance.itemSlots[i];
            if (item != null && item.itemData.itemName == requiredItemName)
            {
                InventoryManager.Instance.UseItem(item.itemData);

                fuseSlots[fusesInserted].SetActive(true);
                fusesInserted++;

                if (fusesInserted == fuseSlots.Length)
                {
                    PuzzleCompleted();
                }
                return;
            }
        }

        Debug.Log("No fuses available.");
    }

    public void TryInsertFuseAt(int slotIndex, FuseSlot fuseslot)
    {
        if (slotIndex < 0 || slotIndex >= fuseSlots.Length) return;
        if (fuseSlots[slotIndex].activeSelf) return;

        for (int i = 0; i < InventoryManager.Instance.itemSlots.Length; i++)
        {
            var item = InventoryManager.Instance.itemSlots[i];
            if (item != null && item.itemData.itemName == requiredItemName)
            {
                InventoryManager.Instance.UseItem(item.itemData);

                fuseSlots[slotIndex].SetActive(true);
                fuseslot.MarkSolved();
                fusesInserted++;

                if (fusesInserted == fuseSlots.Length)
                {
                    PuzzleCompleted();
                }
                return;
            }
        }

        Debug.Log("No fuses available.");
    }

    private void PuzzleCompleted()
    {
        if (mainFuseRenderer != null)
            mainFuseRenderer.material.color = completeColor;

        MarkSolved();
        Debug.Log($"✅ Puzzle {puzzleID} complete!");

        // Unlock the linked door (if assigned)
        if (doorToUnlock != null)
        {
            UnlockLinkedDoor();
        }

        GameProgressTracker.ObjectivesCompleted++;
        FindAnyObjectByType<AutoSaveManager>().SaveAfterObjective(player);
        OnLoseFocus();
    }

    public void MarkSolved()
    {
        isSolved = true;
        GameService.Instance.EventService.OnPuzzleSolved.InvokeEvent(displayName);
        Debug.Log($"{name} (ID: {puzzleID}) has been marked solved and is no longer interactable.");
    }

    /// <summary>
    /// Unlocks the linked door directly.
    /// </summary>
    private void UnlockLinkedDoor()
    {
        if (doorToUnlock.currentState == DoorState.Locked || doorToUnlock.currentState == DoorState.Jammed)
        {
            doorToUnlock.currentState = DoorState.Unlocked;
            Debug.Log($"🔓 Door {doorToUnlock.name} unlocked via puzzle {puzzleID}.");
        }
    }

    // ======================
    // ISaveable Implementation
    // ======================
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        PuzzleState state = new PuzzleState
        {
            puzzleID = puzzleID,
            isSolved = isSolved
        };
        data.puzzles.Add(state);
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        foreach (var state in data.puzzles)
        {
            if (state.puzzleID == puzzleID && state.isSolved)
            {
                RestoreSolvedState();
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

        // Ensure door stays unlocked on load
        if (doorToUnlock != null)
        {
            UnlockLinkedDoor();
        }

        Debug.Log($"Fusebox puzzle restored as solved (ID: {puzzleID}).");
        OnLoseFocus();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FuseBoxPuzzle))]
public class FuseBoxPuzzleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        FuseBoxPuzzle puzzle = (FuseBoxPuzzle)target;

        if (GUILayout.Button("Generate New Puzzle ID"))
        {
            Undo.RecordObject(puzzle, "Generate Puzzle ID");
            puzzle.GetType().GetField("puzzleID",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(puzzle, Guid.NewGuid().ToString());
            EditorUtility.SetDirty(puzzle);
        }
    }
}
#endif
