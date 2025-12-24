using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections;

public class PlayerScreenEffects : MonoBehaviour
{
    [Header("Main Settings")]
    [Tooltip("Player controller reference")]
    public PlayerController playerController;

    [Tooltip("Player health controller reference")]
    public PlayerHealthController healthController;

    [Tooltip("Main post-processing volume (assign manually)")]
    public Volume postProcessVolume;

    [Header("Running Effects")]
    [Tooltip("Enable motion blur when running")]
    public bool enableRunningMotionBlur = true;

    [Tooltip("Maximum motion blur intensity when running")]
    [Range(0f, 1f)]
    public float maxMotionBlurIntensity = 0.3f;

    [Header("Stamina Effects")]
    [Tooltip("Enable vignette effect when stamina is low")]
    public bool enableStaminaVignette = true;

    [Tooltip("Maximum vignette intensity when stamina is low")]
    [Range(0f, 1f)]
    public float maxVignetteIntensity = 0.5f;

    [Tooltip("Enable chromatic aberration when tired")]
    public bool enableStaminaChromaticAberration = true;

    [Tooltip("Maximum chromatic aberration when tired")]
    [Range(0f, 1f)]
    public float maxChromaticAberrationIntensity = 0.3f;

    [Header("Health Effects")]
    [Tooltip("Screen shake intensity on damage")]
    public float damageShakeIntensity = 0.5f;

    [Tooltip("Screen shake duration on damage")]
    public float damageShakeDuration = 0.3f;

    [Tooltip("Enable screen flash on damage")]
    public bool enableDamageFlash = true;

    [Tooltip("Damage flash color")]
    public Color damageFlashColor = new Color(1f, 0f, 0f, 0.3f);

    [Tooltip("Damage flash duration")]
    public float damageFlashDuration = 0.2f;

    [Tooltip("Enable health-based screen effects")]
    public bool enableHealthEffects = true;

    [Tooltip("Health vignette color")]
    public Color healthVignetteColor = new Color(1f, 0f, 0f, 0.5f);

    [Tooltip("Maximum health vignette intensity")]
    [Range(0f, 1f)]
    public float maxHealthVignetteIntensity = 0.4f;

    [Header("Transition Settings")]
    [Tooltip("Speed of effect transitions")]
    public float transitionSpeed = 2f;

    // HDRP Components
    private MotionBlur motionBlur;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;

    // UI Components for damage flash
    private Camera playerCamera;
    private GameObject damageFlashObject;
    private UnityEngine.UI.Image damageFlashImage;

    // Current effect values
    private float currentMotionBlurIntensity = 0f;
    private float currentVignetteIntensity = 0f;
    private float currentChromaticAberrationIntensity = 0f;
    private float currentHealthVignetteIntensity = 0f;

    // Screen shake
    private Vector3 originalCameraPosition;
    private bool isShaking = false;

    void Start()
    {
        // Auto-find components if not assigned
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (healthController == null)
            healthController = GetComponent<PlayerHealthController>();

        if (postProcessVolume == null)
            postProcessVolume = FindObjectOfType<Volume>();

        // Get post-processing components
        GetPostProcessingComponents();

        // Setup camera and damage flash
        SetupCameraAndDamageFlash();

        // Subscribe to health events
        SubscribeToHealthEvents();
    }

    void OnDestroy()
    {
        // Unsubscribe from health events
        UnsubscribeFromHealthEvents();
    }

    void Update()
    {
        if (playerController == null || postProcessVolume == null) return;

        UpdateRunningEffects();
        UpdateStaminaEffects();
        UpdateHealthEffects();
        ApplyEffects();
    }

    void GetPostProcessingComponents()
    {
        if (postProcessVolume == null || postProcessVolume.profile == null) return;

        // Try to get existing components from the volume profile
        postProcessVolume.profile.TryGet<MotionBlur>(out motionBlur);
        postProcessVolume.profile.TryGet<Vignette>(out vignette);
        postProcessVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration);

        // If components don't exist, add them
        if (enableRunningMotionBlur && motionBlur == null)
        {
            motionBlur = postProcessVolume.profile.Add<MotionBlur>(false);
            SetupMotionBlur();
        }

        if ((enableStaminaVignette || enableHealthEffects) && vignette == null)
        {
            vignette = postProcessVolume.profile.Add<Vignette>(false);
            SetupVignette();
        }

        if (enableStaminaChromaticAberration && chromaticAberration == null)
        {
            chromaticAberration = postProcessVolume.profile.Add<ChromaticAberration>(false);
            SetupChromaticAberration();
        }
    }

    void SetupCameraAndDamageFlash()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();

        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.transform.localPosition;
        }

        // Create damage flash UI
        if (enableDamageFlash)
        {
            SetupDamageFlash();
        }
    }

    void SetupDamageFlash()
    {
        // Create a canvas for the damage flash
        GameObject canvasObject = new GameObject("DamageFlashCanvas");
        canvasObject.transform.SetParent(this.transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // High priority

        UnityEngine.UI.CanvasScaler scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;

        UnityEngine.UI.GraphicRaycaster raycaster = canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Create the damage flash image
        damageFlashObject = new GameObject("DamageFlash");
        damageFlashObject.transform.SetParent(canvasObject.transform, false);

        damageFlashImage = damageFlashObject.AddComponent<UnityEngine.UI.Image>();
        damageFlashImage.raycastTarget = false;
        damageFlashImage.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, 0f);

        // Make it cover the full screen
        RectTransform rectTransform = damageFlashImage.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }


    void SubscribeToHealthEvents()
    {
        if (healthController == null) return;

        healthController.OnDamageTaken.AddListener(OnDamageTaken);
        healthController.OnDeath.AddListener(OnDeath);
    }

    void UnsubscribeFromHealthEvents()
    {
        if (healthController == null) return;

        healthController.OnDamageTaken.RemoveListener(OnDamageTaken);
        healthController.OnDeath.RemoveListener(OnDeath);
    }

    void SetupMotionBlur()
    {
        if (motionBlur == null) return;

        motionBlur.intensity.overrideState = true;
        motionBlur.intensity.value = 0f;
        motionBlur.active = false;
    }

    void SetupVignette()
    {
        if (vignette == null) return;

        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0f;
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = 0.4f;
        vignette.color.overrideState = true;
        vignette.color.value = Color.black;
        vignette.active = false;
    }

    void SetupChromaticAberration()
    {
        if (chromaticAberration == null) return;

        chromaticAberration.intensity.overrideState = true;
        chromaticAberration.intensity.value = 0f;
        chromaticAberration.active = false;
    }

    void UpdateRunningEffects()
    {
        if (!enableRunningMotionBlur) return;

        // Motion blur when running
        float targetMotionBlur = 0f;
        if (playerController.IsRunning && playerController.IsMoving)
        {
            // Scale motion blur based on actual speed
            float speedFactor = Mathf.Clamp01(playerController.CurrentSpeed / playerController.runSpeed);
            targetMotionBlur = speedFactor * maxMotionBlurIntensity;
        }

        currentMotionBlurIntensity = Mathf.Lerp(currentMotionBlurIntensity, targetMotionBlur,
            transitionSpeed * Time.deltaTime);
    }

    void UpdateStaminaEffects()
    {
        float staminaPercentage = playerController.StaminaPercentage;
        float tirednessLevel = 1f - staminaPercentage; // 0 = full stamina, 1 = no stamina

        // Vignette effect when tired (only if health effects aren't overriding)
        float targetVignette = 0f;
        if (enableStaminaVignette && staminaPercentage < 0.5f && !healthController.IsCritical)
        {
            // Start effect when below 50% stamina, max effect at 0% stamina
            float vignetteStrength = Mathf.InverseLerp(0.5f, 0f, staminaPercentage);
            targetVignette = vignetteStrength * maxVignetteIntensity;
        }

        // Chromatic aberration when very tired
        float targetChromaticAberration = 0f;
        if (enableStaminaChromaticAberration && staminaPercentage < 0.3f)
        {
            // Start effect when below 30% stamina
            float aberrationStrength = Mathf.InverseLerp(0.3f, 0f, staminaPercentage);
            targetChromaticAberration = aberrationStrength * maxChromaticAberrationIntensity;
        }

        // Smooth transitions
        currentVignetteIntensity = Mathf.Lerp(currentVignetteIntensity, targetVignette,
            transitionSpeed * Time.deltaTime);
        currentChromaticAberrationIntensity = Mathf.Lerp(currentChromaticAberrationIntensity,
            targetChromaticAberration, transitionSpeed * Time.deltaTime);
    }

    void UpdateHealthEffects()
    {
        if (!enableHealthEffects || healthController == null) return;

        // Health-based vignette effect
        float targetHealthVignette = 0f;
        if (healthController.IsCritical)
        {
            // Intense vignette when in critical health
            float healthRatio = healthController.CurrentHealth / healthController.criticalHealthThreshold;
            targetHealthVignette = Mathf.Lerp(maxHealthVignetteIntensity, 0f, healthRatio);
        }

        currentHealthVignetteIntensity = Mathf.Lerp(currentHealthVignetteIntensity, targetHealthVignette,
            transitionSpeed * Time.deltaTime);
    }

    void ApplyEffects()
    {
        // Apply Motion Blur
        if (motionBlur != null)
        {
            bool shouldBeActive = currentMotionBlurIntensity > 0.01f;
            motionBlur.active = shouldBeActive;
            if (shouldBeActive)
            {
                motionBlur.intensity.value = currentMotionBlurIntensity;
            }
        }

        // Apply Vignette (combine stamina and health effects)
        if (vignette != null)
        {
            float combinedVignetteIntensity = Mathf.Max(currentVignetteIntensity, currentHealthVignetteIntensity);
            bool shouldBeActive = combinedVignetteIntensity > 0.01f;
            vignette.active = shouldBeActive;

            if (shouldBeActive)
            {
                vignette.intensity.value = combinedVignetteIntensity;

                // Use health vignette color when in critical health, otherwise black
                if (currentHealthVignetteIntensity > currentVignetteIntensity)
                {
                    vignette.color.value = healthVignetteColor;
                }
                else
                {
                    vignette.color.value = Color.black;
                }
            }
        }

        // Apply Chromatic Aberration
        if (chromaticAberration != null)
        {
            bool shouldBeActive = currentChromaticAberrationIntensity > 0.01f;
            chromaticAberration.active = shouldBeActive;
            if (shouldBeActive)
            {
                chromaticAberration.intensity.value = currentChromaticAberrationIntensity;
            }
        }
    }

    // Health event handlers
    void OnDamageTaken(float damage, DamageType damageType, Vector3 damageDirection)
    {
        // Screen shake
        if (damageShakeIntensity > 0 && damageShakeDuration > 0)
        {
            float shakeIntensity = Mathf.Clamp(damage / 20f, 0.1f, 1f) * damageShakeIntensity;
            StartCoroutine(ScreenShakeCoroutine(shakeIntensity, damageShakeDuration));
        }

        // Damage flash
        if (enableDamageFlash && damageFlashImage != null)
        {
            StartCoroutine(DamageFlashCoroutine());
        }
    }

    void OnDeath()
    {
        // Stop any ongoing screen shake
        if (isShaking)
        {
            StopCoroutine(ScreenShakeCoroutine(0f, 0f));
            if (playerCamera != null)
            {
                playerCamera.transform.localPosition = originalCameraPosition;
            }
            isShaking = false;
        }
    }

    // Coroutines
    IEnumerator ScreenShakeCoroutine(float intensity, float duration)
    {
        if (isShaking || playerCamera == null) yield break;

        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float strength = intensity * (1f - (elapsed / duration));
            Vector3 offset = Random.insideUnitSphere * strength;
            offset.z = 0f; // Keep camera from moving forward/backward

            playerCamera.transform.localPosition = originalCameraPosition + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.localPosition = originalCameraPosition;
        isShaking = false;
    }

    IEnumerator DamageFlashCoroutine()
    {
        if (damageFlashImage == null) yield break;

        // Flash in
        float elapsed = 0f;
        float halfDuration = damageFlashDuration * 0.5f;

        // Fade in
        while (elapsed < halfDuration)
        {
            float alpha = Mathf.Lerp(0f, damageFlashColor.a, elapsed / halfDuration);
            damageFlashImage.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        // Fade out
        while (elapsed < halfDuration)
        {
            float alpha = Mathf.Lerp(damageFlashColor.a, 0f, elapsed / halfDuration);
            damageFlashImage.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure it's completely transparent
        damageFlashImage.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, 0f);
    }

    // Public methods for external control
    public void SetMotionBlurEnabled(bool enabled)
    {
        enableRunningMotionBlur = enabled;
        if (!enabled && motionBlur != null)
        {
            motionBlur.active = false;
            currentMotionBlurIntensity = 0f;
        }
    }

    public void SetVignetteEnabled(bool enabled)
    {
        enableStaminaVignette = enabled;
        if (!enabled && vignette != null && !enableHealthEffects)
        {
            vignette.active = false;
            currentVignetteIntensity = 0f;
        }
    }

    public void SetChromaticAberrationEnabled(bool enabled)
    {
        enableStaminaChromaticAberration = enabled;
        if (!enabled && chromaticAberration != null)
        {
            chromaticAberration.active = false;
            currentChromaticAberrationIntensity = 0f;
        }
    }

    public void SetHealthEffectsEnabled(bool enabled)
    {
        enableHealthEffects = enabled;
        if (!enabled)
        {
            currentHealthVignetteIntensity = 0f;
        }
    }

    public void SetDamageFlashEnabled(bool enabled)
    {
        enableDamageFlash = enabled;
        if (damageFlashObject != null)
        {
            damageFlashObject.SetActive(enabled);
        }
    }

    public void TriggerCustomScreenShake(float intensity, float duration)
    {
        StartCoroutine(ScreenShakeCoroutine(intensity, duration));
    }

    public void TriggerCustomDamageFlash(Color flashColor, float duration)
    {
        if (damageFlashImage != null)
        {
            Color originalColor = damageFlashColor;
            float originalDuration = damageFlashDuration;

            damageFlashColor = flashColor;
            damageFlashDuration = duration;

            StartCoroutine(DamageFlashCoroutine());

            // Restore original values after the flash
            damageFlashColor = originalColor;
            damageFlashDuration = originalDuration;
        }
    }

    // --- Screen Effects Control ---
    public void ToggleMotionBlur()
    {
        SetMotionBlurEnabled(!enableRunningMotionBlur);
    }

    public void ToggleVignette()
    {
        SetVignetteEnabled(!enableStaminaVignette);
    }

    public void ToggleChromaticAberration()
    {
        SetChromaticAberrationEnabled(!enableStaminaChromaticAberration);
    }
}