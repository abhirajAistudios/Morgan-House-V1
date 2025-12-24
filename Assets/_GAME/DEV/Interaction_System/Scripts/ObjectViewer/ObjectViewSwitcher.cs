using UnityEngine;

/// <summary>
/// Switches between the main gameplay camera and the Object interaction camera/view.
/// Also handles enabling/disabling UI and player controls during Object interaction.
/// </summary>
public class ObjectViewSwitcher : MonoBehaviour
{
    public Camera objectCamera;            // The camera used to focus on the Object
    public GameObject viewerUI;
    public GameObject playerController;    // Player movement/interaction script or container

    private bool inObjectView = false;         // Tracks whether the player is currently in the Object view

    /// <summary>
    /// Activates the Object view:
    /// - Switches camera
    /// - Enables Object UI
    /// - Disables player movement
    /// - Unlocks the cursor
    /// </summary>
    public void EnterObjectView()
    {
        inObjectView = true;
        
        objectCamera.enabled = true;

        // Unlock and show the cursor for UI interaction
        viewerUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Enable Object-specific UI and disable player control
        playerController.SetActive(false);
    }

    /// <summary>
    /// Returns to the main game view:
    /// - Switches back to gameplay camera
    /// - Disables Object UI
    /// - Enables player movement
    /// - Locks the cursor
    /// </summary>
    public void ExitObjectView()
    {
        // Disable Object UI and re-enable player controls
        playerController.SetActive(true);
        
        viewerUI.SetActive(false);
        inObjectView = false;
        
        objectCamera.enabled = false;

        // Lock and hide the cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}