using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealthController : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points")]
    public float maxHealth = 100f;

    [Tooltip("Current health points")]
    public float currentHealth = 100f;

    [Tooltip("Health regeneration rate per second")]
    public float healthRegenRate = 5f;

    [Tooltip("Delay before health starts regenerating")]
    public float regenDelay = 3f;

    [Tooltip("Minimum health threshold for critical state")]
    public float criticalHealthThreshold = 25f;

    [Tooltip("Health threshold below which regen stops")]
    public float minRegenHealth = 10f;

    [Tooltip("Enable automatic health regeneration")]
    public bool enableHealthRegen = true;

    [Header("Damage Settings")]
    [Tooltip("Invincibility duration after taking damage")]
    public float invincibilityDuration = 0.5f;

    [Tooltip("Damage reduction multiplier when crouching")]
    [Range(0f, 1f)]
    public float crouchDamageReduction = 0.8f;

    [Tooltip("Fall damage threshold (units per second)")]
    public float fallDamageThreshold = 15f;

    [Tooltip("Fall damage multiplier")]
    public float fallDamageMultiplier = 10f;

    [Header("Death Settings")]
    [Tooltip("Enable respawn system")]
    public bool enableRespawn = true;

    [Tooltip("Respawn delay in seconds")]
    public float respawnDelay = 3f;

    [Tooltip("Respawn position (leave null for current position)")]
    public Transform respawnPoint;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent<float, DamageType, Vector3> OnDamageTaken;
    public UnityEvent OnCriticalHealth;
    public UnityEvent OnLeaveCriticalHealth;
    public UnityEvent OnDeath;
    public UnityEvent OnRespawn;
    public UnityEvent<float> OnHealed;

    // Components
    private PlayerController playerController;
    private PlayerAudioController audioController;
    private PlayerUIController uiController;
    private PlayerScreenEffects screenEffects;

    // Health state
    private bool isDead = false;
    private bool isInvincible = false;
    private bool isCritical = false;
    private float lastDamageTime;
    private float lastFallVelocity;

    [Header("Debug Testing")]
    [Tooltip("Activate & Show debug GUI for health testing")]
    public bool DebugTest = false;

    // Add these private variables after your existing private variables
    private bool showDebugGUI = false;
    private bool guiVisible = false;
    private GUIStyle guiStyle;
    private GUIStyle headerStyle;
    private GUIStyle keyStyle;

    void Start()
    {
        // Get components
        playerController = GetComponent<PlayerController>();
        audioController = GetComponent<PlayerAudioController>();
        uiController = FindObjectOfType<PlayerUIController>();
        screenEffects = GetComponent<PlayerScreenEffects>();

        // Initialize health
        currentHealth = maxHealth;

        // Fire initial health changed event
        OnHealthChanged?.Invoke(currentHealth);
    }

    void Update()
    {
        HandleHealthRegeneration();
        HandleCriticalState();
        HandleFallDamage();

        if (DebugTest == true)
        {
            showDebugGUI = true;
            guiVisible = true;

            HealthTestInputs();

        }
    }

    void HandleHealthRegeneration()
    {
        if (!enableHealthRegen || isDead || currentHealth >= maxHealth || currentHealth <= minRegenHealth)
            return;

        // Check if enough time has passed since last damage
        if (Time.time - lastDamageTime >= regenDelay)
        {
            float regenAmount = healthRegenRate * Time.deltaTime;
            Heal(regenAmount, false); // Silent healing for regen
        }
    }

    void HandleCriticalState()
    {
        bool shouldBeCritical = currentHealth <= criticalHealthThreshold && !isDead;

        if (shouldBeCritical && !isCritical)
        {
            // Entering critical state
            isCritical = true;
            OnCriticalHealth?.Invoke();
        }
        else if (!shouldBeCritical && isCritical)
        {
            // Leaving critical state
            isCritical = false;
            OnLeaveCriticalHealth?.Invoke();
        }
    }

    void HandleFallDamage()
    {
        if (playerController == null || isDead) return;

        if (playerController.IsGrounded)
        {
            // Check if we just landed and calculate fall damage
            if (lastFallVelocity < -fallDamageThreshold)
            {
                float fallSpeed = Mathf.Abs(lastFallVelocity);
                float damage = (fallSpeed - fallDamageThreshold) * fallDamageMultiplier;

                if (damage > 0)
                {
                    TakeDamage(damage, DamageType.Fall);
                }
            }
            lastFallVelocity = 0f;
        }
        else
        {
            // Track falling velocity
            lastFallVelocity = playerController.Velocity.y;
        }
    }

    public void TakeDamage(float amount, DamageType damageType = DamageType.Normal, Vector3 damageDirection = default)
    {
        if (isDead || isInvincible || amount <= 0) return;

        // Apply damage reduction if crouching
        if (playerController != null && playerController.IsCrouching && damageType != DamageType.Fall)
        {
            amount *= crouchDamageReduction;
        }

        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        lastDamageTime = Time.time;

        // Fire events
        OnHealthChanged?.Invoke(currentHealth);
        OnDamageTaken?.Invoke(amount, damageType, damageDirection);

        // Start invincibility
        if (invincibilityDuration > 0)
        {
            StartCoroutine(InvincibilityCoroutine());
        }

        // Check for death
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }

        Debug.Log($"Took {amount:F1} {damageType} damage. Health: {currentHealth:F1}/{maxHealth:F1}");
    }

    public void Heal(float amount, bool playEffects = true)
    {
        if (isDead || amount <= 0) return;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        // Fire events
        OnHealthChanged?.Invoke(currentHealth);
        OnHealed?.Invoke(amount);

        Debug.Log($"Healed {amount:F1} health. Health: {currentHealth:F1}/{maxHealth:F1}");
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        OnDeath?.Invoke();

        // Disable player controls
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        Debug.Log("Player died!");

        // Handle respawn
        if (enableRespawn)
        {
            StartCoroutine(RespawnCoroutine());
        }
    }

    void Respawn()
    {
        isDead = false;
        isCritical = false;
        currentHealth = maxHealth;

        // Fire events
        OnHealthChanged?.Invoke(currentHealth);
        OnRespawn?.Invoke();

        // Re-enable player controls
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Move to respawn point
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }

        Debug.Log("Player respawned!");
    }

    // Coroutines
    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    // Public methods for external access
    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        float healthRatio = currentHealth / maxHealth;
        maxHealth = newMaxHealth;
        currentHealth = newMaxHealth * healthRatio;

        OnHealthChanged?.Invoke(currentHealth);
    }

    public void RestoreToFullHealth()
    {
        Heal(maxHealth - currentHealth);
    }

    public void KillPlayer()
    {
        TakeDamage(currentHealth + 1, DamageType.Instant);
    }

    public void SetInvincible(bool invincible, float duration = 0f)
    {
        if (invincible)
        {
            isInvincible = true;
            if (duration > 0)
            {
                StartCoroutine(TimedInvincibilityCoroutine(duration));
            }
        }
        else
        {
            isInvincible = false;
        }
    }

    IEnumerator TimedInvincibilityCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    // Public properties
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsDead => isDead;
    public bool IsInvincible => isInvincible;
    public bool IsCritical => isCritical;
    public bool CanRegenerate => enableHealthRegen && !isDead && currentHealth < maxHealth &&
                                 currentHealth > minRegenHealth && Time.time - lastDamageTime >= regenDelay;


    private void HealthTestInputs()
    {

        // Test inputs (remove in final build)
        if (Input.GetKeyDown(KeyCode.T))
            TakeDamage(10f); // Take 10 damage

        if (Input.GetKeyDown(KeyCode.H))
            Heal(15f); // Heal 15 health

        if (Input.GetKeyDown(KeyCode.K))
            KillPlayer(); // Instant kill

        if (Input.GetKeyDown(KeyCode.R))
            RestoreToFullHealth(); // Full heal

        if (Input.GetKeyDown(KeyCode.I))
            SetInvincible(true, 3f); // 3 seconds invincibility

        if (Input.GetKeyDown(KeyCode.F))
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.Move(Vector3.up * 20f);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetHealth(50f); // Test regen from 50%

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetHealth(5f); // Test no regen below threshold
    }
    void OnGUI()
    {
        if (!showDebugGUI || !guiVisible) return;

        // Initialize styles if needed
        if (guiStyle == null)
        {
            guiStyle = new GUIStyle();
            guiStyle.fontSize = 12;
            guiStyle.normal.textColor = Color.white;
            guiStyle.margin = new RectOffset(5, 5, 2, 2);

            headerStyle = new GUIStyle();
            headerStyle.fontSize = 14;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.yellow;
            headerStyle.margin = new RectOffset(5, 5, 5, 5);

            keyStyle = new GUIStyle();
            keyStyle.fontSize = 12;
            keyStyle.fontStyle = FontStyle.Bold;
            keyStyle.normal.textColor = Color.cyan;
            keyStyle.margin = new RectOffset(5, 5, 2, 2);
        }

        // Create GUI box
        Rect boxRect = new Rect(10, 10, 300, 320);
        GUI.Box(boxRect, "");

        float y = 20;
        GUI.Label(new Rect(20, y, 260, 20), "HEALTH TEST CONTROLS", headerStyle);
        y += 30;

        // Current health status
        string status = $"Health: {currentHealth:F1}/{maxHealth:F1}";
        if (isCritical) status += " [CRITICAL]";
        if (isDead) status += " [DEAD]";
        if (isInvincible) status += " [INVINCIBLE]";
        GUI.Label(new Rect(20, y, 260, 20), status, guiStyle);
        y += 25;

        // Controls
        GUI.Label(new Rect(20, y, 260, 20), "CONTROLS:", headerStyle);
        y += 25;

        GUI.Label(new Rect(20, y, 20, 20), "T", keyStyle);
        GUI.Label(new Rect(45, y, 200, 20), "Take 10 damage", guiStyle);
        y += 20;

        GUI.Label(new Rect(20, y, 20, 20), "H", keyStyle);
        GUI.Label(new Rect(45, y, 200, 20), "Heal 15 health", guiStyle);
        y += 20;

        GUI.Label(new Rect(20, y, 20, 20), "K", keyStyle);
        GUI.Label(new Rect(45, y, 200, 20), "Kill player", guiStyle);
        y += 20;

        GUI.Label(new Rect(20, y, 20, 20), "R", keyStyle);
        GUI.Label(new Rect(45, y, 200, 20), "Restore full health", guiStyle);
        y += 20;

        GUI.Label(new Rect(20, y, 20, 20), "I", keyStyle);
        GUI.Label(new Rect(45, y, 200, 20), "3sec invincibility", guiStyle);
        y += 20;

        GUI.Label(new Rect(20, y, 20, 20), "F", keyStyle);
        GUI.Label(new Rect(45, y, 200, 20), "Teleport up (fall damage)", guiStyle);
        y += 20;

        GUI.Label(new Rect(20, y, 20, 20), "1", keyStyle);
        GUI.Label(new Rect(45, y, 200, 20), "Set health to 50%", guiStyle);
        y += 20;

        GUI.Label(new Rect(20, y, 20, 20), "2", keyStyle);
        GUI.Label(new Rect(45, y, 200, 20), "Set health to 5%", guiStyle);
        y += 20;

        GUI.Label(new Rect(20, y, 30, 20), "Toggle GUI of via Debug Bool in Health Script", keyStyle);
    }
}

public enum DamageType
{
    Normal,
    Fall,
    Fire,
    Cold,
    Poison,
    Instant
}