using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimationController : MonoBehaviour
{
    private static PlayerAnimationController _instance;

    public static PlayerAnimationController Instance => _instance;

    [Header("Animation References")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform playerModel;
    [SerializeField] private RigBuilder playerRigBuilder;

    private Vector3 originalModelLocalPos;
    private readonly Vector3 crouchModelOffset = new Vector3(0f, 0f, -0.3f); // Move model slightly backward when crouching

    private PlayerController playerController;
    private InputHandler inputHandler;

    protected virtual void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        inputHandler = GetComponent<InputHandler>();

        if (playerModel != null)
            originalModelLocalPos = playerModel.localPosition;
    }

    private void Update()
    {
        HandleCrouchAnimation();
        HandleMovementAnimation();
        HandleJumpAnimation();
        HandleLeanAnimation();
    }

    /// <summary>
    /// Updates movement animation parameters like MoveX and MoveY based on input.
    /// </summary>
    private void HandleMovementAnimation()
    {
        if (!playerController.isGrounded) return;

        Vector2 moveInput = inputHandler.MovementInput;
        float scale = playerController.isRunning ? 2f : 1f;

        Vector2 scaledInput = moveInput * scale;

        playerAnimator.SetFloat("MoveX", scaledInput.x, 0.1f, Time.deltaTime);
        playerAnimator.SetFloat("MoveY", scaledInput.y, 0.1f, Time.deltaTime);
        playerAnimator.SetBool("IsCrouching", playerController.isCrouching);
    }

    /// <summary>
    /// Manages crouch and slide transitions, including model and camera offset updates.
    /// </summary>
    private void HandleCrouchAnimation()
    {
        float targetHeight = playerController.isSliding 
            ? playerController.slideHeight 
            : playerController.isCrouching 
                ? playerController.crouchHeight 
                : playerController.standingHeight;

        playerController.controller.height = Mathf.Lerp(
            playerController.controller.height,
            targetHeight,
            playerController.crouchTransitionSpeed * Time.deltaTime
        );

        // Animation parameters
        playerAnimator.SetBool("IsCrouching", playerController.isCrouching);
        playerAnimator.SetFloat("CrouchSpeed", inputHandler.MovementInput.magnitude);
        playerAnimator.SetBool("CrouchWalk", playerController.isCrouching && inputHandler.MovementInput.magnitude > 0.1f);

        // Move model position slightly backward when crouching
        if (playerModel != null)
        {
            Vector3 targetOffset = playerController.isCrouching
                ? originalModelLocalPos + crouchModelOffset
                : originalModelLocalPos;

            playerModel.localPosition = Vector3.Lerp(
                playerModel.localPosition,
                targetOffset,
                playerController.crouchTransitionSpeed * Time.deltaTime
            );
        }

        // Adjust camera Y offset during crouch or slide
        float cameraYOffset = 0f;
        if (playerController.isSliding)
            cameraYOffset = (playerController.standingHeight - playerController.slideHeight) * 0.5f;
        else if (playerController.isCrouching)
            cameraYOffset = (playerController.standingHeight - playerController.crouchHeight) * 0.5f;

        Vector3 targetCameraOffset = playerController.originalCameraPosition - new Vector3(0f, cameraYOffset, 0f);
        playerController.originalCameraOffset = Vector3.Lerp(
            playerController.originalCameraOffset,
            targetCameraOffset,
            playerController.crouchTransitionSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Sets basic jumping and grounded animation parameters.
    /// </summary>
    private void HandleJumpAnimation()
    {
        playerAnimator.SetFloat("Speed", playerController.currentMovement.magnitude);
        playerAnimator.SetFloat("VerticalVelocity", playerController.velocity.y);
        playerAnimator.SetBool("IsGrounded", playerController.isGrounded);
    }

    /// <summary>
    /// Activates or deactivates rig layers for leaning left/right.
    /// </summary>
    private void HandleLeanAnimation()
    {
        if (playerController.isLeaning)
        {
            if (inputHandler.LeanDirection == -1) // Lean Left
            {
                playerRigBuilder.layers[2].active = true;
                playerRigBuilder.layers[3].active = false;
            }
            else if (inputHandler.LeanDirection == 1) // Lean Right
            {
                playerRigBuilder.layers[2].active = false;
                playerRigBuilder.layers[3].active = true;
            }
        }
        else
        {
            // Disable both lean rigs
            playerRigBuilder.layers[2].active = false;
            playerRigBuilder.layers[3].active = false;
        }
    }

    /// <summary>
    /// Triggered externally when player picks up something.
    /// </summary>
    public void InvokeInteractTrigger()
    {
        playerAnimator.SetTrigger("PickUp");
    }

    /// <summary>
    /// Sets grounded state externally (if needed).
    /// </summary>
    public void SetGrounded(bool isGrounded)
    {
        playerAnimator.SetBool("IsGrounded", isGrounded);
    }
}
