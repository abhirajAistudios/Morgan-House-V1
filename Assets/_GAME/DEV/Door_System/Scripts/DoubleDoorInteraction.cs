using UnityEngine;

public class DoubleDoorInteraction : BaseInteractable
{
    [SerializeField] private DoorInteraction[] doors;

    public override void OnInteract()
    {
        foreach (DoorInteraction door in doors)
        {
            door.OnInteract();
        }
    }
}
