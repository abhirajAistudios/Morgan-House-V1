using UnityEngine;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("Mouse sensitivity for looking around")]
    public float mouseSensitivity = 100f;

    [Tooltip("Enable/disable input handling")]
    public bool enableInput = true;

    // Movement Input
    public Vector2 MovementInput { get; private set; }
    public bool IsMovingBackward { get; private set; }

    // Action Inputs
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool RunHeld { get; private set; }
    public bool CrouchPressed { get; private set; }
    public bool CrouchReleased { get; private set; }
    public bool CrouchHeld { get; private set; }

    // Leaning Input
    public bool LeanLeftHeld { get; private set; }
    public bool LeanRightHeld { get; private set; }
    public int LeanDirection { get; private set; } // -1 for left, 1 for right, 0 for none

    //Interacting Input
    
    public bool InteractPressed { get; private set; }
    // Look Input
    public Vector2 LookInput { get; private set; }

    // Cursor Control
    public bool EscapePressed { get; private set; }

    // Internal state
    private bool cursorLocked = true;
    void Start()
    {
        // Lock cursor at start
        SetCursorLock(true);
    }

    void Update()
    {
        if (!enableInput) return;
        
        HandleMovementInput();
        HandleActionInput();
        HandleLeaningInput();
        HandleLookInput();
        HandleInteractInput();
    }

    void HandleMovementInput()
    {
        // Get raw input for precise control
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        MovementInput = new Vector2(horizontal, vertical);

        // Normalize for diagonal movement
        if (MovementInput.magnitude > 1f)
        {
            SetCursorLock(true);
            MovementInput.Normalize();
        }

        // Check if moving backward (any backward component)
        IsMovingBackward = vertical < -0.1f;
    }

    void HandleInteractInput()
    {
        InteractPressed = Input.GetKeyDown(KeyCode.Z);
        if (InteractPressed)
        {
            PlayerAnimationController.Instance.InvokeInteractTrigger();
        }
    }
    void HandleActionInput()
    {
        // Jump input
        JumpPressed = Input.GetButtonDown("Jump");
        JumpHeld = Input.GetButton("Jump");

        // Run input
        RunHeld = Input.GetKey(KeyCode.LeftShift);

        // Crouch input - support both Control and C keys
        CrouchPressed = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C);
        CrouchReleased = Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C);
        CrouchHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
    }

    void HandleLeaningInput()
    {
        // Leaning input
        LeanLeftHeld = Input.GetKey(KeyCode.Q);
        LeanRightHeld = Input.GetKey(KeyCode.E);

        // Determine lean direction
        if (LeanLeftHeld && !LeanRightHeld)
        {
            LeanDirection = -1;
        }
        else if (LeanRightHeld && !LeanLeftHeld)
        {
            LeanDirection = 1;
        }
        else
        {
            LeanDirection = 0;
        }
    }

    void HandleLookInput()
    {
        if (!cursorLocked) return;

        // Get mouse input with sensitivity applied
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        LookInput = new Vector2(mouseX, mouseY);
    }

    //void HandleCursorInput()
    //{
    //    // Toggle cursor lock with Escape
    //    EscapePressed = Input.GetKeyDown(KeyCode.Escape);

    //    if (EscapePressed)
    //    {
    //        SetCursorLock(!cursorLocked);
    //        enableInput = !enableInput;
    //        LookInput = new Vector2(0, 0);
    //       // SceneManager.LoadScene("PauseMenu");
    //    }
    //}

    public void SetCursorLock(bool locked)
    {
        cursorLocked = locked;

        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Public methods for external control
    public void EnableInput(bool enable)
    {
        enableInput = enable;

        if (!enable)
        {
            ClearAllInputs();
            SetCursorLock(false);
        }
    }

    public void ClearAllInputs()
    {
        MovementInput = Vector2.zero;
        IsMovingBackward = false;
        JumpPressed = false;
        JumpHeld = false;
        RunHeld = false;
        CrouchPressed = false;
        CrouchReleased = false;
        CrouchHeld = false;
        LeanLeftHeld = false;
        LeanRightHeld = false;
        LeanDirection = 0;
        LookInput = Vector2.zero;
        EscapePressed = false;
        InteractPressed = false;
    }

    // Utility methods for common input combinations
    public bool WantsToRun()
    {
        return RunHeld && MovementInput.magnitude > 0 && !IsMovingBackward;
    }

    public bool WantsToSlide()
    {
        return CrouchPressed && MovementInput.magnitude > 0 && !IsMovingBackward;
    }

    public bool WantsToLean()
    {
        return LeanDirection != 0;
    }

    public Vector3 GetMovementDirection(Transform playerTransform)
    {
        if (MovementInput.magnitude == 0) return Vector3.zero;

        Vector3 forward = playerTransform.forward;
        Vector3 right = playerTransform.right;

        Vector3 direction = (forward * MovementInput.y + right * MovementInput.x).normalized;
        return direction;
    }

    // Debug information
    public void PrintInputState()
    {
        Debug.Log($"Movement: {MovementInput}, Backward: {IsMovingBackward}, " +
                 $"Jump: {JumpPressed}/{JumpHeld}, Run: {RunHeld}, " +
                 $"Crouch: {CrouchPressed}/{CrouchHeld}, Lean: {LeanDirection}, " +
                 $"Look: {LookInput}");
    }

    // Input validation
    public bool IsValidMovementInput()
    {
        return MovementInput.magnitude > 0.1f;
    }

    public bool IsValidLookInput()
    {
        return LookInput.magnitude > 0.001f;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw movement direction
        if (MovementInput.magnitude > 0)
        {
            Gizmos.color = Color.green;
            Vector3 direction = GetMovementDirection(transform);
            Gizmos.DrawRay(transform.position, direction * 2f);
        }

        // Draw lean direction
        if (LeanDirection != 0)
        {
            Gizmos.color = Color.blue;
            Vector3 leanDir = transform.right * LeanDirection;
            Gizmos.DrawRay(transform.position, leanDir * 1f);
        }
    }
}