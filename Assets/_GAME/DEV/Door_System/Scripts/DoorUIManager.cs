using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Singleton manager for displaying door interaction UI messages,
/// with support for timed auto-hide.
/// </summary>
public class DoorUIManager : MonoBehaviour
{
    #region Singleton Setup

    public static DoorUIManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        HideMessage(); // Ensure UI starts hidden
    }

    #endregion

    #region UI References

    [SerializeField] private TextMeshProUGUI interactionText;

    #endregion

    #region Internal Logic

    private Coroutine hideRoutine;

    /// <summary>
    /// Displays the given message in the UI. Optionally hides after a duration.
    /// </summary>
    /// <param name="message">Text to show</param>
    /// <param name="duration">How long before it disappears (0 = stays)</param>
    public void ShowMessage(string message, float duration = 0f)
    {
        interactionText.gameObject.SetActive(true);
        interactionText.text = message;

        // Stop previous hide timer if one is running
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }

        // Start new hide timer if duration is specified
        if (duration > 0f)
        {
            hideRoutine = StartCoroutine(HideAfterDelay(duration));
        }
    }

    /// <summary>
    /// Hides the message after a delay.
    /// </summary>
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideMessage();
    }

    /// <summary>
    /// Immediately hides the message and clears text.
    /// </summary>
    public void HideMessage()
    {
        interactionText.gameObject.SetActive(false);
        interactionText.text = "";
    }

    #endregion
}
