/// <summary>
/// Interface for all interactable objects in the game world.
/// </summary>
public interface IInteractables
{
    /// <summary>
    /// Name shown when object is within interact range.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Description shown in the ObjectViewer panel.
    /// </summary>
    string Description { get; }

    bool IsInteractable { get; }
    /// <summary>
    /// Called when the object comes into focus (looked at).
    /// </summary>
    void OnFocus();

    /// <summary>
    /// Called when the object is no longer being looked at.
    /// </summary>
    void OnLoseFocus();

    /// <summary>
    /// Called when the object is interacted with (e.g., pressing 'E').
    /// </summary>
    void OnInteract();

    /// <summary>
    /// Tooltip to show in UI when hovering over the object.
    /// </summary>
    string GetTooltipText();
}