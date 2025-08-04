using UnityEngine;

/// <summary>
/// Switches between the main gameplay camera and the puzzle interaction camera/view.
/// Also handles enabling/disabling UI and player controls during puzzle interaction.
/// </summary>
public class LockPickCameraSwitcher : MonoBehaviour
{
    public Camera mainCamera;              // The main gameplay camera
    public Camera puzzleCamera;            // The camera used to focus on the puzzle
    public Camera dialPuzzleCamera;
    public GameObject puzzleUI;            // UI elements specific to the puzzle view
    public GameObject playerController;    // Player movement/interaction script or container

    private bool inPuzzle = false;         // Tracks whether the player is currently in the puzzle view

    /// <summary>
    /// Activates the puzzle view:
    /// - Switches camera
    /// - Enables puzzle UI
    /// - Disables player movement
    /// - Unlocks the cursor
    /// </summary>
    public void EnterLockPinView()
    {
        if (inPuzzle) return;
        inPuzzle = true;

        // Switch to puzzle camera
        dialPuzzleCamera.enabled = false;
        mainCamera.enabled = false;
        puzzleCamera.enabled = true;

        // Unlock and show the cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Enable puzzle-specific UI and disable player control
        puzzleUI.SetActive(true);
        playerController.SetActive(false);
    }

    /// <summary>
    /// Returns to the main game view:
    /// - Switches back to gameplay camera
    /// - Disables puzzle UI
    /// - Enables player movement
    /// - Locks the cursor
    /// </summary>
    public void ExitPuzzleView()
    {
        if (!inPuzzle) return;
        inPuzzle = false;

        // Switch back to main camera
        mainCamera.enabled = true;
        dialPuzzleCamera.enabled = true;
        puzzleCamera.enabled = false;

        // Disable puzzle UI and re-enable player controls
        puzzleUI.SetActive(false);
        playerController.SetActive(true);

        // Lock and hide the cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}