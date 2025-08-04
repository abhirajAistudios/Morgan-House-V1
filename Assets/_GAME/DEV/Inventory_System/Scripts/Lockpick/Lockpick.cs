using UnityEngine;
using UnityEngine.SceneManagement;
// using static DoorInteraction;

public class Lockpick : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform innerLock;
    public Transform pickPosition;
     public LockPickCameraManager exit;
     public DoorInteraction open;
    public AudioSource openAudio;

    [Header("Lockpick Settings")]
    public float lockSpeed = 10f;
    public float unlockThreshold = 15f;
    public float maxUnlockRotation = 90f;

    [Tooltip("Adjust to match your lockpick's default sprite/model orientation.")]
    public float rotationOffset = 0f;

    [SerializeField]
    private float[] unlockAngles = new float[4];

    private float eulerAngle;
    private bool isMouseHeld = false;
    private bool isUnlocking = false;
    private float currentTargetUnlockAngle = 0f;

    void Start()
    {
        // Enable cursor for lockpicking UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GenerateRandomUnlockAngles();

        // Place the lockpick at the designated position
        if (pickPosition != null)
            transform.position = pickPosition.position;
    }

    void Update()
    {
        HandleMouseInput();

        if (isMouseHeld)
            RotatePickAndAttemptUnlock();
        else
            ResetInnerLockRotation();
    }

    /// <summary>
    /// Handles mouse click and release detection.
    /// </summary>
    void HandleMouseInput()
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

    /// <summary>
    /// Handles pick rotation, sweet spot detection, and unlocking animation.
    /// </summary>
    void RotatePickAndAttemptUnlock()
    {
        // Get angle based on mouse position and apply rotation
        Vector3 dir = Input.mousePosition - cam.WorldToScreenPoint(transform.position);
        eulerAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, eulerAngle - rotationOffset);

        float currentPickAngle = NormalizeAngle(transform.eulerAngles.z);
        isUnlocking = false;

        // Check if current angle matches any unlock sweet spot
        foreach (float unlockAngle in unlockAngles)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(currentPickAngle, unlockAngle)) <= unlockThreshold)
            {
                currentTargetUnlockAngle = unlockAngle;
                isUnlocking = true;
                break;
            }
        }

        if (isUnlocking)
        {
            // Rotate the inner lock towards the unlock position
            float newZ = Mathf.LerpAngle(innerLock.eulerAngles.z, maxUnlockRotation, Time.deltaTime * lockSpeed);
            innerLock.eulerAngles = new Vector3(0, 0, newZ);

            // Unlock successful
            if (Mathf.Abs(innerLock.eulerAngles.z) >= maxUnlockRotation - 2f)
            {
                openAudio.Play();
                Debug.Log("Unlocked! Target angle was: " + currentTargetUnlockAngle);
                isMouseHeld = false;
                enabled = false;
                OnSuccessfulLockpick();
            }
        }
        else
        {
            // Reset lock if pick is not in the sweet spot
            float newZ = Mathf.LerpAngle(innerLock.eulerAngles.z, 0, Time.deltaTime * lockSpeed * 2);
            innerLock.eulerAngles = new Vector3(0, 0, newZ);
        }
    }

    /// <summary>
    /// Gradually resets the inner lock if not interacting.
    /// </summary>
    void ResetInnerLockRotation()
    {
        float newZ = Mathf.LerpAngle(innerLock.eulerAngles.z, 0, Time.deltaTime * lockSpeed);
        innerLock.eulerAngles = new Vector3(0, 0, newZ);
    }

    /// <summary>
    /// Generates random sweet spot angles for unlocking.
    /// </summary>
    void GenerateRandomUnlockAngles()
    {
        for (int i = 0; i < unlockAngles.Length; i++)
        {
            unlockAngles[i] = Random.Range(0f, 360f);
        }
    }

    /// <summary>
    /// Ensures angles stay between 0 and 360.
    /// </summary>
    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }

    /// <summary>
    /// Called when lockpicking is successful.
    /// </summary>
    public void OnSuccessfulLockpick()
    {
        // Mark door to open after returning to main scene
         //DoorUnlockTracker.wasUnlockedViaLockpick = true;

        // Lock and hide cursor again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set door state and force open
         open.currentState = DoorState.Unlocked;
         exit.ExitLockpickMode(); // Return to main scene
         open.TryOpenDoor();    // Trigger door opening
    }
}
