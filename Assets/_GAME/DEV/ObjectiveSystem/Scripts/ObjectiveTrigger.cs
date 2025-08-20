using System.Collections.Generic;
using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    [SerializeField] private List<ObjectiveDataSO> objectiveToTrigger;
    private void OnTriggerEnter(Collider hit)
    {
        if (hit.CompareTag("Player"))
        {
            GameManager.Instance.StartNewObjective(objectiveToTrigger);
        }
    }
}
