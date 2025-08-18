using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;

public class FlashlightController : MonoBehaviour, ISaveable
{
    public static FlashlightController Instance;

    [Header("Battery System")]
    public float maxBattery = 100f;
    public float batteryDrainPerSecond = 5f;
    public float currentBattery;

    [Header("UI")]
    public string ItemID;
    public Slider batterySlider;
    public Image sliderFill;
    public Color normalColor = Color.green;
    public Color lowBatteryColor = Color.red;

    [Header("Controls")]
    public KeyCode toggleKey = KeyCode.Alpha1;
    public KeyCode reloadKey = KeyCode.R;

    private Light flashlight;
    private bool isOn = false;
    private bool hasFlashlight = false;
    private bool requiresBattery = false;
    public bool isCrouching = false;

    public GameObject flashLightPrefab;

    [Header("Battery Notification UI")]
    public TextMeshProUGUI batteryStatusText;

<<<<<<< Updated upstream
    [Header("Animation Variables")]
=======
    // Animation
>>>>>>> Stashed changes
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private RigBuilder FlashlightRig;

    private void Awake()
    {
        Instance = this;
        currentBattery = maxBattery;
        DisableFlashRig();
        batterySlider.gameObject.SetActive(false);
    }

    public void RegisterFlashlight(Light light)
    {
        flashlight = light;
        flashlight.enabled = false;
        isOn = false;
        flashLightPrefab.SetActive(false);
    }

    public void EnableFlashlight()
    {
        hasFlashlight = true;
        currentBattery = maxBattery;
        requiresBattery = false;

        if (flashlight != null)
            flashlight.enabled = false;

        isOn = false;
        batterySlider.value = 1f;
        batterySlider.gameObject.SetActive(true);
        flashLightPrefab.SetActive(true);
    }

    private void Update()
    {
        if (!hasFlashlight || flashlight == null) return;

        if (isOn)
        {
            UpdateFlashObjectPostion();
            GameService.Instance.EventService.OnObjectUsed.InvokeEvent(ItemID);
        }
        else
        {
            DisableFlashRig();
        }

        if (Input.GetKeyDown(toggleKey))
            ToggleFlashlight();

        if (Input.GetKeyDown(reloadKey))
            TryRechargeBattery();

        if (isOn)
        {
            currentBattery -= batteryDrainPerSecond * Time.deltaTime;
            currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);

            GameService.Instance.EventService.OnObjectUsed.InvokeEvent(ItemID);
            flashlight.intensity = Mathf.Lerp(0f, 1.5f, currentBattery / maxBattery);

            if (currentBattery <= 0f)
            {
                flashlight.enabled = false;
                isOn = false;
                requiresBattery = true;
                flashLightPrefab.SetActive(false);
<<<<<<< Updated upstream
                Debug.Log("Battery depleted. Press R to recharge.");
=======
>>>>>>> Stashed changes
            }
        }

        UpdateUI();
    }

    void ToggleFlashlight()
    {
<<<<<<< Updated upstream
        if (requiresBattery)
        {
            Debug.Log("Battery empty! Press R to use a battery.");
            return;
        }
=======
        if (requiresBattery) return;
>>>>>>> Stashed changes

        if (currentBattery > 0)
        {
            isOn = !isOn;
            flashlight.enabled = isOn;
<<<<<<< Updated upstream
            UpdateFlashObjectPostion();

=======
>>>>>>> Stashed changes
            if (flashLightPrefab != null)
                flashLightPrefab.SetActive(isOn);
        }
    }

    void TryRechargeBattery()
    {
        if (!requiresBattery)
        {
            ShowBatteryStatus("Battery not depleted yet!", Color.yellow);
            return;
        }

        foreach (var slot in InventoryManager.Instance.itemSlots)
        {
            if (slot != null && slot.itemData.itemName == "Battery")
            {
                playerAnimator.SetTrigger("Recharged");
                InventoryManager.Instance.UseItem(slot.itemData);
                currentBattery = maxBattery;
                isOn = false;
                requiresBattery = false;

                if (flashlight != null)
                    flashlight.enabled = false;

                ShowBatteryStatus("Flashlight recharged!", Color.green);
                return;
            }
        }

        ShowBatteryStatus("No batteries in inventory!", Color.red);
    }

    void UpdateUI()
    {
        if (batterySlider == null) return;

        float fill = currentBattery / maxBattery;
        batterySlider.value = fill;
        sliderFill.color = currentBattery <= 20f ? lowBatteryColor : normalColor;
    }

    void ShowBatteryStatus(string message, Color color)
    {
        if (batteryStatusText == null) return;
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
        batteryStatusText.text = message;
        batteryStatusText.color = color;
        batteryStatusText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideBatteryStatus));
        Invoke(nameof(HideBatteryStatus), 2f);
    }

    void HideBatteryStatus()
    {
        if (batteryStatusText != null)
            batteryStatusText.gameObject.SetActive(false);
    }

    void UpdateFlashObjectPostion()
    {
        if (hasFlashlight && !isCrouching)
        {
            FlashlightRig.layers[0].active = true;
            FlashlightRig.layers[1].active = false;
        }
        else if (hasFlashlight && isCrouching)
        {
            FlashlightRig.layers[0].active = false;
            FlashlightRig.layers[1].active = true;
        }
        else
        {
            DisableFlashRig();
        }
    }

    void DisableFlashRig()
    {
        FlashlightRig.layers[0].active = false;
        FlashlightRig.layers[1].active = false;
    }

<<<<<<< Updated upstream
    // ------------------ ISaveable Implementation ------------------
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        data.flashlightData.hasFlashlight = hasFlashlight;
        data.flashlightData.requiresBattery = requiresBattery;
        data.flashlightData.currentBattery = currentBattery;
        data.flashlightData.isOn = isOn;
=======
    // ---------------- SAVE / LOAD ----------------
    public void SaveState(ref AutoSaveManager.SaveData data)
    {
        data.flashlightData.hasFlashlight = hasFlashlight;
        data.flashlightData.isOn = isOn;
        data.flashlightData.currentBattery = currentBattery;
>>>>>>> Stashed changes
    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
        hasFlashlight = data.flashlightData.hasFlashlight;
<<<<<<< Updated upstream
        requiresBattery = data.flashlightData.requiresBattery;
        currentBattery = data.flashlightData.currentBattery;
        isOn = data.flashlightData.isOn;

        if (hasFlashlight)
        {
            // Re-activate UI
            batterySlider.gameObject.SetActive(true);
            batterySlider.value = currentBattery / maxBattery;

            // Ensure prefab is active
            if (flashLightPrefab != null)
            {
                flashLightPrefab.SetActive(true);

                // Find the Light component inside the prefab and re-register it
                Light lightComp = flashLightPrefab.GetComponentInChildren<Light>(true);
                if (lightComp != null)
                    RegisterFlashlight(lightComp);
            }

            // Apply on/off state
            if (flashlight != null)
                flashlight.enabled = isOn;

            UpdateFlashObjectPostion();
        }
        else
        {
            batterySlider.gameObject.SetActive(false);
            if (flashLightPrefab != null) flashLightPrefab.SetActive(false);
        }
    }

=======
        isOn = data.flashlightData.isOn;
        currentBattery = data.flashlightData.currentBattery;

        if (hasFlashlight)
        {
            EnableFlashlight();
            currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);
            flashlight.enabled = isOn;
            if (flashLightPrefab != null)
                flashLightPrefab.SetActive(isOn);
        }
        else
        {
            DisableFlashRig();
            batterySlider.gameObject.SetActive(false);
        }
    }
>>>>>>> Stashed changes
}
