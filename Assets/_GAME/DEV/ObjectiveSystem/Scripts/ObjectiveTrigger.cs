using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers a set of objectives when the player enters the trigger zone.
/// Attach this component to a GameObject with a Collider set to "Is Trigger".
/// </summary>
public class ObjectiveTrigger : MonoBehaviour
{
    [Tooltip("List of objectives to trigger when the player enters this trigger")]
    [SerializeField] private List<ObjectiveDataSO> objectiveToTrigger;
    
    /// Called when another collider enters the trigger zone
    private void OnTriggerEnter(Collider hit)
    {
        // Check if the entering collider is the player
        if (hit.CompareTag("Player"))
        {
            // Start the specified objectives using the GameManager
            GameManager.Instance.StartNewObjective(objectiveToTrigger);
        }
    }
}