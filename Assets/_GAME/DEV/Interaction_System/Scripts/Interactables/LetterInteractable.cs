using UnityEngine;

/// <summary>
/// Represents a letter or document that can be picked up and inspected.
/// Supports highlighting and opens the object viewer on interaction.
/// </summary>
public class LetterInteractable : BaseInteractable
{
    [Header("Letter Settings")]
    [Tooltip("Tooltip shown when hovering over this object.")]
    [SerializeField] private string tooltip = "Pick up";

    [Tooltip("The name of the letter shown in the UI.")]
    [SerializeField] private string displayName = "Letter";

    [Tooltip("The detailed description shown in the object viewer.")]
    [TextArea(8, 15)]
    [SerializeField] private string description = "No description available.";

    [Tooltip("Optional material color on focus.")]
    [SerializeField] private Color focusColor = Color.yellow;

    [Tooltip("Optional material color on unfocus.")]
    [SerializeField] private Color unfocusColor = Color.cyan;

    // Provide info to the UI system
    public override string DisplayName => displayName;
    public override string Description => description;
    public override string GetTooltipText() => !string.IsNullOrEmpty(tooltip) ? tooltip : displayName;

    public override void OnFocus()
    {
        base.OnFocus();

        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = focusColor;
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = unfocusColor;
        }
    }

    public override void OnInteract()
    {
        base.OnInteract(); // sets hasBeenUsed if !isReusable

        if (!IsInteractable)
        {
            return;
        }

        // Open object viewer
        GameService.Instance.ObjectViewer.Show(gameObject, this);

        // Play sound if available
        GameService.Instance.SoundService?.Play(Sounds.OBJECT);
        
        // Optional: switch view (only if needed)
        ObjectViewSwitcher switcher = FindObjectOfType<ObjectViewSwitcher>();
        switcher?.EnterPuzzleView();
    }
}