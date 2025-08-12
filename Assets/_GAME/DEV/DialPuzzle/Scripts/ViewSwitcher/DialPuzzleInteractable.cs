using UnityEngine;

/// <summary>
/// Interactable object that lets the player enter a puzzle view.
/// </summary>
public class DialPuzzleInteractable : BaseInteractable
{
    [Header("Tooltip & Info")]
    [SerializeField] private string tooltip = "Enter Puzzle";
    [SerializeField] private string displayName = "Puzzle Base";
    [TextArea][SerializeField] private string description = "Interact to enter the puzzle view.";
    private DialPuzzleController controller;

    public bool isSolved = false;

    public override bool IsInteractable => !isSolved;
    public override string DisplayName => displayName;
    public override string Description => description;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponentInParent<DialPuzzleController>();
    }

    public override void OnFocus()
    {
        if (!IsInteractable) return;
        Debug.Log($"[{name}] Focused.");
    }

    public override void OnLoseFocus()
    {
        if (!IsInteractable) return;
        Debug.Log($"[{name}] Lost focus.");
    }

    public override void OnInteract()
    {
        if (!IsInteractable) return;

        Debug.Log($"[PuzzleInteractable] Interacted: {name}");

        if (controller != null)
        {
            controller.dialPuzzleViewSwitcher.EnterPuzzleView();
        }
        else
        {
            Debug.LogWarning("No PuzzleViewSwitcher found on this puzzle instance.");
        }
    }

    public override string GetTooltipText()
    {
        return IsInteractable ? tooltip : string.Empty;
    }

    public void MarkSolved()
    {
        isSolved = true;
        Debug.Log($"{name} has been marked solved and is no longer interactable.");
    }
}
