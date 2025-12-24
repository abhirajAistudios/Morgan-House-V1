using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(InputHandler))]
public class PlayerController : MonoBehaviour
{
    private Transform cameraHolder;
    private InputHandler inputHandler;

    [Header("Movement Settings")]
    [Tooltip("Walking speed in units per second")]
    public float walkSpeed = 5f;

    [Tooltip("Running speed in units per second")]
    public float runSpeed = 10f;

    [Tooltip("Crouching speed in units per second")]
    public float crouchSpeed = 2.5f;

    [Tooltip("Backward movement speed multiplier")]
    [Range(0.1f, 1f)]
    public float backwardSpeedMultiplier = 0.6f;

    [Tooltip("Backward running speed multiplier")]
    [Range(0.1f, 1f)]
    public float backwardRunSpeedMultiplier = 0.7f;

    [Tooltip("How quickly the player accelerates to target speed")]
    public float acceleration = 10f;

    [Tooltip("How quickly the player decelerates when input stops")]
    public float deceleration = 10f;

    [Tooltip("Height of jump in units")]
    public float jumpHeight = 2f;

    [Tooltip("Gravity force applied to the player")]
    public float gravity = -20f;

    [Tooltip("Multiplier for gravity when falling")]
    public float fallMultiplier = 2.5f;

    [Tooltip("Multiplier for gravity when jump button is released")]
    public float lowJumpMultiplier = 2f;

    [Tooltip("How much control the player has while in air (0-1)")]
    public float airControl = 0.3f;

    [Header("Sliding Settings")]
    [Tooltip("Enable sliding mechanics")]
    public bool enableSliding = true;

    [Tooltip("Minimum speed required to start sliding")]
    public float minSlideSpeed = 7f;

    [Tooltip("Initial slide speed when starting")]
    public float slideSpeed = 12f;

    [Tooltip("How quickly slide speed decelerates")]
    public float slideDeceleration = 8f;

    [Tooltip("Minimum speed before slide ends")]
    public float minSlideEndSpeed = 3f;

    [Tooltip("Maximum slide duration")]
    public float maxSlideDuration = 3f;

    [Tooltip("Camera tilt angle during slide")]
    public float slideCameraTilt = 5f;

    [Tooltip("Speed of camera tilt transition")]
    public float slideTiltSpeed = 8f;

    [Tooltip("Slide height (how much lower than crouch)")]
    public float slideHeight = 0.8f;

    [Tooltip("How much control player has while sliding")]
    [Range(0f, 1f)]
    public float slideControl = 0.3f;

    [Header("Stamina Settings")]
    [Tooltip("Maximum stamina points")]
    public float maxStamina = 100f;

    [Tooltip("Stamina consumed per second while running")]
    public float staminaDrainRate = 20f;

    [Tooltip("Stamina regenerated per second")]
    public float staminaRegenRate = 15f;

    [Tooltip("Delay before stamina starts regenerating")]
    public float staminaRegenDelay = 1f;

    [Tooltip("Minimum stamina required to start running")]
    public float minStaminaToRun = 10f;

    [Tooltip("Stamina cost for starting a slide")]
    public float slideCost = 15f;

    [Header("Leaning Settings")]
    [Tooltip("Enable leaning with Q and E keys")]
    public bool enableLeaning = true;

    [Tooltip("Maximum lean angle in degrees")]
    public float leanAngle = 8f;

    [Tooltip("Speed of lean transition")]
    public float leanSpeed = 8f;

    [Tooltip("Distance to lean sideways")]
    public float leanDistance = 0.8f;

    [Tooltip("Forward offset when leaning for better peeking")]
    public float leanForwardOffset = 0.3f;

    [Tooltip("Layers to check for collision when leaning")]
    public LayerMask leanCheckMask = 1;

    [Header("Look Settings")]
    [Tooltip("Maximum angle to look up")]
    public float lookUpLimit = 80f;

    [Tooltip("Maximum angle to look down")]
    public float lookDownLimit = -80f;

    [Header("Crouch Settings")]
    [Tooltip("Character controller height when crouching")]
    public float crouchHeight = 1f;

    [Tooltip("Character controller height when standing")]
    public float standingHeight = 2f;

    [Tooltip("Speed of crouch transition")]
    public float crouchTransitionSpeed = 10f;

    [Header("Head Bob Settings")]
    [Tooltip("Enable head bobbing while walking")]
    public bool enableHeadBob = true;

    [Tooltip("Speed of head bob when walking")]
    public float walkBobSpeed = 12f;

    [Tooltip("Amplitude of head bob when walking")]
    public float walkBobAmount = 0.03f;

    [Tooltip("Speed of head bob when running")]
    public float runBobSpeed = 16f;

    [Tooltip("Amplitude of head bob when running")]
    public float runBobAmount = 0.04f;

    [Tooltip("Smoothness of head bob transition")]
    public float bobSmoothness = 10f;

    [Header("Breathing Effect Settings")]
    [Tooltip("Enable subtle breathing effect when idle")]
    public bool enableBreathingEffect = true;

    [Tooltip("Speed of breathing animation")]
    public float breathingSpeed = 1.5f;

    [Tooltip("Horizontal amplitude of breathing effect")]
    public float breathingAmount = 0.02f;

    [Tooltip("Vertical amplitude of breathing effect")]
    public float breathingVerticalAmount = 0.01f;

    [Header("Camera Sway Settings")]
    [Tooltip("Enable camera sway based on mouse movement")]
    public bool enableCameraSway = true;

    [Tooltip("Amount of camera sway")]
    public float swayAmount = 0.02f;

    [Tooltip("Speed of camera sway transition")]
    public float swaySpeed = 2f;

    // Components
    public CharacterController controller { get; private set; }
    public Camera playerCamera {  get; private set; }

    // Movement variables
    public Vector3 velocity;
    public Vector3 currentMovement {  get; private set; }
    public bool isRunning { get; private set; }
    public bool isCrouching { get; private set; }
    public bool isGrounded { get; private set; }

    // Sliding variables
    public bool isSliding { get; private set; }
    private float currentSlideSpeed;
    private Vector3 slideDirection;
    private float slideTimer;
    private float currentSlideTilt;
    private float targetSlideTilt;

    // Stamina variables
    private float currentStamina;
    private float staminaRegenTimer;
    private bool canRun = true;

    // Leaning variables
    private float currentLeanAngle;
    private float targetLeanAngle;
    public Vector3 originalCameraOffset;
    private Vector3 currentLeanOffset;
    private Vector3 targetLeanOffset;
    public bool isLeaning { get; private set; }

    // Look variables
    private float xRotation = 0f;
    private float yRotation = 0f;

    // Head bob variables
    private float headBobTimer;
    public Vector3 originalCameraPosition { get; private set; }
    private Vector3 currentBobOffset;
    private Vector3 targetBobOffset;

    // Breathing effect variables
    private float breathingTimer;

    // Camera sway variables
    private Vector3 swayPosition;
    
    void Start()
    {
        // Get components
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<InputHandler>();
        playerCamera = Camera.main;

        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();

        cameraHolder = playerCamera.transform.parent;

        // Store original camera position for head bob
        originalCameraPosition = playerCamera.transform.localPosition;
        originalCameraOffset = originalCameraPosition;

        // Initialize stamina
        currentStamina = maxStamina;

        // Set up character controller
        controller.height = standingHeight;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        HandleLeaning();
        HandleCrouch();
        HandleJump();
        HandleStamina();
        HandleHeadBob();
        HandleBreathingEffect();
        HandleCameraSway();
        HandleSlideTilt();
        HandleSliding();
        UpdatePlayerPositionForObjective();
    }

    void LateUpdate()
    {
        if (!isLeaning)
        {
            playerCamera.transform.localPosition = originalCameraOffset + swayPosition + currentBobOffset;
        }
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        PlayerAnimationController.Instance.SetGrounded(isGrounded);
        
        Vector3 targetMovement = Vector3.zero;

        if (isSliding)
        {
            // Handle sliding movement
            HandleSlideMovement();
            targetMovement = slideDirection * currentSlideSpeed;
        }
        else
        {
            // Normal movement
            Vector3 movementDirection = inputHandler.GetMovementDirection(transform);
            //playerAnimator.EnableWalkAnimation();

            // Determine speed based on state
            float targetSpeed = walkSpeed;
            if (isCrouching)
            {
                targetSpeed = crouchSpeed;
            }
            else if (isRunning)
            {
                targetSpeed = runSpeed;
            }

            // Apply backward movement penalties
            if (inputHandler.IsMovingBackward)
            {
                if (isRunning)
                {
                    targetSpeed *= backwardRunSpeedMultiplier;
                }
                else
                {
                    targetSpeed *= backwardSpeedMultiplier;
                }
            }

            targetMovement = movementDirection * targetSpeed;
        }

        // Apply air control when not grounded
        float controlMultiplier = isGrounded ? 1f : airControl;

        // Apply slide control when sliding
        if (isSliding)
            controlMultiplier *= slideControl;

        // Smooth acceleration/deceleration
        float accelerationRate = inputHandler.MovementInput.magnitude > 0 ? acceleration : deceleration;
        currentMovement = Vector3.Lerp(currentMovement, targetMovement, accelerationRate * controlMultiplier * Time.deltaTime);

        // Apply movement
        controller.Move(currentMovement * Time.deltaTime);

        // Apply gravity
        ApplyGravity();
        controller.Move(velocity * Time.deltaTime);

        // Update running state
        UpdateRunningState();
        
    }


    void UpdateRunningState()
    {
        bool wantsToRun = inputHandler.WantsToRun() && !isCrouching && !isSliding;
        isRunning = wantsToRun && canRun && currentStamina > minStaminaToRun;
    }

    void HandleSliding()
    {
        // Check if we should start sliding
        if (enableSliding && inputHandler.CrouchPressed && !isSliding && !isCrouching)
        {
            if (isRunning && currentMovement.magnitude >= minSlideSpeed &&
                currentStamina >= slideCost && !inputHandler.IsMovingBackward)
            {
                StartSlide();
            }
            else if (!isSliding)
            {
                isCrouching = !isCrouching;
            }
        }
        else if (inputHandler.CrouchPressed && !isSliding)
        {
            isCrouching = !isCrouching;
        }

        // End slide early if crouch is released
        if (isSliding && inputHandler.CrouchReleased)
        {
            EndSlide();
        }
    }

    void StartSlide()
    {
        if (!enableSliding) return;

        isSliding = true;
        isCrouching = false;
        isRunning = false;

        // Set slide parameters
        slideDirection = transform.forward;
        currentSlideSpeed = slideSpeed;
        slideTimer = 0f;
        targetSlideTilt = slideCameraTilt;

        // Consume stamina
        currentStamina -= slideCost;
        currentStamina = Mathf.Max(0f, currentStamina);

        Debug.Log("Started sliding!");
    }

    void HandleSlideMovement()
    {
        if (!isSliding) return;

        slideTimer += Time.deltaTime;

        // Decelerate slide speed
        currentSlideSpeed = Mathf.Lerp(currentSlideSpeed, 0f, slideDeceleration * Time.deltaTime);

        // Add slight steering during slide
        if (inputHandler.MovementInput.magnitude > 0)
        {
            Vector3 steerDirection = transform.right * inputHandler.MovementInput.x;
            slideDirection = Vector3.Slerp(slideDirection, slideDirection + steerDirection * 0.3f, slideControl * Time.deltaTime);
            slideDirection.Normalize();
        }

        // Check if slide should end
        if (currentSlideSpeed <= minSlideEndSpeed || slideTimer >= maxSlideDuration)
        {
            EndSlide();
        }
    }

    void EndSlide()
    {
        if (!isSliding) return;

        isSliding = false;
        isCrouching = true; // Transition to crouch after slide
        targetSlideTilt = 0f;

        Debug.Log("Ended sliding!");
    }

    void HandleSlideTilt()
    {
        // Handle camera tilt during sliding
        if (isSliding)
        {
            currentSlideTilt = Mathf.Lerp(currentSlideTilt, targetSlideTilt, slideTiltSpeed * Time.deltaTime);
        }
        else
        {
            currentSlideTilt = Mathf.Lerp(currentSlideTilt, 0f, slideTiltSpeed * Time.deltaTime);
        }

        // Apply tilt to camera (this combines with lean tilt)
        Vector3 eulerAngles = playerCamera.transform.localEulerAngles;
        eulerAngles.z = currentLeanAngle + currentSlideTilt;
        playerCamera.transform.localEulerAngles = eulerAngles;
    }

    void ApplyGravity()
    {
        if (velocity.y < 0)
        {
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else if (velocity.y > 0 && !inputHandler.JumpHeld)
        {
            velocity.y += gravity * lowJumpMultiplier * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    void HandleLook()
    {
        Vector2 lookInput = inputHandler.LookInput;

        // Rotate the body horizontally
        yRotation += lookInput.x;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        // Rotate the camera vertically
        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, lookDownLimit, lookUpLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleCrouch()
    {
        FlashlightController.Instance.isCrouching = isCrouching;
        
            float targetHeight = standingHeight;
            float targetCenterY = standingHeight / 2f;

            if (isSliding)
            {
                targetHeight = slideHeight;
                targetCenterY = slideHeight / 2f;
            }
            else if (isCrouching)
            {
                targetHeight = crouchHeight;
                targetCenterY = crouchHeight / 2f;
            }

            // Fix: Adjust both height and center to prevent full-body sinking
            controller.height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            controller.center = Vector3.Lerp(controller.center, new Vector3(0f, targetCenterY, 0f), crouchTransitionSpeed * Time.deltaTime);

            // Adjust camera position based on crouch/slide only (same logic as before)
            Vector3 targetCameraPos = originalCameraPosition;
            if (isSliding)
            {
                targetCameraPos.y = originalCameraPosition.y - (standingHeight - slideHeight) * 0.5f;
            }
            else if (isCrouching)
            {
                targetCameraPos.y = originalCameraPosition.y - (standingHeight - crouchHeight) * 0.5f;
            }

            // Breathing offset (unchanged)
            Vector3 breathingOffset = Vector3.zero;
            if (enableBreathingEffect && !isCrouching && !isSliding && isGrounded && inputHandler.MovementInput.magnitude == 0)
            {
                breathingOffset = new Vector3(
                    Mathf.Sin(breathingTimer) * breathingAmount,
                    Mathf.Sin(breathingTimer * 0.5f) * breathingVerticalAmount,
                    0f
                );
            }

            // Camera offset transition (unchanged)
            originalCameraOffset = Vector3.Lerp(
                originalCameraOffset,
                targetCameraPos,
                crouchTransitionSpeed * Time.deltaTime
            );

            // Apply final camera position if not leaning (unchanged)
            if (!isLeaning)
            {
                playerCamera.transform.localPosition = originalCameraOffset + swayPosition + breathingOffset + currentBobOffset;
            }

    }

    void HandleStamina()
    {
        bool isMoving = inputHandler.MovementInput.magnitude > 0 && isGrounded;

        // Only drain stamina when running forward (not backward)
        if (isRunning && isMoving && !inputHandler.IsMovingBackward)
        {
            // Drain stamina when running
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0f, currentStamina);

            // Reset regen timer when running
            staminaRegenTimer = 0f;

            // Stop running if stamina is too low
            if (currentStamina <= 0f)
            {
                canRun = false;
            }
        }
        else
        {
            // Regenerate stamina when not running forward
            staminaRegenTimer += Time.deltaTime;

            if (staminaRegenTimer >= staminaRegenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);

                // Allow running again when stamina is sufficient
                if (currentStamina >= minStaminaToRun)
                {
                    canRun = true;
                }
            }
        }
    }

    void HandleBreathingEffect()
    {
        if (!enableBreathingEffect) return;

        // Only apply breathing effect when idle (not moving)
        if (isGrounded && inputHandler.MovementInput.magnitude == 0 && !isSliding)
        {
            breathingTimer += Time.deltaTime * breathingSpeed;

            Vector3 breathingOffset = new Vector3(
                Mathf.Sin(breathingTimer) * breathingAmount,
                Mathf.Sin(breathingTimer * 0.5f) * breathingVerticalAmount,
                0f
            );

            playerCamera.transform.localPosition = originalCameraPosition + breathingOffset + swayPosition;
        }
    }

    void HandleJump()
    {
        if (inputHandler.JumpPressed && isGrounded && !isSliding)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }



    void HandleLeaning()
    {
        if (!enableLeaning || isSliding || isRunning) return;

        // Update leaning state from input
        isLeaning = inputHandler.WantsToLean();

        // Calculate target lean angle and offset
        if (isLeaning)
        {
            targetLeanAngle = inputHandler.LeanDirection == -1 ? leanAngle : -leanAngle;

            // Check if we can lean in the desired direction
            Vector3 desiredOffset = CalculateLeanOffset(inputHandler.LeanDirection);
            Vector3 checkPosition = transform.position + desiredOffset;

            // Raycast to check if leaning position is valid
            if (CanLeanToPosition(checkPosition))
            {
                targetLeanOffset = desiredOffset;
            }
            else
            {
                targetLeanOffset = GetSafeLeanOffset(inputHandler.LeanDirection);
            }
        }
        else
        {
            targetLeanAngle = 0f;
            targetLeanOffset = Vector3.zero;
        }

        // Smooth transitions
        if (targetLeanOffset != Vector3.zero)
        {
            currentLeanAngle = Mathf.Lerp(currentLeanAngle, targetLeanAngle, leanSpeed * Time.deltaTime);
        }
        else
        {
            currentLeanAngle = Mathf.Lerp(currentLeanAngle, 0f, leanSpeed * Time.deltaTime);
        }

        currentLeanOffset = Vector3.Lerp(currentLeanOffset, targetLeanOffset, leanSpeed * Time.deltaTime);

        // Apply lean rotation to camera holder
        if (cameraHolder != null)
        {
            cameraHolder.localPosition = transform.InverseTransformDirection(currentLeanOffset);
        }

        // Apply lean position offset to camera
        Vector3 finalCameraPosition = originalCameraOffset + currentBobOffset + swayPosition + transform.InverseTransformDirection(currentLeanOffset);
        playerCamera.transform.localPosition = finalCameraPosition;
        
    }

    Vector3 CalculateLeanOffset(int direction)
    {
        if (direction == 0) return Vector3.zero;

        Vector3 rightVector = transform.right;
        Vector3 forwardVector = transform.forward;

        Vector3 offset = Vector3.zero;
        offset += rightVector * (direction * leanDistance);
        offset += forwardVector * leanForwardOffset;

        return offset;
    }

    bool CanLeanToPosition(Vector3 position)
    {
        float checkRadius = controller.radius * 0.8f;
        float checkHeight = controller.height * 0.5f;

        // Check center position
        if (Physics.CheckSphere(position, checkRadius, leanCheckMask))
            return false;

        // Check head position
        if (Physics.CheckSphere(position + Vector3.up * checkHeight, checkRadius * 0.7f, leanCheckMask))
            return false;

        // Check foot position
        if (Physics.CheckSphere(position + Vector3.down * checkHeight, checkRadius * 0.7f, leanCheckMask))
            return false;

        return true;
    }

    Vector3 GetSafeLeanOffset(int direction)
    {
        for (float distance = leanDistance * 0.1f; distance <= leanDistance; distance += leanDistance * 0.1f)
        {
            Vector3 testOffset = Vector3.zero;
            testOffset += transform.right * (direction * distance);
            testOffset += transform.forward * leanForwardOffset;

            Vector3 testPosition = transform.position + testOffset;

            if (CanLeanToPosition(testPosition))
            {
                return testOffset;
            }
        }

        return Vector3.zero;
    }

    public float LeanDirection
    {
        get
        {
            if (isLeaning)
                return inputHandler.LeanDirection;
            return 0f;
        }
    }


    void HandleCameraSway()
    {
        if (!enableCameraSway) return;

        Vector2 lookInput = inputHandler.LookInput;

        swayPosition = Vector3.Lerp(swayPosition, new Vector3(
            -lookInput.x * swayAmount,
            -lookInput.y * swayAmount,
            0f
        ), swaySpeed * Time.deltaTime);
    }

    void HandleHeadBob()
    {
        if (!enableHeadBob) return;

        if (isGrounded && inputHandler.MovementInput.magnitude > 0 && !isSliding)
        {
            float bobSpeed = isRunning ? runBobSpeed : walkBobSpeed;
            float bobAmount = isRunning ? runBobAmount : walkBobAmount;

            headBobTimer += Time.deltaTime * bobSpeed;

            // More natural head bob pattern
            float horizontalBob = Mathf.Sin(headBobTimer) * bobAmount * 0.5f;
            float verticalBob = Mathf.Sin(headBobTimer * 2f) * bobAmount;

            // Smooth the bob movement to reduce jerkiness
            targetBobOffset = new Vector3(horizontalBob, verticalBob, 0f);
            currentBobOffset = Vector3.Lerp(currentBobOffset, targetBobOffset, bobSmoothness * Time.deltaTime);

            playerCamera.transform.localPosition = originalCameraPosition + currentBobOffset + swayPosition;
        }
        else
        {
            headBobTimer = 0f;
            // Smoothly return to original position when stopping
            targetBobOffset = Vector3.zero;
            currentBobOffset = Vector3.Lerp(currentBobOffset, targetBobOffset, bobSmoothness * Time.deltaTime);
            playerCamera.transform.localPosition = originalCameraPosition + currentBobOffset + swayPosition;
        }
    }

    // Public methods for external access
    public bool IsGrounded => isGrounded;
    public bool IsCrouching => isCrouching;
    public bool IsRunning => isRunning;
    public bool IsSliding => isSliding;
    public bool IsMoving => inputHandler.MovementInput.magnitude > 0;
    public bool IsMovingBackward => inputHandler.IsMovingBackward;
    public float CurrentSpeed => currentMovement.magnitude;
    public Vector3 Velocity => velocity;
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercentage => currentStamina / maxStamina;
    public bool CanRun => canRun;
    public bool IsLeaning => isLeaning;
    public float LeanAngle => currentLeanAngle;
    public float SlideSpeed => currentSlideSpeed;
    public Vector3 SlideDirection => slideDirection;
    public CharacterController GetCharacterController() => controller;
    public InputHandler GetInputHandler() => inputHandler;

    // Public methods for external control
    public void SetInputEnabled(bool enabled)
    {
        inputHandler.EnableInput(enabled);
    }

    public void SetCursorLock(bool locked)
    {
        inputHandler.SetCursorLock(locked);
    }

    public void ForceEndSlide()
    {
        if (isSliding)
        {
            EndSlide();
        }
    }

    public void ResetStamina()
    {
        currentStamina = maxStamina;
        canRun = true;
    }

    public void AddStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0f, maxStamina);
        if (currentStamina >= minStaminaToRun)
        {
            canRun = true;
        }
    }

    public void DrainStamina(float amount)
    {
        currentStamina = Mathf.Max(0f, currentStamina - amount);
        if (currentStamina <= 0f)
        {
            canRun = false;
        }
    }

    void OnDrawGizmos()
    {
        // Draw ground check ray
        Gizmos.color = Color.red;

        CharacterController charController = controller;
        if (charController == null)
            charController = GetComponent<CharacterController>();

        if (charController != null)
        {
            float rayDistance = charController.height * 0.5f + 0.3f;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayDistance);
        }
        else
        {
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 1.3f);
        }

        // Draw lean check positions
        if (enableLeaning && Application.isPlaying)
        {
            Gizmos.color = Color.blue;

            // Draw left lean position
            Vector3 leftLeanPos = transform.position + CalculateLeanOffset(-1);
            Gizmos.DrawWireSphere(leftLeanPos, 0.3f);

            // Draw right lean position  
            Vector3 rightLeanPos = transform.position + CalculateLeanOffset(1);
            Gizmos.DrawWireSphere(rightLeanPos, 0.3f);

            // Draw current lean position
            if (isLeaning)
            {
                Gizmos.color = Color.green;
                Vector3 currentPos = transform.position + currentLeanOffset;
                Gizmos.DrawWireSphere(currentPos, 0.3f);
            }
        }

        // Draw slide direction
        if (isSliding && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, slideDirection * 2f);
        }

        // Draw movement direction from input handler
        if (Application.isPlaying && inputHandler != null && inputHandler.MovementInput.magnitude > 0)
        {
            Gizmos.color = Color.cyan;
            Vector3 moveDir = inputHandler.GetMovementDirection(transform);
            Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, moveDir * 1.5f);
        }
    }

    private void UpdatePlayerPositionForObjective()
    {
        if (IsMoving)
        {
            EventService.Instance.OnPlayerMoved.InvokeEvent(transform);
        }
    }
}