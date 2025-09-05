using UnityEngine;
using System.Collections.Generic;

public class MainTriggerArea : MonoBehaviour
{
    [Header("Trigger Events")]
    public List<BaseTrigger> triggerEvents = new List<BaseTrigger>();

    private Collider triggerCollider;
    private bool hasActivated = false; // ✅ prevent re-triggering

    void Start()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogWarning("MainTriggerArea needs a Collider with 'Is Trigger' enabled.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ✅ Only allow first activation
        if (hasActivated) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Trigger Area Activated!");

            // Call all events in the list
            foreach (var triggerEvent in triggerEvents)
            {
                if (triggerEvent != null)
                    triggerEvent.OnTriggered();
            }

            // ✅ Permanently disable this trigger
            hasActivated = true;
            if (triggerCollider != null)
                triggerCollider.enabled = false;

            Debug.Log("MainTriggerArea has been deactivated permanently.");
        }
    }
}
