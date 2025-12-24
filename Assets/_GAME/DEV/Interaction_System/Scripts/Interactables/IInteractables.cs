// Interface for all interactable objects in the game world.
public interface IInteractables
{
    /// Name shown when object is within interact range.
    string DisplayName { get; }
    
    /// Description shown in the ObjectViewer panel.
    string Description { get; }

    bool IsInteractable { get; }
    /// Called when the object comes into focus (looked at).
    void OnFocus();
    
    /// Called when the object is no longer being looked at.
    void OnLoseFocus();
    
    /// Called when the object is interacted with (e.g., pressing 'E').
    void OnInteract();
    
    /// Tooltip to show in UI when hovering over the object.
    string GetTooltipText();
}