using UnityEngine;
using System;

/// <summary>
/// Handles door interaction: unlocking (key or lockpick), opening/closing, sounds, and save/load state.
/// </summary>
public class DoorInteraction : BaseInteractable, ISaveable
{
    [Header("Door State")]
    public DoorState currentState = DoorState.Locked;   // Initial door state (Locked by default)
    private bool isOpen = false;                        // Tracks if door is currently open

    [Header("Key & Lockpick Settings")]
    public string itemname;                             // ID for events when door is interacted with
    public string requiredKeyName = "Key";              // Name of key required to unlock
    public bool canBeLockpicked = true;                 // Whether lockpicking is allowed

    [Header("References")]
    public Transform doorHinge;                         // Hinge transform for rotating door
    public AudioSource audioSource;                     // Audio source for playing door sounds
    public AudioClip soundLocked;                       // Sound when trying to open a locked door
    public AudioClip soundUnlock;                       // Sound when unlocking a door
    public AudioClip soundOpen;                         // Sound when door opens
    public AudioClip soundClose;                        // Sound when door closes

    [Header("Rotation Settings")]
    public float openAngle = 90f;                       // Angle door opens
    public float rotateTime = 1f;                       // Time taken to open/close door

    [Header("Auto-Close Settings")]
    public bool autoCloseEnabled = true;                // Should door auto-close when player leaves?
    public float autoCloseDistance = 5f;                // Distance threshold to trigger auto-close

    [Header("Designer Control")]
    public bool disableInteraction = false;             // Can be disabled in editor for scripted events

    [Header("Lockpick Transition")]
    public LockPickCameraManager transition;            // Reference to lockpick transition handler

    [Header("Unique Save ID")]
    [SerializeField] private string uniqueID;           // Unique ID for saving/loading door state

    private Transform playerTransform;                  // Player reference (for auto-close & direction)
    private float hingeStartY;                          // Initial rotation of hinge (closed position)
    private float lastOpenDirection = 1f;               // Stores last open direction (for restoring state)

    // Display name for interaction prompts
    public override string DisplayName => "Door";
    public override string Description => "A door that can be locked, unlocked, and opened.";

    private void OnValidate()
    {
        // Generate a GUID automatically if uniqueID is empty
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = Guid.NewGuid().ToString();
    }

    private void Start()
    {
        // Disable script if hinge is missing
        if (doorHinge == null)
        {
            enabled = false;
            return;
        }
        // Save original closed rotation
        hingeStartY = doorHinge.eulerAngles.y;
    }

    private void Update()
    {
        // If auto-close is enabled and player moves too far → close the door
        if (autoCloseEnabled && isOpen && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance > autoCloseDistance)
                CloseDoor();
        }
    }

    public override void OnFocus() { }     // (Optional) highlight door when aiming
    public override void OnLoseFocus() { } // (Optional) remove highlight

    public override void OnInteract()
    {
        if (disableInteraction) return; // If disabled → do nothing

        if (currentState == DoorState.Jammed)
        {
            TryLockpick(); // Jammed = lockpick attempt
            return;
        }

        // Toggle between open/close when interacting
        if (isOpen) CloseDoor();
        else TryOpenDoor();
    }

    // ----------------------------
    // Door Logic
    // ----------------------------
    public void TryOpenDoor()
    {
        switch (currentState)
        {
            case DoorState.Unlocked:   // If unlocked → open
                OpenDoorBasedOnPlayerSide();
                break;
            case DoorState.Locked:     // If locked → check for key
                TryUseKey();
                break;
        }
    }

    private void TryUseKey()
    {
        // Search player's inventory for correct key
        for (int i = 0; i < InventoryManager.Instance.itemSlots.Length; i++)
        {
            var item = InventoryManager.Instance.itemSlots[i];
            if (item != null && item.itemData.itemName == requiredKeyName)
            {
                // Use the key & unlock
                GameService.Instance.EventService.OnObjectUsed.InvokeEvent(itemname);
                InventoryManager.Instance.UseItemByIndex(i);

                currentState = DoorState.Unlocked;
                PlaySound(soundUnlock);
                OpenDoorBasedOnPlayerSide();
                return;
            }
        }

        // If no key found → locked feedback
        PlaySound(soundLocked);
        GameService.Instance.UIService.ShowMessage("You need a key.", 1.5f);
    }

    private void TryLockpick()
    {
        // If lockpick is available → enter lockpick mode
        if (canBeLockpicked && PlayerHasLockpick())
        {
            transition.EnterLockpickMode();
            ConsumeLockpick();
        }
        else
        {
            // Otherwise show feedback
            PlaySound(soundLocked);
            GameService.Instance.UIService.ShowMessage("This door is jammed.", 1.5f);
        }
    }

    private bool PlayerHasLockpick()
    {
        // Search for lockpick in inventory
        foreach (var item in InventoryManager.Instance.itemSlots)
        {
            if (item != null && item.itemData.itemName == "Lockpick")
                return true;
        }
        return false;
    }

    private void ConsumeLockpick()
    {
        // Consume first found lockpick item
        for (int i = 0; i < InventoryManager.Instance.itemSlots.Length; i++)
        {
            var item = InventoryManager.Instance.itemSlots[i];
            if (item != null && item.itemData.itemName == "Lockpick")
            {
                InventoryManager.Instance.UseItem(item.itemData);
                break;
            }
        }
    }

    private void OpenDoorBasedOnPlayerSide()
    {
        isOpen = true;
        playerTransform = FindAnyObjectByType<PlayerController>()?.transform;
        if (playerTransform == null) return;

        // Decide whether to push or pull based on player position
        Vector3 doorForward = doorHinge.forward;
        Vector3 toPlayer = (playerTransform.position - doorHinge.position).normalized;
        float direction = Vector3.Dot(doorForward, toPlayer) > 0 ? 1f : -1f;

        lastOpenDirection = direction;
        float targetYRotation = hingeStartY + (direction * openAngle);

        // Animate rotation using LeanTween
        LeanTween.rotateY(doorHinge.gameObject, targetYRotation, rotateTime).setEaseOutExpo();
        PlaySound(soundOpen);
    }

    private void CloseDoor()
    {
        isOpen = false;
        LeanTween.rotateY(doorHinge.gameObject, hingeStartY, rotateTime).setEaseOutExpo();
        PlaySound(soundClose);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void UnlockViaLockpick()
    {
        // Called from lockpick success
        currentState = DoorState.Unlocked;
        PlaySound(soundUnlock);
        OpenDoorBasedOnPlayerSide();

        // Restore cursor lock (for gameplay after lockpick minigame)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // -------------------------
    // Save / Load
    // -------------------------
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        // Store door state inside save data list
        data.doors.Add(new AutoSaveManager.DoorStateData
        {
            doorID = uniqueID,
            doorState = currentState,
            isOpen = isOpen
        });
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        // Restore saved state if ID matches
        foreach (var state in data.doors)
        {
            if (state.doorID == uniqueID)
            {
                currentState = state.doorState;
                if (currentState == DoorState.Unlocked && state.isOpen)
                    RestoreOpenState();
                else
                    RestoreClosedState();
                return;
            }
        }
    }

    private void RestoreOpenState()
    {
        // Instantly set door to open state without animation
        isOpen = true;
        LeanTween.rotateY(doorHinge.gameObject, hingeStartY + (lastOpenDirection * openAngle), 0f);
    }

    private void RestoreClosedState()
    {
        // Instantly set door to closed state without animation
        isOpen = false;
        LeanTween.rotateY(doorHinge.gameObject, hingeStartY, 0f);
    }
}
