using UnityEngine;

public class FlashlightPickup : BaseInteractable
{
    public string tooltip = "Pick up Flashlight";
    public string displayName = "Flashlight";
    [TextArea(5, 10)] public string description = "A flashlight that helps you see in the dark.";

    public GameObject flashlightPrefab; // Assign in inspector
    private bool isPicked = false;

    public string DisplayName => displayName;
    public string Description => description;
    public string GetTooltipText() => tooltip;

    public override void OnFocus() { }
    public override void OnLoseFocus() { }

    public override void OnInteract()
    {
        if (isPicked) return;

        // 1. Enable flashlight system
        FlashlightController.Instance.EnableFlashlight();

        // 2. Instantiate light under the camera
        GameObject cam = Camera.main.gameObject;

        // Instantiate the actual light prefab under the camera
        GameObject lightObj = Instantiate(flashlightPrefab, cam.transform);
        lightObj.transform.localPosition = new Vector3(0, -0.3f, 0.5f);
        lightObj.transform.localRotation = Quaternion.identity;

        // 3. Ensure the prefab is active after instantiation
        lightObj.SetActive(true);

        // 4. Register it with controller
        FlashlightController.Instance.RegisterFlashlight(lightObj.GetComponent<Light>());

        isPicked = true;

        // 5. Destroy only the pickup object (capsule), not the flashlight
        Destroy(gameObject);
    }


}
