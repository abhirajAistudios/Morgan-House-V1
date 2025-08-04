using UnityEngine;
using System;

public class FuseSlot : BaseInteractable, ISaveable
{
    [SerializeField] private FuseBoxPuzzle parentPuzzle;
    [SerializeField] private int slotIndex;
    private bool isPlaced = false;

    [Header("Unique ID")]
    [SerializeField] private string uniqueID; // per-slot unique ID

    public override bool IsInteractable => !isPlaced;

    public override string DisplayName => "Fuse Slot";
    public override string Description => "Insert fuse here.";
    public override string GetTooltipText() => "Insert Fuse";

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = Guid.NewGuid().ToString(); // generate unique ID once in editor
    }

    public override void OnFocus()
    {
        // Optional highlight
        // GetComponent<Renderer>().material.color = Color.yellow;
    }

    public override void OnLoseFocus()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }

    public override void OnInteract()
    {
        if (IsInteractable == false) return;
        parentPuzzle.TryInsertFuseAt(slotIndex, this);
    }

    public void MarkSolved()
    {
        isPlaced = true;
        Debug.Log($"{name} (ID: {uniqueID}) has been marked solved and is no longer interactable.");
    }

    // ----------------
    // Save / Load
    // ----------------
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        if (isPlaced && !data.collectedItems.Contains(uniqueID))
            data.collectedItems.Add(uniqueID);
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        if (data.collectedItems.Contains(uniqueID))
        {
            isPlaced = true;
            // visually update slot (so it shows fuse already placed)
            if (TryGetComponent<Renderer>(out var renderer))
                renderer.material.color = Color.white;

            Debug.Log($"FuseSlot restored as placed (ID: {uniqueID})");
        }
    }
}
