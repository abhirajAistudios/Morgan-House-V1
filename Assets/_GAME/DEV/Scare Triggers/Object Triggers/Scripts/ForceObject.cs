using UnityEngine;

public class ForceObject : BaseTrigger
{
    [Header("Movement Settings")]
    private Rigidbody rb;
    
    public Vector3 moveDirection = Vector3.forward; // Direction of push
    public float forceAmount = 25f;                // Strength of the force

    private Vector3 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnTriggered()
    {
        //rb.isKinematic = false;
        startPosition = transform.position;

        // Apply force once
        Vector3 forceDir = transform.TransformDirection(moveDirection.normalized);
        rb.AddForce(forceDir * forceAmount, ForceMode.Impulse);
    }
}