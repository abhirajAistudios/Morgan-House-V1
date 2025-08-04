using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class FlashlightController : MonoBehaviour
{
    public static FlashlightController Instance;

    [Header("Battery System")]
    public float maxBattery = 100f;
    public float batteryDrainPerSecond = 5f;
    public float currentBattery;

    [Header("UI")]
    public Slider batterySlider;
    public Image sliderFill;
    public Color normalColor = Color.green;
    public Color lowBatteryColor = Color.red;

    [Header("Controls")]
    public KeyCode toggleKey = KeyCode.Alpha1;  // Toggle with key '1'
    public KeyCode reloadKey = KeyCode.R;

    private Light flashlight;
    private bool isOn = false;
    private bool hasFlashlight = false;
    private bool requiresBattery = false;
    public bool isCrouching = false;

    public GameObject flashLightPrefab;

    [Header("Battery Notification UI")]
    public TextMeshProUGUI batteryStatusText;

    //Animation Variables
    [SerializeField]
    private Animator playerAnimator;
    [SerializeField]
    private RigBuilder FlashlightRig;
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
            flashlight.enabled = false; // ⚠️ Keep it off initially

        isOn = false; // Must toggle manually
        batterySlider.value = 1f;
        batterySlider.gameObject.SetActive(true);
        flashLightPrefab.SetActive(true);
    }

    private void Update()
    {
        if (!hasFlashlight || flashlight == null) return;
        UpdateFlashObjectPostion();

        if (Input.GetKeyDown(toggleKey))
            ToggleFlashlight();

        if (Input.GetKeyDown(reloadKey))
            TryRechargeBattery();
        if (isOn)
        {
            // 🪫 Drain battery ONLY when flashlight is ON
            currentBattery -= batteryDrainPerSecond * Time.deltaTime;
            currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);

            flashlight.intensity = Mathf.Lerp(0f, 1.5f, currentBattery / maxBattery);

            if (currentBattery <= 0f)
            {
                flashlight.enabled = false;
                isOn = false;
                requiresBattery = true;
                Debug.Log("Battery depleted. Press R to recharge.");

                flashLightPrefab.SetActive(false); //  Already good
            }

        }
        UpdateUI();
    }

    void ToggleFlashlight()
    {
        if (requiresBattery)
        {
            Debug.Log(" Battery empty! Press R to use a battery.");
            return;
        }

        if (currentBattery > 0)
        {
            isOn = !isOn;
            flashlight.enabled = isOn;

            // ✅ Flashlight prefab should follow the isOn state
            if (flashLightPrefab != null)
                flashLightPrefab.SetActive(isOn);
        }
    }


    void TryRechargeBattery()
    {
        if (requiresBattery==false)
        {
            Debug.Log(" Battery not depleted yet!");
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

                Debug.Log(" Flashlight recharged. Press 1 to turn on.");
                ShowBatteryStatus("Flashlight recharged!", Color.green);
                return;
            }
        }

        Debug.Log(" No batteries in inventory.");
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
        Debug.Log("rECHARGE1");
        if (batteryStatusText == null) return;
        Debug.Log("rECHARGE2");

        batteryStatusText.text = message;
        batteryStatusText.color = color;
        batteryStatusText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideBatteryStatus)); // Cancel previous hide if any
        Invoke(nameof(HideBatteryStatus), 2f);   // Hide after 2 seconds
    }

    void HideBatteryStatus()
    {
        if (batteryStatusText != null)
            batteryStatusText.gameObject.SetActive(false);
    }

    void UpdateFlashObjectPostion()
    {
        // Set the rig weight if the player has the flashlight (regardless of whether it's ON or OFF)
        if (hasFlashlight && !isCrouching)
        {
            FlashlightRig.layers[0].active = true;
            FlashlightRig.layers[1].active = false;
        }
        else if(hasFlashlight && isCrouching)
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

}
