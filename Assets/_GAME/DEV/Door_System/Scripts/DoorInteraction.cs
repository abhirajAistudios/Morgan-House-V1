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

    [Header("Rotation Settings")]
    public float openAngle = 90f;                       // Angle door opens
    public float rotateSpeed = 1f;                       // Time taken to open/close door

    [Header("Auto-Close Settings")]
    public bool autoCloseEnabled = true;                // Should door auto-close when player leaves?
    public float autoCloseDistance = 5f;                // Distance threshold to trigger auto-close

    [Header("Designer Control")]
    public bool disableInteraction = false;             // Can be disabled in editor for scripted events
    public bool openOnlyOnce = false;                   // ✅ If true, door can only be opened once
    private bool hasOpenedOnce = false;                 // ✅ Tracks if door was opened before

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
        if (disableInteraction) return;

        // ✅ Prevent reopening if openOnlyOnce is true
        if (openOnlyOnce && hasOpenedOnce)
            return;

        if (currentState == DoorState.Jammed)
        {
            TryLockpick();
            return;
        }

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
            case DoorState.Unlocked:
                OpenDoorBasedOnPlayerSide();
                break;
            case DoorState.Locked:
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
                EventService.Instance.OnObjectUsed.InvokeEvent(itemname);
                InventoryManager.Instance.UseItemByIndex(i);

                currentState = DoorState.Unlocked;
                SoundService.Instance.Play(Sounds.DOORUNLOCK);
                OpenDoorBasedOnPlayerSide();
                return;
            }
        }

        SoundService.Instance.Play(Sounds.DOORLOCK);
        UIService.Instance.ShowMessage("You need a key.", 1.5f);
    }

    private void TryLockpick()
    {
        if (canBeLockpicked && PlayerHasLockpick())
        {
            transition.EnterLockpickMode();
            ConsumeLockpick();
        }
        else
        {
            SoundService.Instance.Play(Sounds.DOORLOCK);
            UIService.Instance.ShowMessage("This door is jammed.", 1.5f);
        }
    }

    private bool PlayerHasLockpick()
    {
        foreach (var item in InventoryManager.Instance.itemSlots)
        {
            if (item != null && item.itemData.itemName == "Lockpick")
                return true;
        }
        return false;
    }

    private void ConsumeLockpick()
    {
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

    public void UnlockFuseDoor()
    {
        if (currentState == DoorState.FuseLockDoor)
        {
            currentState = DoorState.Unlocked;
        }
    }

    public void UnlockDialDoor()
    {
        if (currentState == DoorState.DialLockDoor)
        {
            currentState = DoorState.Unlocked;
        }
    }

    private void OpenDoorBasedOnPlayerSide()
    {
        isOpen = true;
        //hasOpenedOnce = true; // ✅ Mark as opened forever if openOnlyOnce is enabled

        playerTransform = FindAnyObjectByType<PlayerController>()?.transform;
        if (playerTransform == null) return;

        Vector3 doorForward = doorHinge.forward;
        Vector3 toPlayer = (playerTransform.position - doorHinge.position).normalized;
        float direction = Vector3.Dot(doorForward, toPlayer) > 0 ? 1f : -1f;

        lastOpenDirection = direction;
        float targetYRotation = hingeStartY + (direction * openAngle);

        LeanTween.rotateY(doorHinge.gameObject, targetYRotation, rotateSpeed).setEaseOutExpo();
        SoundService.Instance.Play(Sounds.DOOROPEN);
    }

    private void CloseDoor()
    {
        isOpen = false;
        LeanTween.rotateY(doorHinge.gameObject, hingeStartY, rotateSpeed).setEaseOutExpo();
        SoundService.Instance.Play(Sounds.DOORCLOSE);
    }

    public void MarkAsOpendOnce()
    {
        hasOpenedOnce = true;
        CloseDoor();
    }

    // -------------------------
    // Save / Load
    // -------------------------
    public void SaveState(ref SaveData data)
    {
        data.doors.Add(new DoorStateData
        {
            doorID = uniqueID,
            doorState = currentState,
            isOpen = isOpen,
            lastOpenDirection = lastOpenDirection,
            currentYRotation = doorHinge.eulerAngles.y,
            hasOpenedOnce = hasOpenedOnce // ✅ Save once-only state
        });
    }

    public void LoadState(SaveData data)
    {
        foreach (var state in data.doors)
        {
            if (state.doorID == uniqueID)
            {
                currentState = state.doorState;
                lastOpenDirection = state.lastOpenDirection;
                hasOpenedOnce = state.hasOpenedOnce; // ✅ Restore once-only state

                if (state.isOpen)
                    RestoreOpenState(state.currentYRotation);
                else
                    RestoreClosedState();

                return;
            }
        }
    }

    private void RestoreOpenState(float savedYRotation)
    {
        isOpen = true;

        float normalizedSaved = NormalizeAngle(savedYRotation);
        float normalizedStart = NormalizeAngle(hingeStartY);
        float expectedRotation = NormalizeAngle(hingeStartY + (lastOpenDirection * openAngle));

        if (Mathf.Abs(normalizedSaved - expectedRotation) < 1f)
        {
            LeanTween.rotateY(doorHinge.gameObject, savedYRotation, 0f);
        }
        else
        {
            LeanTween.rotateY(doorHinge.gameObject, expectedRotation, 0f);
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }

    private void RestoreClosedState()
    {
        isOpen = false;
        LeanTween.rotateY(doorHinge.gameObject, hingeStartY, 0f);
    }
}
