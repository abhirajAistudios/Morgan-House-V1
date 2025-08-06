using UnityEngine;
using System;

/// <summary>
/// Manages interaction with a rotatable door object. 
/// Handles unlocking (via key or lockpick), opening/closing, sounds, and saving/loading state.
/// </summary>
public class DoorInteraction : BaseInteractable, ISaveable
{
    [Header("Door State")]
    public DoorState currentState = DoorState.Locked;
    private bool isOpen = false;

    [Header("Key & Lockpick Settings")]
    public string itemname;
    [Tooltip("Name of the key item required to unlock the door.")]
    public string requiredKeyName = "Key";
    public bool canBeLockpicked = true;

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

    [Header("Auto-Close Settings")]
    public bool autoCloseEnabled = true;          // Should door close when player walks away
    public float autoCloseDistance = 5f;          // Distance before auto-closing

    [Header("Designer Control")]
    public bool disableInteraction = false;       // Optional flag to disable interaction

    [Header("Lockpick Transition")]
    public LockPickCameraManager transition;     // Camera switcher when entering lockpick view

    [Header("Unique Save ID")]
    [SerializeField] private string uniqueID;   // Unique persistent ID for saving/loading

    private Transform playerTransform;
    private float hingeStartY;                    // Starting Y rotation of the hinge
    private float lastOpenDirection = 1f;

    public override string DisplayName => "Door";
    public override string Description => "A door that can be locked, unlocked, and opened.";

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = Guid.NewGuid().ToString();
    }

    private void Start()
    {
        if (doorHinge == null)
        {
            Debug.LogError("DoorInteraction: Door hinge not assigned!");
            enabled = false;
            return;
        }

        hingeStartY = doorHinge.eulerAngles.y;
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

    public override void OnInteract()
    {
        if (disableInteraction) return;

        if (currentState == DoorState.Jammed)
        {
            TryLockpick();
            return;
        }

        if (isOpen)
            CloseDoor();
        else
            TryOpenDoor();
    }

    // ----------------------------
    // Door Opening / Closing Logic
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
        for (int i = 0; i < InventoryManager.Instance.itemSlots.Length; i++)
        {
            InventoryItem item = InventoryManager.Instance.itemSlots[i];
            if (item != null && item.itemData.itemName == requiredKeyName)
            {
                GameService.Instance.EventService.OnObjectUsed.InvokeEvent(itemname);
                InventoryManager.Instance.UseItemByIndex(i);

                currentState = DoorState.Unlocked;
                PlaySound(soundUnlock);
                OpenDoorBasedOnPlayerSide();

                FindAnyObjectByType<AutoSaveManager>()?.SaveAfterObjective(
                    FindAnyObjectByType<PlayerController>()?.transform
                );
                return;
            }
        }

        PlaySound(soundLocked);
        GameService.Instance.UIService.ShowMessage("You need a key.", 1.5f);
    }

    private void TryLockpick()
    {
        if (canBeLockpicked && PlayerHasLockpick())
        {
            transition.EnterLockpickMode();

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
        currentState = DoorState.Unlocked;
        PlaySound(soundUnlock);
        OpenDoorBasedOnPlayerSide();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // -------------------------
    // Save / Load Functionality
    // -------------------------
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        AutoSaveManager.DoorStateData state = new AutoSaveManager.DoorStateData
        {
            doorID = uniqueID,
            doorState = currentState,   // Save exact DoorState
            isOpen = isOpen
        };
        data.doors.Add(state);
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        foreach (var state in data.doors)
        {
            if (state.doorID == uniqueID)
            {
                currentState = state.doorState;  // Restore the exact state (Locked, Unlocked, Jammed)

                if (currentState == DoorState.Unlocked && state.isOpen)
                    RestoreOpenState();
                else
                    RestoreClosedState();

                Debug.Log($"Door {uniqueID} restored as {currentState}, open={state.isOpen}");
                return;
            }
        }
    }

    private void RestoreOpenState()
    {
        isOpen = true;
        LeanTween.rotateY(doorHinge.gameObject, hingeStartY + (lastOpenDirection * openAngle), 0f);
        Debug.Log($"Door {uniqueID} restored as OPEN and unlocked.");
    }

    private void RestoreClosedState()
    {
        isOpen = false;
        LeanTween.rotateY(doorHinge.gameObject, hingeStartY, 0f);
        Debug.Log($"Door {uniqueID} restored as CLOSED.");
    }
}
