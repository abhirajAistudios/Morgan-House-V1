using UnityEngine;
using System;
using UnityEditor;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine.Events;
using UnityEngine.Rendering.HighDefinition; // ✅ Needed for UnityEvent

/// <summary>
/// A fuse box puzzle where the player must insert a required number of fuses
/// to unlock a linked door and progress.
/// </summary>
public class FuseBoxPuzzle : BaseInteractable, ISaveable
{
    [Header("Fuse Puzzle Setup")]
    [Tooltip("Fuse slot objects that will be activated as fuses are inserted.")]
    [SerializeField] private GameObject[] fuseSlots;
  
    [Tooltip("The name of the required item to solve the puzzle.")]
    [SerializeField] private string requiredItemName = "Fuse";
    
    [SerializeField] private Renderer lightColor;
    
    [SerializeField] private Material incompleteColor;
    [SerializeField] private Material completeColor;

    [Header("Puzzle Identification")]
    [SerializeField] private string puzzleID = Guid.NewGuid().ToString();
    private int fusesInserted = 0;
    private bool isSolved = false;

    [Header("Interactable UI")]
    [SerializeField] private string displayName = "Fuse Box";
    [TextArea(3, 6)]
    [SerializeField] private string description = "Insert fuses to power the system.";
    [SerializeField] private string tooltip = "Insert Fuse";

    [Header("Sounds")]
    [SerializeField] private Sounds fuseInserted;
    [SerializeField] private Sounds puzzleSolved;
    [SerializeField] private AudioSource generatorOnSound;

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
            {
                slot.SetActive(false);
            }

            generatorOnSound.Stop();
            lightColor.material = incompleteColor;
        }
    }

    private void Update()
    {
        UpdateGeneratorSound();
    }

    private void UpdateGeneratorSound()
    {
        if (Time.timeScale != 0.0f)
        {
            generatorOnSound.enabled = true;
            if (isSolved)
            {
                if (generatorOnSound.isPlaying) return;
                generatorOnSound.Play();
            }
        }
        else if(Time.timeScale == 0.0f)
        {
            generatorOnSound.enabled = false;
        }
    }

    public override void OnFocus() { }
    public override void OnLoseFocus() { }

    public void TryInsertFuseAt(int slotIndex, FuseSlot fuseSlot)
    {
        if (slotIndex < 0 || slotIndex >= fuseSlots.Length) return;
        if (fuseSlots[slotIndex].activeSelf) return;

        if (TryConsumeFuse())
        {
            ActivateFuseSlot(slotIndex);
            fuseSlot.MarkSolved();
            SoundService.Instance.Play(fuseInserted);
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
        MarkSolved();
        
        generatorOnSound.Play();
        SoundService.Instance.Play(puzzleSolved);
        lightColor.material = completeColor;
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
        EventService.Instance.OnPuzzleSolved.InvokeEvent(displayName);
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

        foreach (var slot in fuseSlots)
            slot.SetActive(true);
        
        generatorOnSound.Play();    
        lightColor.material = completeColor;
        fusesInserted = fuseSlots.Length;
        onPuzzleCompleted?.Invoke();

        OnLoseFocus();
    }
    #endregion
}
