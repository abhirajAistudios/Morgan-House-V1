using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages interaction with a rotatable door object. 
/// Handles unlocking (via key or lockpick), opening/closing, sounds, and automatic closing.
/// </summary>
public class DoorInteraction : BaseInteractable
{
    public DoorState currentState = DoorState.Locked;

    [Tooltip("Name of the key item required to unlock the door.")]
    public string requiredKeyName = "Key";

    [Header("References")]
    public Transform doorHinge;                   // Hinge of the door to rotate
    public AudioSource audioSource;               // Audio source for door sounds
    public AudioClip soundLocked;                 // Sound when door is locked
    public AudioClip soundUnlock;                 // Sound when door is unlocked
    public AudioClip soundOpen;                   // Sound when door opens
    public AudioClip soundClose;                  // Sound when door closes

    [Header("Rotation Settings")]
    public float openAngle = 90f;                 // Angle the door opens
    public float rotateTime = 1f;                 // Time taken to rotate

    [Header("Designer Control")]
    public bool disableInteraction = false;       // Optional flag to disable interaction

    [Header("Auto-Close Settings")]
    public bool autoCloseEnabled = true;          // Should door close when player walks away
    public float autoCloseDistance = 5f;          // Distance before auto-closing

    [Header("Lockpick Settings")]
    public bool canBeLockpicked = true;           // Can this door be lockpicked
    
    private bool isOpen = false;
    private Transform playerTransform;
    private float hingeStartY;                    // Starting Y rotation of the hinge
    private float lastOpenDirection = 1f;

    public LockPickCameraManager transition;     // Camera switcher when entering lockpick view

    public override string DisplayName => "Door";
    public override string Description => "This is a rotatable door interactable.";

    /// <summary>
    /// Shared tracker to communicate between scenes if the door was lockpicked.
    /// </summary>
    public static class DoorUnlockTracker
    {
        public static bool wasUnlockedViaLockpick = false;
    }

    private void Start()
    {
        // Validate that the door hinge has been assigned
        if (doorHinge == null)
        {
            Debug.LogError("DoorInteraction: Door hinge not assigned!");
            enabled = false;
            return;
        }

        hingeStartY = doorHinge.eulerAngles.y;

        // If door was previously unlocked via lockpick (flag set from Lockpick scene)
        if (DoorUnlockTracker.wasUnlockedViaLockpick)
        {
            Debug.Log("Door was unlocked via lockpick. Opening door...");
            UnlockViaLockpick();
            DoorUnlockTracker.wasUnlockedViaLockpick = false;
        }
    }

    private void Update()
    {
        // Auto-close logic when player moves away from the open door
        if (autoCloseEnabled && isOpen && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance > autoCloseDistance)
            {
                CloseDoor();
            }
        }
    }

    public override void OnFocus() { }

    public override void OnLoseFocus() { }

    /// <summary>
    /// Called when the player interacts with the door.
    /// </summary>
    public override void OnInteract()
    {
        if (disableInteraction) return;

        // Handle jammed door
        if (currentState == DoorState.Jammed)
        {
            if (canBeLockpicked && PlayerHasLockpick())
            {
                // Enter lockpick minigame view
                transition.EnterLockpickMode();

                // Remove one lockpick from inventory
                for (int i = 0; i < InventoryManager.Instance.itemSlots.Length; i++)
                {
                    InventoryItem item = InventoryManager.Instance.itemSlots[i];
                    if (item != null && item.itemData.itemName == "Lockpick")
                    {
                        InventoryManager.Instance.UseItem(item.itemData);
                        break;
                    }
                }
            }
            else
            {
                PlaySound(soundLocked);
                GameService.Instance.UIService.ShowMessage("This door is jammed.", 1.5f);
            }

            return;
        }

        // Toggle open/close
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            TryOpenDoor();
        }
    }

    /// <summary>
    /// Determines how to open the door based on its current state.
    /// </summary>
    public void TryOpenDoor()
    {
        switch (currentState)
        {
            case DoorState.Unlocked:
                GameService.Instance.UIService.ShowMessage("The door is unlocked.", 1.5f);
                OpenDoorBasedOnPlayerSide();
                break;

            case DoorState.Locked:
                TryUseKey();
                break;
        }
    }

    /// <summary>
    /// Attempts to use a key from the inventory to unlock the door.
    /// </summary>
    private void TryUseKey()
    {
        for (int i = 0; i < InventoryManager.Instance.itemSlots.Length; i++)
        {
            InventoryItem item = InventoryManager.Instance.itemSlots[i];
            if (item != null && item.itemData.itemName == requiredKeyName)
            {
                InventoryManager.Instance.UseItemByIndex(i);
                currentState = DoorState.Unlocked;
                PlaySound(soundUnlock);
                GameService.Instance.UIService.ShowMessage("Used the key.", 1.5f);
                OpenDoorBasedOnPlayerSide();
                return;
            }
        }

        PlaySound(soundLocked);
        GameService.Instance.UIService.ShowMessage("You need a key.", 1.5f);
    }

    /// <summary>
    /// Checks whether the player currently has a lockpick in their inventory.
    /// </summary>
    private bool PlayerHasLockpick()
    {
        foreach (var item in InventoryManager.Instance.itemSlots)
        {
            if (item != null && item.itemData.itemName == "Lockpick")
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Opens the door depending on which side the player is standing.
    /// </summary>
    private void OpenDoorBasedOnPlayerSide()
    {
        isOpen = true;

        playerTransform = FindAnyObjectByType<PlayerController>()?.transform;
        if (playerTransform == null) return;

        Vector3 doorForward = doorHinge.forward;
        Vector3 toPlayer = (playerTransform.position - doorHinge.position).normalized;
        float dot = Vector3.Dot(doorForward, toPlayer);
        float direction = dot > 0 ? 1f : -1f;

        lastOpenDirection = direction;
        float targetYRotation = hingeStartY + (direction * openAngle);

        LeanTween.rotateY(doorHinge.gameObject, targetYRotation, rotateTime).setEaseOutExpo();
        PlaySound(soundOpen);
    }

    /// <summary>
    /// Closes the door back to its original rotation.
    /// </summary>
    private void CloseDoor()
    {
        isOpen = false;
        LeanTween.rotateY(doorHinge.gameObject, hingeStartY, rotateTime).setEaseOutExpo();
        PlaySound(soundClose);
    }

    /// <summary>
    /// Plays a sound using the configured audio source.
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Unlocks the door via the lockpick success callback.
    /// Called when returning from the lockpick scene.
    /// </summary>
    public void UnlockViaLockpick()
    {
        currentState = DoorState.Unlocked;
        GameService.Instance.UIService.ShowMessage("Unlocked via lockpick!", 1.5f);
        PlaySound(soundUnlock);
        OpenDoorBasedOnPlayerSide();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}