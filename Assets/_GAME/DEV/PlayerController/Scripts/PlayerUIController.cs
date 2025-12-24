using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerUIController : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public PlayerHealthController healthController;
    //public PlayerScreenEffects screenEffects;

    [Header("Stamina UI")]
    public GameObject staminaPanel;
    public Slider staminaSlider;
    public TextMeshProUGUI staminaText;
    public bool showStaminaAsPercentage = true;
    public bool autoHideStaminaBar = true;
    public float staminaBarFadeDelay = 2f;

    [Header("Health UI")]
    public GameObject healthPanel;
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public bool showHealthAsPercentage = false;
    public bool autoHideHealthBar = false;
    public float healthBarFadeDelay = 3f;

    [Header("Crosshair")]
    public Image crosshair;
    public bool enableDynamicCrosshair = true;
    public float defaultCrosshairSize = 20f;
    public float maxCrosshairSize = 40f;
    public float crosshairSizeSpeed = 5f;
    public Color normalCrosshairColor = Color.white;
    public Color tiredCrosshairColor = Color.red;

    [Header("Status Icons")]
    public Image statusIcon;
    public Sprite defaultSprite;
    public Sprite walkingSprite;
    public Sprite runningSprite;
    public Sprite crouchingSprite;
    public Sprite leaningLeftSprite;
    public Sprite leaningRightSprite;
    public Image breathingIndicator;
    public float breathingPulseSpeed = 2f;

    [Header("Low Health Effects")]
    public bool enableLowHealthEffects = true;
    public Image lowHealthOverlay;
    public float lowHealthThreshold = 0.3f;
    public Color lowHealthColor = new Color(1f, 0f, 0f, 0.1f);
    public float lowHealthPulseSpeed = 2f;

    /*[Header("Fear Effects")]
    public bool enableFearUI = true;
    public Image fearOverlay;
    public Image vignetteOverlay;*/

    [Header("Animation Settings")]
    public float fadeSpeed = 2f;
    public float scaleSpeed = 5f;

    // Internal
    private float currentCrosshairSize;
    private float staminaBarAlpha = 0f;
    private float healthBarAlpha = 1f;
    private float lastStaminaFullTime;
    private float lastHealthFullTime;
    private bool staminaBarVisible = true;
    private bool healthBarVisible = true;
    private float breathingTimer;
    private float lowHealthTimer;

    private Image healthFillImage;
    private Image staminaFillImage;


    private CanvasGroup staminaCanvasGroup;
    private CanvasGroup healthCanvasGroup;

    void Start()
    {
        // Find components if not assigned
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (healthController == null)
            healthController = FindObjectOfType<PlayerHealthController>();

        /*if (screenEffects == null)
            screenEffects = FindObjectOfType<PlayerScreenEffects>();*/

        InitializeUI();
        SetupCanvasGroups();

        // Subscribe to health events
        if (healthController != null)
        {
            healthController.OnHealthChanged.AddListener(OnHealthChanged);
            healthController.OnDamageTaken.AddListener(OnDamageTaken);
            healthController.OnCriticalHealth.AddListener(OnCriticalHealth);
            healthController.OnLeaveCriticalHealth.AddListener(OnLeaveCriticalHealth);
            healthController.OnDeath.AddListener(OnDeath);
            healthController.OnRespawn.AddListener(OnRespawn);
            healthController.OnHealed.AddListener(OnHealed);
        }

        currentCrosshairSize = defaultCrosshairSize;
    }

    void OnDestroy()
    {
        // Unsubscribe from health events
        if (healthController != null)
        {
            healthController.OnHealthChanged.RemoveListener(OnHealthChanged);
            healthController.OnDamageTaken.RemoveListener(OnDamageTaken);
            healthController.OnCriticalHealth.RemoveListener(OnCriticalHealth);
            healthController.OnLeaveCriticalHealth.RemoveListener(OnLeaveCriticalHealth);
            healthController.OnDeath.RemoveListener(OnDeath);
            healthController.OnRespawn.RemoveListener(OnRespawn);
            healthController.OnHealed.RemoveListener(OnHealed);
        }
    }

    void Update()
    {
        UpdateStaminaUI();
        UpdateHealthUI();
        UpdateCrosshair();
        UpdateStatusIcons();
        UpdateBreathing();
        UpdateLowHealthEffects();
        //UpdateFearEffects();
    }

    void InitializeUI()
    {
        if (crosshair) crosshair.rectTransform.sizeDelta = Vector2.one * defaultCrosshairSize;
        if (statusIcon) statusIcon.gameObject.SetActive(true);
        if (lowHealthOverlay) lowHealthOverlay.color = new Color(1f, 0f, 0f, 0f);
        /*if (fearOverlay) fearOverlay.color = new Color(0f, 0f, 0f, 0f);
        if (vignetteOverlay) vignetteOverlay.color = new Color(0f, 0f, 0f, 0f);*/

        // Initialize health UI with values from health controller
        if (healthController != null)
        {
            if (healthSlider)
            {
                healthSlider.minValue = 0;
                healthSlider.maxValue = healthController.MaxHealth;
                healthSlider.value = healthController.CurrentHealth;
            }
        }
    }

    void SetupCanvasGroups()
    {
        if (staminaPanel)
        {
            staminaCanvasGroup = staminaPanel.GetComponent<CanvasGroup>();
            if (staminaCanvasGroup == null)
                staminaCanvasGroup = staminaPanel.AddComponent<CanvasGroup>();
        }

        if (healthPanel)
        {
            healthCanvasGroup = healthPanel.GetComponent<CanvasGroup>();
            if (healthCanvasGroup == null)
                healthCanvasGroup = healthPanel.AddComponent<CanvasGroup>();
        }
        else if (healthSlider)
        {
            healthCanvasGroup = healthSlider.GetComponent<CanvasGroup>();
            if (healthCanvasGroup == null)
                healthCanvasGroup = healthSlider.gameObject.AddComponent<CanvasGroup>();
        }
    }

    void UpdateStaminaUI()
    {
        if (playerController == null) return;

        float percent = playerController.StaminaPercentage;

        if (staminaSlider != null)
            staminaSlider.value = percent;

        // === Set stamina bar fill color ===
        if (staminaSlider != null && staminaSlider.fillRect != null)
        {
            staminaFillImage = staminaSlider.fillRect.GetComponent<Image>();

            if (staminaFillImage != null)
            {
                if (percent > 0.6f)
                    staminaFillImage.color = Color.green;
                else if (percent > 0.3f)
                    staminaFillImage.color = Color.yellow;
                else
                    staminaFillImage.color = Color.red;
            }
        }

        if (staminaText != null)
        {
            staminaText.text = "Stamina " + (showStaminaAsPercentage
                                ? $"{Mathf.RoundToInt(percent * 100)}%"
                                : $"{Mathf.RoundToInt(playerController.CurrentStamina)}/{Mathf.RoundToInt(playerController.MaxStamina)}");
        }

        // Fade out stamina bar when full and idle
        if (autoHideStaminaBar && staminaCanvasGroup != null)
        {
            if (percent >= 1f && !playerController.IsRunning)
            {
                if (staminaBarVisible)
                {
                    lastStaminaFullTime = Time.time;
                    staminaBarVisible = false;
                }

                if (Time.time - lastStaminaFullTime > staminaBarFadeDelay)
                    staminaBarAlpha = Mathf.Lerp(staminaBarAlpha, 0f, fadeSpeed * Time.deltaTime);
            }
            else
            {
                staminaBarVisible = true;
                staminaBarAlpha = Mathf.Lerp(staminaBarAlpha, 1f, fadeSpeed * Time.deltaTime);
            }

            staminaCanvasGroup.alpha = staminaBarAlpha;
        }
    }


    void UpdateHealthUI()
    {
        if (healthController == null) return;

        float currentHealth = healthController.CurrentHealth;
        float maxHealth = healthController.MaxHealth;
        float healthPercent = healthController.HealthPercentage;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            healthFillImage = healthSlider.fillRect.gameObject.GetComponent<Image>();

            // Update health bar color based on health percentage
            if (healthFillImage != null)
            {
                if (healthPercent > 0.6f)
                    healthFillImage.color = Color.green;
                else if (healthPercent > 0.3f)
                    healthFillImage.color = Color.yellow;
                else
                    healthFillImage.color = Color.red;
            }
            else if (healthSlider.fillRect)
            {
                Image fillImage = healthSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (healthPercent > 0.6f)
                        fillImage.color = Color.green;
                    else if (healthPercent > 0.3f)
                        fillImage.color = Color.yellow;
                    else
                        fillImage.color = Color.red;
                }
            }
        }

        if (healthText != null)
        {
            healthText.text = "Health " + (showHealthAsPercentage
                ? $"{Mathf.RoundToInt(healthPercent * 100)}%"
                : $"{Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}");
        }

        // Auto-hide health bar when full and not in combat
        if (autoHideHealthBar && healthCanvasGroup != null)
        {
            if (healthPercent >= 1f && !healthController.IsCritical)
            {
                if (healthBarVisible)
                {
                    lastHealthFullTime = Time.time;
                    healthBarVisible = false;
                }

                if (Time.time - lastHealthFullTime > healthBarFadeDelay)
                    healthBarAlpha = Mathf.Lerp(healthBarAlpha, 0f, fadeSpeed * Time.deltaTime);
            }
            else
            {
                healthBarVisible = true;
                healthBarAlpha = Mathf.Lerp(healthBarAlpha, 1f, fadeSpeed * Time.deltaTime);
            }

            healthCanvasGroup.alpha = healthBarAlpha;
        }
    }

    void UpdateLowHealthEffects()
    {
        if (!enableLowHealthEffects || lowHealthOverlay == null || healthController == null) return;

        float healthPercent = healthController.HealthPercentage;

        if (healthPercent <= lowHealthThreshold)
        {
            lowHealthTimer += Time.deltaTime * lowHealthPulseSpeed;
            float pulseAlpha = (Mathf.Sin(lowHealthTimer) + 1f) * 0.5f;
            float targetAlpha = (1f - healthPercent / lowHealthThreshold) * lowHealthColor.a * pulseAlpha;

            Color currentColor = lowHealthOverlay.color;
            currentColor.a = Mathf.Lerp(currentColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
            lowHealthOverlay.color = currentColor;
        }
        else
        {
            Color currentColor = lowHealthOverlay.color;
            currentColor.a = Mathf.Lerp(currentColor.a, 0f, fadeSpeed * Time.deltaTime);
            lowHealthOverlay.color = currentColor;
        }
    }

    void UpdateCrosshair()
    {
        if (crosshair == null || playerController == null) return;

        if (!enableDynamicCrosshair) return;

        float targetSize = defaultCrosshairSize;

        if (playerController.IsRunning)
            targetSize = maxCrosshairSize;
        else if (playerController.IsMoving)
            targetSize = Mathf.Lerp(defaultCrosshairSize, maxCrosshairSize, 0.5f);

        if (playerController.StaminaPercentage < 0.3f)
            targetSize *= 1.2f;

        currentCrosshairSize = Mathf.Lerp(currentCrosshairSize, targetSize, crosshairSizeSpeed * Time.deltaTime);
        crosshair.rectTransform.sizeDelta = Vector2.one * currentCrosshairSize;

        Color targetColor = (playerController.StaminaPercentage < 0.3f || playerController.IsRunning)
            ? tiredCrosshairColor : normalCrosshairColor;
        crosshair.color = Color.Lerp(crosshair.color, targetColor, crosshairSizeSpeed * Time.deltaTime);
    }

    void UpdateStatusIcons()
    {
        if (statusIcon == null || playerController == null) return;

        // Only show leaning UI if player is actually leaning (not blocked)
        if (playerController.IsLeaning && Mathf.Abs(playerController.LeanAngle) > 0.1f)
        {
            statusIcon.gameObject.SetActive(true);

            if (playerController.LeanDirection < 0f)
                statusIcon.sprite = leaningLeftSprite;
            else if (playerController.LeanDirection > 0f)
                statusIcon.sprite = leaningRightSprite;
            else
                statusIcon.sprite = defaultSprite;
        }
        else if (playerController.IsCrouching)
        {
            statusIcon.gameObject.SetActive(true);
            statusIcon.sprite = crouchingSprite;
        }
        else if (playerController.IsRunning)
        {
            statusIcon.gameObject.SetActive(true);
            statusIcon.sprite = runningSprite;
        }
        else if (playerController.IsMoving)
        {
            statusIcon.gameObject.SetActive(true);
            statusIcon.sprite = walkingSprite;
        }
        else
        {
            statusIcon.gameObject.SetActive(true);
            statusIcon.sprite = defaultSprite;
        }
    }


    void UpdateBreathing()
    {
        if (breathingIndicator == null || playerController == null) return;

        bool shouldShow = playerController.StaminaPercentage < 0.3f || playerController.IsRunning;

        CanvasGroup canvasGroup = breathingIndicator.GetComponent<CanvasGroup>();
        if (canvasGroup == null) return;

        if (shouldShow)
        {
            canvasGroup.gameObject.SetActive(true);
            breathingTimer += Time.deltaTime * breathingPulseSpeed;

            float alpha = (Mathf.Sin(breathingTimer) + 1f) * 0.5f * (1f - playerController.StaminaPercentage);
            canvasGroup.alpha = alpha;
        }
        else
        {
            canvasGroup.gameObject.SetActive(false);
        }
    }


    /*void UpdateFearEffects()
    {
        if (!enableFearUI) return;

        // Create simple fear/vignette effect based on player state
        float staminaFactor = 1f - playerController.StaminaPercentage;
        float healthFactor = healthController != null ? 1f - healthController.HealthPercentage : 0f;

        // Create a combined "fear" level based on low stamina and health
        float fearLevel = 0f;
        if (playerController.StaminaPercentage < 0.3f || (healthController != null && healthController.HealthPercentage < 0.3f))
        {
            fearLevel = Mathf.Max(staminaFactor, healthFactor) * 0.5f; // Max 0.5 fear level
        }

        // Create vignette effect
        float vignetteLevel = 0f;
        if (playerController.StaminaPercentage < 0.5f || (healthController != null && healthController.HealthPercentage < 0.5f))
        {
            vignetteLevel = Mathf.Max(staminaFactor, healthFactor) * 0.6f; // Max 0.6 vignette level
        }

        if (fearOverlay)
        {
            Color color = fearOverlay.color;
            color.a = Mathf.Lerp(color.a, fearLevel * 0.2f, fadeSpeed * Time.deltaTime);
            fearOverlay.color = color;
        }

        if (vignetteOverlay)
        {
            Color color = vignetteOverlay.color;
            color.a = Mathf.Lerp(color.a, vignetteLevel * 0.4f, fadeSpeed * Time.deltaTime);
            vignetteOverlay.color = color;
        }
    }*/

   
    // --- Health Event Handlers ---
    public void OnHealthChanged(float newHealth)
    {
        // Health changed event handler
        Debug.Log($"Health changed to: {newHealth}");
    }

    public void OnDamageTaken(float damage, DamageType damageType, Vector3 damageDirection)
    {
        // Just log the damage, no UI effects needed
        Debug.Log($"Took {damage} {damageType} damage from direction {damageDirection}");
    }

    public void OnCriticalHealth()
    {
        // Critical health event handler
        Debug.Log("Entered critical health state!");
    }

    public void OnLeaveCriticalHealth()
    {
        // Left critical health event handler
        Debug.Log("Left critical health state!");
    }

    public void OnDeath()
    {
        // Death event handler
        Debug.Log("Player died!");

        // You might want to show a death screen or disable UI elements here
        SetCrosshairVisible(false);
    }

    public void OnRespawn()
    {
        // Respawn event handler
        Debug.Log("Player respawned!");

        // Re-enable UI elements
        SetCrosshairVisible(true);
    }

    public void OnHealed(float amount)
    {
        // Healing event handler
        Debug.Log($"Healed for {amount} health");
    }

    // --- Public API ---
    public void TakeDamage(float damage)
    {
        if (healthController != null)
            healthController.TakeDamage(damage);
    }

    public void Heal(float amount)
    {
        if (healthController != null)
            healthController.Heal(amount);
    }


    public void SetCrosshairVisible(bool visible)
    {
        if (crosshair) crosshair.gameObject.SetActive(visible);
    }


    public void SetHealthBarVisible(bool visible)
    {
        if (healthCanvasGroup != null)
            healthCanvasGroup.alpha = visible ? 1f : 0f;
    }

    public void SetStaminaBarVisible(bool visible)
    {
        if (staminaCanvasGroup != null)
            staminaCanvasGroup.alpha = visible ? 1f : 0f;
    }

    // --- Getters ---
    public float CurrentHealth => healthController != null ? healthController.CurrentHealth : 0f;
    public float MaxHealth => healthController != null ? healthController.MaxHealth : 100f;
    public float HealthPercentage => healthController != null ? healthController.HealthPercentage : 0f;
    public bool IsCritical => healthController != null ? healthController.IsCritical : false;
    public bool IsDead => healthController != null ? healthController.IsDead : false;
}