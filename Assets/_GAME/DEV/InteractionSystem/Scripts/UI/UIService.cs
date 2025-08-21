using UnityEngine;

/// <summary>
/// Manages UI elements related to player interactions.
/// Subscribes to global events to show/hide prompts.
/// </summary>
public class UIService : MonoBehaviour
{
    private EventService _eventService;
    
    /// Initializes the UI service with the EventService instance.
    public void InitializeDependencies(EventService eventService) // Fixed typo here
    {
        _eventService = eventService;
        InitializeEvents();
    }
    
    /// Subscribes to relevant events.
    private void InitializeEvents()
    {
        if (_eventService == null)
        {
            Debug.LogError("UIService: EventService reference is null.");
        }
    }

    /// Proxy method to show interaction messages.
    public void ShowMessage(string message, float duration = 1.5f)
    {
        if (DoorUIManager.Instance != null)
        {
            DoorUIManager.Instance.ShowMessage(message, duration);
        }
        else
        {
            Debug.LogWarning("DoorUIManager instance not found.");
        }
    }
}