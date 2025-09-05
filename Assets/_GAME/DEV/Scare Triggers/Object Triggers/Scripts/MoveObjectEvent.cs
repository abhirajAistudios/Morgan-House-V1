using UnityEngine;

public class MoveObjectEvent : BaseTrigger
{
    [Header("Movement Settings")]
    Rigidbody rb;
    public Vector3 moveDirection = Vector3.forward; // Direction of movement
    public float moveSpeed = 5f;     // Speed of movement
    public float moveDistance = 10f; // Distance before auto-stop

    private Vector3 startPosition;
    private bool isMoving = false;
    
    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.isKinematic = true; // stay still until triggered
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            rb.MovePosition(rb.position + moveDirection.normalized * moveSpeed * Time.fixedDeltaTime);

            // Check if traveled enough distance
            float traveled = Vector3.Distance(startPosition, rb.position);
            if (traveled >= moveDistance)
            {
                StopMovement();
            }
        }
    }

    public override void OnTriggered()
    {
        rb.isKinematic = false;
        startPosition = transform.position;
        isMoving = true;
    }

    private void StopMovement()
    {
        isMoving = false;
        rb.isKinematic = true;
    }

    
    
}
