using UnityEngine;

/// <summary>
/// Represents an interactable photograph that can be viewed in detail and plays audio when used.
/// </summary>
public class PhotoInteractable : BaseInteractable
{
    [Header("Tooltip & Info")]
    [Tooltip("Tooltip shown when hovering over the object.")]
    [SerializeField] private string tooltip = "Pick up";

    [SerializeField, Tooltip("Display name shown when in range.")]
    private string displayName = "Unknown Object";

    [TextArea(10, 15)]
    [SerializeField, Tooltip("Description shown in the viewer.")]
    private string description = "No description provided.";

    // Public read-only properties for UI
    public override string DisplayName => displayName;
    public override string Description => description;
    
    /// Called when the object is being looked at.
    public override void OnFocus()
    {
        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = Color.yellow;
        }
    }
    
    /// Called when the object loses focus.
    public override void OnLoseFocus()
    {
        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = Color.cyan;
        }
    }
    
    /// Called when the player presses the interact key.
    public override void OnInteract()
    {
        ObjectViewSwitcher switcher = FindObjectOfType<ObjectViewSwitcher>();
        if (switcher != null)
        {
            switcher.EnterPuzzleView();
        }

        SoundService.Instance.Play(Sounds.OBJECT);
        GameService.Instance?.ObjectViewer?.Show(gameObject, this);

        if (TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = Color.green;
        }
    }
    
    /// Returns the tooltip string to show in the UI.
    public override string GetTooltipText()
    {
        return string.IsNullOrEmpty(tooltip) ? displayName : tooltip;
    }
}