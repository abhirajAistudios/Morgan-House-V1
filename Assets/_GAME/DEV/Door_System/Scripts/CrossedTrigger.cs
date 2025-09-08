using UnityEngine;

public class CrossedTrigger : MonoBehaviour
{
    [SerializeField] private DoorInteraction door;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            door.MarkAsOpendOnce();
        }
    }
}
