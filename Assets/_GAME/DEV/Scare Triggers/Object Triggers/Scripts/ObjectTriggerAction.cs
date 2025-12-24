using UnityEngine;

/// <summary>
/// Executes different trigger actions based on the selected TriggerActionType.
/// Attach this script to an object and assign an action type in the Inspector.
/// </summary>
public class ObjectTriggerAction : MonoBehaviour
{
    [Header("General Settings")]
    [Tooltip("Select which action this object performs when triggered.")]
    public TriggerActionType actionType;

    private Rigidbody rb;

    // ---------------- ForceObject Settings ----------------
    [Header("Force Settings")]
    [Tooltip("Direction of applied force (local space).")]
    public Vector3 forceDirection = Vector3.forward;
    [Tooltip("Strength of applied force.")]
    public float forceAmount = 25f;

    // ---------------- FallingObject Settings ----------------
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // ---------------- MoveObject Settings ----------------
    [Header("Move Settings")]
    [Tooltip("Direction of movement (local space).")]
    public Vector3 moveDirection = Vector3.forward;
    [Tooltip("Speed of movement.")]
    public float moveSpeed = 5f;
    [Tooltip("Distance before stopping.")]
    public float moveDistance = 10f;

    private Vector3 moveStartPosition;
    private bool isMoving = false;

    // ---------------- ShakeObject Settings ----------------
    [Header("Shake Settings")]
    [Tooltip("How long the object shakes when triggered.")]
    public float shakeDuration = 1f;
    [Tooltip("How far the object moves during the shake.")]
    public float shakeMagnitude = 0.1f;
    [Tooltip("How quickly the shake effect decays.")]
    public float dampingSpeed = 1.0f;

    private Vector3 shakeInitialPosition;
    private float currentShakeDuration;

    // ---------------- Unity Methods ----------------
    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        switch (actionType)
        {
            case TriggerActionType.ForceObject:
                // Force applied on trigger → no setup required
                break;

            case TriggerActionType.FallingObject:
                if (rb != null) rb.isKinematic = true; // Freeze until triggered
                initialPosition = transform.position;
                initialRotation = transform.rotation;
                break;

            case TriggerActionType.MoveObject:
                if (rb != null) rb.isKinematic = true; // Controlled movement
                moveStartPosition = transform.position;
                break;

            case TriggerActionType.ShakeObject:
                shakeInitialPosition = transform.localPosition;
                break;
        }
    }

    private void Update()
    {
        // Handle shaking effect
        if (actionType == TriggerActionType.ShakeObject && currentShakeDuration > 0)
        {
            transform.localPosition = shakeInitialPosition + Random.insideUnitSphere * shakeMagnitude;
            currentShakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else if (actionType == TriggerActionType.ShakeObject)
        {
            transform.localPosition = shakeInitialPosition; // Reset position
        }
    }

    private void FixedUpdate()
    {
        // Handle smooth physics-based movement
        if (actionType == TriggerActionType.MoveObject && isMoving && rb != null)
        {
            rb.MovePosition(rb.position + moveDirection.normalized * moveSpeed * Time.fixedDeltaTime);

            // Stop if traveled enough distance
            float traveled = Vector3.Distance(moveStartPosition, rb.position);
            if (traveled >= moveDistance)
                StopMovement();
        }
    }

    // ---------------- Trigger Execution ----------------
    public void OnTriggered()
    {
        switch (actionType)
        {
            case TriggerActionType.ForceObject:
                if (rb != null)
                {
                    Vector3 dir = transform.TransformDirection(forceDirection.normalized);
                    rb.AddForce(dir * forceAmount, ForceMode.Impulse);
                }
                break;

            case TriggerActionType.FallingObject:
                if (rb != null) rb.isKinematic = false; // Allow gravity to act
                break;

            case TriggerActionType.MoveObject:
                if (rb != null)
                {
                    rb.isKinematic = false;
                    moveStartPosition = transform.position;
                    isMoving = true;
                }
                break;

            case TriggerActionType.ShakeObject:
                currentShakeDuration = shakeDuration; // Start shaking
                break;
        }
    }

    // ---------------- Helper Methods ----------------
    private void StopMovement()
    {
        isMoving = false;
        if (rb != null) rb.isKinematic = true; // Stop movement
    }
}
