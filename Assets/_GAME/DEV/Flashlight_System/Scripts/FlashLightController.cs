using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

/// <summary>
/// Controls the player's flashlight: toggling, battery drain/recharge, UI updates, and saving/loading.
/// </summary>
public class FlashlightController : MonoBehaviour, ISaveable
{
    #region Singleton
    public static FlashlightController Instance;
    #endregion

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
    public KeyCode toggleKey = KeyCode.Alpha1;   // Toggle with key '1'
    public KeyCode reloadKey = KeyCode.R;        // Recharge with 'R'

    [Header("Battery Notification UI")]
    public TextMeshProUGUI batteryStatusText;

    [Header("Animation Variables")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private RigBuilder flashlightRig;

    [Header("References")]
    public GameObject flashLightPrefab;
    public GameObject flashLightModel;

    [Header("Sounds")] 
    [SerializeField] private Sounds ObjectPickUpSound;
    [SerializeField] private Sounds BatteryRechargeSound;
    [SerializeField] private Sounds BatteryDepletedSound;
    
    // Internal state
    private Light flashlight;
    private bool isOn = false;
    private bool hasFlashlight = false;
    private bool requiresBattery = false;
    public bool isCrouching = false;

    #region Unity Lifecycle
    private void Awake()
    {
        Instance = this;
        currentBattery = maxBattery;
        DisableFlashRig();
        batterySlider.gameObject.SetActive(false);
        flashlightRig = FindObjectOfType<RigBuilder>();
        flashLightModel.SetActive(false);
    }

    private void Update()
    {
        if (!hasFlashlight || flashlight == null) return;

        // Handle toggling and recharging
        if (Input.GetKeyDown(toggleKey)) ToggleFlashlight();
        if (Input.GetKeyDown(reloadKey)) TryRechargeBattery();

        // Update battery if flashlight is on
        if (isOn)
        {
            flashLightModel.SetActive(true);
            UpdateFlashObjectPostion();
            DrainBattery();
        }
        else
        {
            flashLightModel.SetActive(false);
            DisableFlashRig();
        }

        UpdateUI();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Registers the flashlight's Light component.
    /// </summary>
    public void RegisterFlashlight(Light light)
    {
        flashlight = light;
        flashlight.enabled = false;
        isOn = false;
        flashLightPrefab.SetActive(false);
    }

    /// <summary>
    /// Grants the player the flashlight and enables its UI.
    /// </summary>
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
    #endregion

    #region Battery Handling
    /// <summary>
    /// Drains battery while flashlight is on.
    /// </summary>
    private void DrainBattery()
    {
        currentBattery -= batteryDrainPerSecond * Time.deltaTime;
        currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);

        flashlight.intensity = Mathf.Lerp(0f, 1.5f, currentBattery / maxBattery);
        EventService.Instance.OnObjectUsed.InvokeEvent(ItemID);

        if (currentBattery <= 0f)
        {
            SoundService.Instance.Play(BatteryDepletedSound);
            flashlight.enabled = false;
            isOn = false;
            requiresBattery = true;
            flashLightPrefab.SetActive(false);
            ShowBatteryStatus("Battery depleted. Press R to recharge.", Color.red);
        }
    }

    /// <summary>
    /// Attempts to recharge the flashlight using a battery from inventory.
    /// </summary>
    private void TryRechargeBattery()
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
                SoundService.Instance.Play(BatteryRechargeSound);
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
    #endregion

    #region Controls
    /// <summary>
    /// Toggles flashlight on/off.
    /// </summary>
    private void ToggleFlashlight()
    {
        if (requiresBattery)
        {
            ShowBatteryStatus("Battery empty! Press R to use a battery.", Color.red);
            return;
        }

        if (currentBattery > 0)
        {
            isOn = !isOn;
            flashlight.enabled = isOn;
            flashLightPrefab?.SetActive(isOn);

            if (isOn) UpdateFlashObjectPostion();
        }
    }
    #endregion

    #region UI
    /// <summary>
    /// Updates battery UI slider and color.
    /// </summary>
    private void UpdateUI()
    {
        if (batterySlider == null) return;

        float fill = currentBattery / maxBattery;
        batterySlider.value = fill;
        sliderFill.color = currentBattery <= 20f ? lowBatteryColor : normalColor;
    }

    /// <summary>
    /// Shows a temporary status message for the flashlight battery.
    /// </summary>
    private void ShowBatteryStatus(string message, Color color)
    {
        if (batteryStatusText == null) return;

        batteryStatusText.text = message;
        batteryStatusText.color = color;
        batteryStatusText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideBatteryStatus));
        Invoke(nameof(HideBatteryStatus), 2f);
    }

    private void HideBatteryStatus()
    {
        if (batteryStatusText != null)
            batteryStatusText.gameObject.SetActive(false);
    }
    #endregion

    #region Rig Handling
    private void UpdateFlashObjectPostion()
    {
        if (hasFlashlight && !isCrouching)
        {
            flashlightRig.layers[0].active = true;
            flashlightRig.layers[1].active = false;
        }
        else if (hasFlashlight && isCrouching)
        {
            flashlightRig.layers[0].active = false;
            flashlightRig.layers[1].active = true;
        }
        else
        {
            DisableFlashRig();
        }
    }

    private void DisableFlashRig()
    {
        flashlightRig.layers[0].active = false;
        flashlightRig.layers[1].active = false;
    }
    #endregion

    #region Save & Load
    /// <summary>
    /// Saves flashlight state.
    /// </summary>
    public void SaveState(ref SaveData data)
    {
        data.flashlightData.hasFlashlight = hasFlashlight;
        data.flashlightData.requiresBattery = requiresBattery;
        data.flashlightData.currentBattery = currentBattery;
        data.flashlightData.isOn = isOn;
    }

    /// <summary>
    /// Loads flashlight state.
    /// </summary>
    public void LoadState(SaveData data)
    {
        hasFlashlight = data.flashlightData.hasFlashlight;
        requiresBattery = data.flashlightData.requiresBattery;
        currentBattery = data.flashlightData.currentBattery;
        isOn = data.flashlightData.isOn;

        if (hasFlashlight)
        {
            // Re-activate UI
            batterySlider.gameObject.SetActive(true);
            batterySlider.value = currentBattery / maxBattery;

            // Ensure prefab and Light are active
            if (flashLightPrefab != null)
            {
                flashLightPrefab.SetActive(true);
                Light lightComp = flashLightPrefab.GetComponentInChildren<Light>(true);
                if (lightComp != null) RegisterFlashlight(lightComp);
            }

            // Apply saved state
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
    #endregion
}
