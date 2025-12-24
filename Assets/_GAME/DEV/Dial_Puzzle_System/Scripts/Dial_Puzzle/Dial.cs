using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a single dial in the puzzle. Handles user interaction, rotation, and communication with the puzzle controller.
/// </summary>
public class Dial : MonoBehaviour
{
    #region Required Variables
    
    [Header("Dial Settings")]
    [SerializeField] private int totalPositions = 3;        // Total number of positions the dial can rotate through (e.g., 3 positions = 0, 1, 2)
    [SerializeField] private float rotationStep = 90f;      // Degrees to rotate per step
    [SerializeField] private float rotationSpeed = 10f;     // Speed at which the dial smoothly rotates
    
    public enum RotationAxis { X, Y, Z }
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;

    [Header("UI Control")]
    [SerializeField] private Button rotateButton;           // Optional UI button to rotate this dial
    
    [Header("Sound")]
    [SerializeField] private Sounds rotateSound; 

    private int currentIndex;                  // Current dial index (position player has rotated to)
    private bool isInteractable;               // Whether the dial can be interacted with
    private Quaternion targetRotation;         // Target rotation the dial should smoothly rotate to
    private Vector3 baseEuler;                 // Store the starting local rotation (Euler angles)

    public int CurrentIndex => currentIndex;   // Public read-only access to the current index

    [SerializeField] private DialPuzzleController puzzleController; // Reference to the central puzzle controller
    
    #endregion

    private void Start()
    {
        // Store the initial local rotation as the base
        baseEuler = transform.localEulerAngles;
        targetRotation = transform.localRotation;

        if (puzzleController == null)
        {
            Debug.LogError("DialPuzzleController not found in scene!");
        }

        if (rotateButton != null)
        {
            rotateButton.onClick.AddListener(Rotate);
            rotateButton.interactable = false; 
        }
    }

    private void Update()
    {
        // Smoothly rotate the dial toward the target rotation using interpolation
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    /// Called when the dial is rotated by the player.
    /// Updates the index, rotates the object, and notifies the puzzle controller.
    /// </summary>
    private void Rotate()
    {
        if (!isInteractable) return;

        SoundService.Instance.Play(rotateSound);
        
        currentIndex = (currentIndex + 1) % totalPositions;

        float angle = currentIndex * rotationStep;
        Vector3 newEuler = baseEuler;

        switch (rotationAxis)
        {
            case RotationAxis.X:
                newEuler.x = baseEuler.x + angle;
                break;
            case RotationAxis.Y:
                newEuler.y = baseEuler.y + angle;
                break;
            case RotationAxis.Z:
                newEuler.z = baseEuler.z + angle;
                break;
        }

        targetRotation = Quaternion.Euler(newEuler);

        puzzleController?.CheckSolution();
    }

    public void SetInteractable(bool interact)
    {
        isInteractable = interact;

        if (rotateButton != null)
            rotateButton.interactable = interact;
    }

    private void OnMouseDown()
    {
        Rotate();
    }
}
