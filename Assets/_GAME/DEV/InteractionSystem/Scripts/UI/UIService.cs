using TMPro;
using UnityEngine;

/// <summary>
/// Manages UI elements related to player interactions.
/// Subscribes to global events to show/hide prompts.
/// </summary>
public class UIService : MonoBehaviour
{
    private EventService _eventService;
    

    /// <summary>
    /// Initializes the UI service with the EventService instance.
    /// </summary>
    public void InitializeDependencies(EventService eventService) // Fixed typo here
    {
        _eventService = eventService;
        InitializeEvents();
    }

    /// <summary>
    /// Subscribes to relevant events.
    /// </summary>
    private void InitializeEvents()
    {
        if (_eventService == null)
        {
            Debug.LogError("UIService: EventService reference is null.");
            return;
        }
    }
    // <summary>
    /// Proxy method to show interaction messages.
    /// </summary>
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

    public void ShowTooltip(string message, float duration = 2f)
    {
        DoorUIManager.Instance.ShowMessage(message, duration);
    }
}