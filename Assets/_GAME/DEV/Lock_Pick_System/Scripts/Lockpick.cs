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
    [SerializeField] private AudioSource wrongDirectionAudio; // Sound for wrong direction

    [Header("Lockpick Settings")]
    [SerializeField] private float unlockThreshold = 2f;      // Small threshold for precise matching
    [SerializeField] private float rotationOffset = 0f;       // Adjustment offset for pick sprite/model

    private float eulerAngle;                  // Current pick angle
    private float previousAngle;               // Previous frame's angle to detect direction
    private bool isMouseHeld = false;          // Is mouse held down on the pick
    private float targetUnlockAngle;           // Randomly assigned angle where the inner lock sits
    private bool hasPassedTarget = false;      // Track if we've passed the target from the wrong direction

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

        // Initialize previous angle
        previousAngle = NormalizeAngle(transform.eulerAngles.z);

        Debug.Log("Target unlock angle: " + targetUnlockAngle); // For testing
    }

    private void Update()
    {
        HandleMouseInput();

        if (isMouseHeld)
            RotatePickAndCheckUnlock();

        // Update previous angle for next frame
        previousAngle = NormalizeAngle(transform.eulerAngles.z);
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
                hasPassedTarget = false; // Reset when starting new attempt
            }
        }

        // Stop dragging on mouse release
        if (Input.GetMouseButtonUp(0))
        {
            isMouseHeld = false;
        }
    }

    /// Handles pick rotation and checks if it matches the lock's target angle.
    private void RotatePickAndCheckUnlock()
    {
        // Get angle based on mouse position and apply rotation to pick
        Vector3 dir = Input.mousePosition - cam.WorldToScreenPoint(transform.position);
        eulerAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, eulerAngle - rotationOffset);

        // Normalize pick angle and compare to lock target angle
        float currentPickAngle = NormalizeAngle(transform.eulerAngles.z);

        // Check if we've passed the target from the wrong direction
        CheckIfPassedTargetWrongDirection(currentPickAngle);

        // Check if the pick is exactly at the inner lock's angle (within threshold)
        // AND we're approaching from the correct direction
        if (Mathf.Abs(Mathf.DeltaAngle(currentPickAngle, targetUnlockAngle)) <= unlockThreshold &&
            IsApproachingFromCorrectDirection(currentPickAngle))
        {
            // Snap the pick to exactly match the inner lock angle for visual feedback
            transform.rotation = Quaternion.Euler(0, 0, targetUnlockAngle);

            // Unlock successful
            openAudio.Play();
            Debug.Log("Unlocked! Pick angle: " + currentPickAngle + " | Target angle: " + targetUnlockAngle);
            isMouseHeld = false;
            OnSuccessfulLockpick();
        }
    }

    /// Checks if we're approaching the target from the correct (closest) direction
    private bool IsApproachingFromCorrectDirection(float currentAngle)
    {
        // Calculate the shortest path to the target
        float angleToTarget = Mathf.DeltaAngle(currentAngle, targetUnlockAngle);

        // Calculate direction of movement
        float angleChange = Mathf.DeltaAngle(previousAngle, currentAngle);

        // If we're not moving, can't be approaching correctly
        if (Mathf.Abs(angleChange) < 0.1f)
            return false;

        // If we've already passed the target from the wrong direction, fail
        if (hasPassedTarget)
            return false;

        // Check if we're moving toward the target using the shortest path
        // Moving clockwise toward target (angleToTarget is negative when we need to move clockwise)
        if (angleToTarget < 0 && angleChange < 0)
            return true;

        // Moving counter-clockwise toward target (angleToTarget is positive when we need to move counter-clockwise)
        if (angleToTarget > 0 && angleChange > 0)
            return true;

        return false;
    }

    /// Check if we've passed the target from the wrong direction
    private void CheckIfPassedTargetWrongDirection(float currentAngle)
    {
        float angleToTarget = Mathf.DeltaAngle(currentAngle, targetUnlockAngle);
        float angleChange = Mathf.DeltaAngle(previousAngle, currentAngle);

        // If we're moving away from the target after passing it
        if (Mathf.Abs(angleChange) > 1f) // Ignore tiny movements
        {
            // We've passed the target if the angle to target changed sign
            bool wasApproaching = (angleToTarget > 0 && angleChange > 0) || (angleToTarget < 0 && angleChange < 0);
            bool isNowMovingAway = (angleToTarget > 0 && angleChange < 0) || (angleToTarget < 0 && angleChange > 0);

            if (!wasApproaching && isNowMovingAway)
            {
                hasPassedTarget = true;
                if (wrongDirectionAudio != null) wrongDirectionAudio.Play();
                Debug.Log("Wrong direction! Passed the target angle.");
            }
        }
    }

    /// Ensures angles stay between 0 and 360.
    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0) angle += 360f;
        return angle;
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

        // Disable this script to prevent further interaction
        enabled = false;
    }

    // Optional: Visual feedback in editor
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Draw a line from the lockpick to the target angle for debugging
            Vector3 dir = Quaternion.Euler(0, 0, targetUnlockAngle) * Vector3.right * 2f;
            Gizmos.color = hasPassedTarget ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, transform.position + dir);

            // Draw the allowed approach direction
            float approachAngle = targetUnlockAngle + 90f; // Perpendicular to show direction
            Vector3 approachDir = Quaternion.Euler(0, 0, approachAngle) * Vector3.right * 0.5f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + approachDir);
        }
    }
}