using UnityEngine;

public class ShakeObject : BaseTrigger
{
    [Header("Shake Settings")]
    public float shakeDuration = 1f;
    public float shakeMagnitude = 0.1f;
    public float dampingSpeed = 1.0f;

    private Vector3 initialPosition;
    private float currentShakeDuration;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        if (currentShakeDuration > 0)
        {
            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;

            currentShakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else if (currentShakeDuration <= 0)
        {
            transform.localPosition = initialPosition;
        }
    }

    public override void OnTriggered()
    {
        currentShakeDuration = shakeDuration; // Reset shake when triggered
    }
}
