using UnityEngine;

/// <summary>
/// Manages UI elements related to player interactions.
/// Subscribes to global events to show/hide prompts.
/// </summary>
public class UIService : GenericMonoSingleton<UIService>
{
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