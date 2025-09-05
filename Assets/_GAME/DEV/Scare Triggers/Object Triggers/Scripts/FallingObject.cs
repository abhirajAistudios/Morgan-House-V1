using UnityEngine;

public class FallingObject : BaseTrigger
{
    [Header("Falling Object Settings")]
     Rigidbody rb;
   
    private Vector3 initialPosition;
    private Quaternion initialRotation;


    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.isKinematic = true; // keep object frozen until triggered

        // Save start position & rotation in case we want reset
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public override void OnTriggered()
    {
        if (rb != null)
        {
            rb.isKinematic = false;  // enable gravity & physics
        }
    }

   
}
