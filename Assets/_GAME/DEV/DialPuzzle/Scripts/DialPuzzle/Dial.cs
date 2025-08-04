using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a single dial in the puzzle. Handles user interaction, rotation, and communication with the puzzle controller.
/// </summary>
public class Dial : MonoBehaviour
{
    [Header("Dial Settings")]
    public int totalPositions = 3;        // Total number of positions the dial can rotate through (e.g., 3 positions = 0, 1, 2)
    public float rotationStep = 90f;      // Degrees to rotate per step (e.g., 90° for 4 directions)
    public float rotationSpeed = 10f;     // Speed at which the dial smoothly rotates

    [Header("UI Control")]
    public Button RotateButton;           // Optional UI button to rotate this dial

    private int currentIndex = 0;         // Current dial index (position player has rotated to)
    private bool isInteractable = false;  // Whether the dial can be interacted with
    private Quaternion targetRotation;    // Target rotation the dial should smoothly rotate to

    public int CurrentIndex => currentIndex; // Public read-only access to the current index

    public DialPuzzleController puzzleController; // Reference to the central puzzle controller

    private void Start()
    {
        // Store the current rotation as the initial target
        targetRotation = transform.rotation;

        // Find the puzzle controller in the scene (assumes only one exists)..
        if (puzzleController == null)
        {
            Debug.LogError("DialPuzzleController not found in scene!");
        }

        // Set up the UI button to trigger the Rotate method
        if (RotateButton != null)
        {
            RotateButton.onClick.AddListener(Rotate);
            RotateButton.interactable = false; // Initially disabled until activated by the controller
        }
    }

    private void Update()
    {
        // Smoothly rotate the dial toward the target rotation using interpolation
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    /// Called when the dial is rotated by the player.
    /// Updates the index, rotates the object, and notifies the puzzle controller.
    /// </summary>
    public void Rotate()
    {
        if (!isInteractable) return;

        // Advance to the next index, looping back to 0 if needed
        currentIndex = (currentIndex + 1) % totalPositions;

        // Calculate the new target rotation angle
        float angle = currentIndex * rotationStep;
        targetRotation = Quaternion.Euler(0, angle, 0);

        // Notify the puzzle controller to check if the current combination is correct
        puzzleController?.CheckSolution();
    }

    /// <summary>
    /// Enables or disables interaction with this dial (both via button and direct click).
    /// </summary>
    public void SetInteractable(bool interact)
    {
        isInteractable = interact;

        if (RotateButton != null)
            RotateButton.interactable = interact;
    }

    /// <summary>
    /// Optional: allow the player to rotate the dial by clicking it directly in the scene (if using colliders).
    /// </summary>
    private void OnMouseDown()
    {
        Rotate();
    }
}
