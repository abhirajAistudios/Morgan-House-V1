using UnityEngine;

/// Manages the camera and UI switching between the main game view and the dial puzzle view.
public class DialPuzzleViewSwitcher : MonoBehaviour
{
    [Header("Puzzle References")]
    [Tooltip("The main player camera used during normal gameplay")]
    public Camera mainCamera;      // Assign the global player camera
    [Tooltip("The camera that shows the puzzle view")]
    public Camera puzzleCamera;    // Assign THIS puzzle’s camera
    [Tooltip("The UI canvas specific to this puzzle")]
    public GameObject puzzleUI;    // Assign THIS puzzle’s UI canvas
    [Tooltip("The player controller object to disable during puzzle interaction")]
    public GameObject playerController;

    // Tracks whether we're currently in puzzle view
    private bool inPuzzle = false;
    private GameObject objectiveCanvas;
    
    private void Start()
    {
        objectiveCanvas = GameObject.FindWithTag("Objective Canvas");
    }

    /// <summary>
    /// Switches from main game view to puzzle view
    /// - Enables the puzzle camera
    /// - Disables the main camera
    /// - Shows the puzzle UI
    /// - Disables player movement
    /// - Unlocks and shows the cursor
    /// </summary>
    public void EnterPuzzleView()
    {
        if (inPuzzle) return;
        inPuzzle = true;
        
        objectiveCanvas.SetActive(false);

        // Switch to puzzle camera
        puzzleCamera.gameObject.SetActive(true);
        mainCamera.enabled = false;
        puzzleCamera.enabled = true;

        // Show puzzle UI and disable player controls
        puzzleUI.SetActive(true);
        playerController.SetActive(false);

        // Show and unlock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Switches back from puzzle view to main game view
    /// - Enables the main camera
    /// - Disables the puzzle camera
    /// - Hides the puzzle UI
    /// - Re-enables player movement
    /// - Locks and hides the cursor
    /// </summary>
    public void ExitPuzzleView()
    {
        if (!inPuzzle) return;
        inPuzzle = false;
        
        objectiveCanvas.SetActive(true);

        // Switch back to main camera
        puzzleCamera.gameObject.SetActive(false);
        mainCamera.enabled = true;
        puzzleCamera.enabled = false;

        // Hide puzzle UI and re-enable player controls
        puzzleUI.SetActive(false);
        playerController.SetActive(true);

        // Hide and lock cursor for first-person controls
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
