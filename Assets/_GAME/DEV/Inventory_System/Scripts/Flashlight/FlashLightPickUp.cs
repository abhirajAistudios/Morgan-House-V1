using UnityEngine;
using System;

public class FlashlightPickup : BaseInteractable, ISaveable
{
    [Header("UI Info")]
    public string tooltip = "Pick up Flashlight";
    public string displayName = "Flashlight";
    [TextArea(5, 10)] public string description = "A flashlight that helps you see in the dark.";

    [Header("Pickup Settings")]
    public GameObject flashlightPrefab; // Assign in inspector

    [Header("Unique ID")]
    [SerializeField] private string uniqueID;

    private bool isPicked = false;

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = Guid.NewGuid().ToString();
    }

    private void Start()
    {
        // When loading, LoadState() will be called by AutoSaveManager,
        // so this Start() doesn't need to check anything.
    }

    public override string DisplayName => displayName;
    public override string Description => description;
    public override string GetTooltipText() => tooltip;

    public override void OnFocus() { }
    public override void OnLoseFocus() { }

    public override void OnInteract()
    {
        if (isPicked) return;
        base.OnInteract();
        
        // 1. Enable flashlight system
        FlashlightController.Instance.EnableFlashlight();
        
        ItemTracker.Instance.OnPlayerCollectedItem(displayName);
        ObjectiveManager.Instance.OnObjectiveUpdatedImmediately();

        // 2. Instantiate light under the camera
        GameObject cam = Camera.main.gameObject;
        GameObject lightObj = Instantiate(flashlightPrefab, cam.transform);
        lightObj.transform.localPosition = new Vector3(0, -0.3f, 0.5f);
        lightObj.transform.localRotation = Quaternion.identity;

        // 3. Ensure the prefab is active
        lightObj.SetActive(true);

        // 4. Register with controller
        FlashlightController.Instance.RegisterFlashlight(lightObj.GetComponent<Light>());

        // 5. Mark as picked up
        isPicked = true;
        gameObject.SetActive(false);
    }

    // --- ISaveable Implementation ---
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        if (isPicked && !data.collectedItems.Contains(uniqueID))
        {
            data.collectedItems.Add(uniqueID);
        }
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        if (data.collectedItems.Contains(uniqueID))
        {
            isPicked = true;
            gameObject.SetActive(false);
        }
    }
}
