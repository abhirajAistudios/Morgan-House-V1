using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    public List<ObjectiveDataSO> objectiveToTrigger;
    private BoxCollider boxCollider;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }
    private void OnTriggerEnter(Collider hit)
    {
        if (hit.CompareTag("Player"))
        {
            GameManager.Instance.StartNewObjective(objectiveToTrigger);
        }
    }
}
