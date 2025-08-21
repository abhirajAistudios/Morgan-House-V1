using UnityEngine;

/// <summary>
/// Interactable object that lets the player enter a puzzle view.
/// </summary>
public class DialPuzzleInteractable : BaseInteractable
{
    [Header("Tooltip & Info")]
    [Tooltip("Text shown when hovering over the interactable")]
    [SerializeField] private string tooltip = "Enter Puzzle";
    
    [Tooltip("Display name for this puzzle")]
    [SerializeField] private string displayName = "Puzzle Base";
    
    [Tooltip("Detailed description of the puzzle")]
    [TextArea][SerializeField] private string description = "Interact to enter the puzzle view.";
    
    // Reference to the puzzle controller that manages this interactable
    private DialPuzzleController controller;

    // Tracks if this puzzle has been solved
    public bool isSolved = false;

    // Properties from BaseInteractable
    public override bool IsInteractable => !isSolved;  // Can only interact if not solved
    public override string DisplayName => displayName;  // Returns the display name
    public override string Description => description;  // Returns the description

    private void Awake()
    {
        if (controller == null)
            controller = GetComponentInParent<DialPuzzleController>();
    }
    
    /// Called when the player looks at this interactable
    public override void OnFocus()
    {
        //Place your code to run when the player looks at this object
    }
    
    /// Called when the player looks away from this interactable
    public override void OnLoseFocus()
    {
        //Place your code to run when the player looks away from this object
    }

    /// <summary>
    /// Handles interaction with the puzzle object
    /// Switches to the puzzle view if not already solved
    /// </summary>
    public override void OnInteract()
    {
        if (!IsInteractable) return;

        Debug.Log($"[PuzzleInteractable] Interacted: {name}");

        if (controller != null)
        {
            // Switch to the puzzle view
            controller.dialPuzzleViewSwitcher.EnterPuzzleView();
        }
        else
        {
            Debug.LogWarning("No PuzzleViewSwitcher found on this puzzle instance.");
        }
    }
    
    /// Returns the tooltip text if the puzzle is interactable
    public override string GetTooltipText()
    {
        return IsInteractable ? tooltip : string.Empty;
    }
    
    /// Marks this puzzle as solved, making it non-interactable
    public void MarkSolved()
    {
        isSolved = true;
        Debug.Log($"{name} has been marked solved and is no longer interactable.");
    }
}