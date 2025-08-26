using UnityEngine;

public class Lockpick : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;                      // Camera used for raycasting and mouse-to-world calculations
    [SerializeField] private Transform innerLock;             // The static lock core (target point)
    [SerializeField] private Transform pickPosition;          // The position where the lockpick is placed
    [SerializeField] private LockPickCameraManager exit;      // Handles exiting lockpick mode
    [SerializeField] private DoorInteraction open;            // Door interaction reference
    [SerializeField] private AudioSource openAudio;           // Unlock sound
    [SerializeField] private GameObject innerLockVisual;      // The visual part of the inner lock to overlap with

    [Header("Lockpick Settings")]
    [SerializeField] private float rotationOffset = 0f;       // Adjustment offset for pick sprite/model

    private float eulerAngle;                  // Current pick angle
    private bool isMouseHeld = false;          // Is mouse held down on the pick?
    private bool isUnlocked = false;           // Track if lock is already unlocked
    private float targetUnlockAngle;           // Randomly assigned angle where the inner lock sits

    private void Start()
    {
        // Enable cursor for lockpicking UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Generate a random lock target angle (where inner lock will "sit")
        targetUnlockAngle = Random.Range(0f, 360f);

        // Rotate the inner lock to that static target angle (it won't rotate anymore)
        innerLock.localEulerAngles = new Vector3(0, 0, targetUnlockAngle);

        // Place the lockpick at its designated position
        if (pickPosition != null)
            transform.position = pickPosition.position;

        Debug.Log("Target unlock angle: " + targetUnlockAngle); // For testing
    }

    private void Update()
    {
        HandleMouseInput();

        if (isMouseHeld && !isUnlocked)
        {
            RotatePick();
        }
    }

    /// Handles mouse click and release detection.
    private void HandleMouseInput()
    {
        // Begin dragging when mouse is pressed on this object
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                isMouseHeld = true;
            }
        }

        // Stop dragging on mouse release
        if (Input.GetMouseButtonUp(0))
        {
            isMouseHeld = false;
        }
    }

    /// Handles pick rotation based on mouse position
    private void RotatePick()
    {
        // Get angle based on mouse position and apply rotation to pick
        Vector3 dir = Input.mousePosition - cam.WorldToScreenPoint(transform.position);
        eulerAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, eulerAngle - rotationOffset);
    }

    /// Called when the lockpick enters the trigger zone of the inner lock visual
    private void OnTriggerEnter(Collider other)
    {
        // Check if we're touching the inner lock visual and not already unlocked
        if (!isUnlocked && other.gameObject == innerLockVisual)
        {
            // Unlock successful
            isUnlocked = true;
            openAudio.Play();
            Debug.Log("Unlocked! Lockpick entered trigger zone of inner lock visual. Target angle was: " + targetUnlockAngle);
            isMouseHeld = false;
            OnSuccessfulLockpick();
        }
    }

    /// Called when lockpicking is successful.
    private void OnSuccessfulLockpick()
    {
        // Lock and hide cursor again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set door state and force open
        open.currentState = DoorState.Unlocked;
        exit.ExitLockpickMode(); // Return to main scene
        open.TryOpenDoor();      // Trigger door opening
    }
}