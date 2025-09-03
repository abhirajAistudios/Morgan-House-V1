using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudioController : MonoBehaviour
{
    [Header("Footstep Settings")]
    [Tooltip("Default footstep sounds to play when walking")]
    public AudioClip[] footstepSounds;

    [Tooltip("Footstep sounds for grass surfaces")]
    public AudioClip[] grassFootsteps;

    [Tooltip("Footstep sounds for stone surfaces")]
    public AudioClip[] stoneFootsteps;

    [Tooltip("Footstep sounds for wood surfaces")]
    public AudioClip[] woodFootsteps;

    [Tooltip("Time interval between footstep sounds")]
    public float footstepInterval = 0.5f;

    [Header("Dynamic Audio Settings")]
    [Tooltip("Breathing sounds to play when tired or running")]
    public AudioClip[] breathingSounds;

    [Tooltip("Cloth rustling sounds to play when moving")]
    public AudioClip[] clothRustlingSounds;

    [Tooltip("Heartbeat sound to play when low on stamina")]
    public AudioClip heartbeatSound;

    [Tooltip("Maximum volume for breathing sounds")]
    public float breathingVolumeMax = 0.3f;

    [Tooltip("Volume for cloth rustling sounds")]
    public float clothRustlingVolume = 0.2f;

    [Tooltip("Maximum volume for heartbeat sound")]
    public float heartbeatVolumeMax = 0.4f;

    [Header("Health Audio Settings")]
    [Tooltip("Damage sounds to play when taking damage")]
    public AudioClip[] damageSounds;

    [Tooltip("Critical health heartbeat sound")]
    public AudioClip criticalHeartbeatSound;

    [Tooltip("Death sound effect")]
    public AudioClip deathSound;

    [Tooltip("Healing sound effect")]
    public AudioClip healingSound;

    [Tooltip("Volume for damage sounds")]
    [Range(0f, 1f)]
    public float damageAudioVolume = 0.7f;

    [Tooltip("Volume for critical heartbeat")]
    [Range(0f, 1f)]
    public float criticalHeartbeatVolume = 0.5f;

    // Components
    private AudioSource audioSource;
    private AudioSource breathingAudioSource;
    private AudioSource heartbeatAudioSource;
    private AudioSource criticalAudioSource;
    private PlayerController controller;
    private PlayerHealthController healthController;

    // Audio timers
    private float footstepTimer;
    private float clothRustlingTimer;
    private float breathingTimer;
    private float heartbeatTimer;
    private bool isPlayingHeartbeat;
    private bool isPlayingCriticalHeartbeat;
    private string currentSurface = "default";

    void Start()
    {
        // Get components
        audioSource = GetComponent<AudioSource>();
        controller = GetComponent<PlayerController>();
        healthController = GetComponent<PlayerHealthController>();

        // Set up additional audio sources
        SetupAudioSources();

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
        HandleFootsteps();
        HandleDynamicAudio();
        HandleCriticalHeartbeat();
    }

    void SetupAudioSources()
    {
        // Create breathing audio source
        GameObject breathingGO = new GameObject("BreathingAudio");
        breathingGO.transform.parent = transform;
        breathingAudioSource = breathingGO.AddComponent<AudioSource>();
        breathingAudioSource.loop = false;
        breathingAudioSource.volume = 0f;

        // Create heartbeat audio source
        GameObject heartbeatGO = new GameObject("HeartbeatAudio");
        heartbeatGO.transform.parent = transform;
        heartbeatAudioSource = heartbeatGO.AddComponent<AudioSource>();
        heartbeatAudioSource.loop = true;
        heartbeatAudioSource.volume = 0f;
        if (heartbeatSound != null)
        {
            heartbeatAudioSource.clip = heartbeatSound;
        }

        // Create critical health heartbeat audio source
        GameObject criticalGO = new GameObject("CriticalHeartbeat");
        criticalGO.transform.parent = transform;
        criticalAudioSource = criticalGO.AddComponent<AudioSource>();
        criticalAudioSource.loop = true;
        criticalAudioSource.volume = 0f;
        criticalAudioSource.spatialBlend = 0f; // 2D sound

        if (criticalHeartbeatSound != null)
        {
            criticalAudioSource.clip = criticalHeartbeatSound;
        }
    }

    void SubscribeToHealthEvents()
    {
        if (healthController == null) return;

        healthController.OnDamageTaken.AddListener(OnDamageTaken);
        healthController.OnCriticalHealth.AddListener(OnCriticalHealth);
        healthController.OnLeaveCriticalHealth.AddListener(OnLeaveCriticalHealth);
        healthController.OnDeath.AddListener(OnDeath);
        healthController.OnHealed.AddListener(OnHealed);
    }

    void UnsubscribeFromHealthEvents()
    {
        if (healthController == null) return;

        healthController.OnDamageTaken.RemoveListener(OnDamageTaken);
        healthController.OnCriticalHealth.RemoveListener(OnCriticalHealth);
        healthController.OnLeaveCriticalHealth.RemoveListener(OnLeaveCriticalHealth);
        healthController.OnDeath.RemoveListener(OnDeath);
        healthController.OnHealed.RemoveListener(OnHealed);
    }

    void HandleFootsteps()
    {
        if (controller.IsGrounded && controller.IsMoving && !controller.IsCrouching)
        {
            footstepTimer += Time.deltaTime;

            float interval = footstepInterval;
            if (controller.IsRunning)
                interval *= 0.7f; // Faster footsteps when running

            if (footstepTimer >= interval)
            {
                PlayFootstepSound();
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    void PlayFootstepSound()
    {
        AudioClip[] soundsToUse = footstepSounds;

        // Detect surface type
        DetectSurface();

        switch (currentSurface)
        {
            case "grass":
                soundsToUse = grassFootsteps;
                break;
            case "stone":
                soundsToUse = stoneFootsteps;
                break;
            case "wood":
                soundsToUse = woodFootsteps;
                break;
            default:
                soundsToUse = footstepSounds;
                break;
        }

        if (soundsToUse != null && soundsToUse.Length > 0)
        {
            AudioClip clipToPlay = soundsToUse[Random.Range(0, soundsToUse.Length)];
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    void DetectSurface()
    {
        RaycastHit hit;
        CharacterController charController = controller.GetCharacterController();
        if (Physics.Raycast(transform.position, Vector3.down, out hit, charController.height * 0.5f + 0.3f))
        {
            if (hit.collider.CompareTag("Grass"))
                currentSurface = "grass";
            else if (hit.collider.CompareTag("Stone"))
                currentSurface = "stone";
            else if (hit.collider.CompareTag("Wood"))
                currentSurface = "wood";
            else
                currentSurface = "default";
        }
    }

    void HandleDynamicAudio()
    {
        HandleBreathingAudio();
        HandleHeartbeatAudio();
        HandleClothRustling();
    }

    void HandleBreathingAudio()
    {
        if (breathingSounds == null || breathingSounds.Length == 0) return;

        float staminaPercentage = controller.StaminaPercentage;
        bool shouldBreatheHeavy = staminaPercentage < 0.3f || controller.IsRunning;

        if (shouldBreatheHeavy)
        {
            breathingTimer += Time.deltaTime;
            float breathingInterval = controller.IsRunning ? 1f : 2f;

            if (breathingTimer >= breathingInterval)
            {
                AudioClip breathingClip = breathingSounds[Random.Range(0, breathingSounds.Length)];
                float volume = Mathf.Lerp(0.1f, breathingVolumeMax, 1f - staminaPercentage);
                breathingAudioSource.PlayOneShot(breathingClip, volume);
                breathingTimer = 0f;
            }
        }
        else
        {
            breathingTimer = 0f;
        }
    }

    void HandleHeartbeatAudio()
    {
        if (heartbeatSound == null) return;

        float staminaPercentage = controller.StaminaPercentage;
        bool shouldPlayHeartbeat = staminaPercentage < 0.2f || controller.IsRunning;

        if (shouldPlayHeartbeat && !isPlayingHeartbeat)
        {
            heartbeatAudioSource.Play();
            isPlayingHeartbeat = true;
        }
        else if (!shouldPlayHeartbeat && isPlayingHeartbeat)
        {
            heartbeatAudioSource.Stop();
            isPlayingHeartbeat = false;
        }

        if (isPlayingHeartbeat)
        {
            float volume = Mathf.Lerp(0f, heartbeatVolumeMax, 1f - staminaPercentage);
            heartbeatAudioSource.volume = volume;

            // Speed up heartbeat when running or low stamina
            float pitch = controller.IsRunning ? 1.2f : Mathf.Lerp(0.8f, 1.1f, 1f - staminaPercentage);
            heartbeatAudioSource.pitch = pitch;
        }
    }

    void HandleCriticalHeartbeat()
    {
        if (healthController == null || criticalHeartbeatSound == null) return;

        // Update critical heartbeat volume and pitch based on health
        if (isPlayingCriticalHeartbeat && healthController.IsCritical)
        {
            float healthRatio = healthController.CurrentHealth / healthController.criticalHealthThreshold;
            criticalAudioSource.volume = Mathf.Lerp(criticalHeartbeatVolume, 0f, healthRatio);
            criticalAudioSource.pitch = Mathf.Lerp(1.5f, 1f, healthRatio);
        }
    }

    void HandleClothRustling()
    {
        if (clothRustlingSounds == null || clothRustlingSounds.Length == 0) return;

        bool isMoving = controller.IsMoving && controller.IsGrounded;
        if (isMoving)
        {
            clothRustlingTimer += Time.deltaTime;
            float rustlingInterval = controller.IsRunning ? 0.3f : 0.6f;

            if (clothRustlingTimer >= rustlingInterval)
            {
                AudioClip rustlingClip = clothRustlingSounds[Random.Range(0, clothRustlingSounds.Length)];
                float volume = controller.IsRunning ? clothRustlingVolume : clothRustlingVolume * 0.5f;
                audioSource.PlayOneShot(rustlingClip, volume);
                clothRustlingTimer = 0f;
            }
        }
        else
        {
            clothRustlingTimer = 0f;
        }
    }

    // Health event handlers
    void OnDamageTaken(float damage, DamageType damageType, Vector3 damageDirection)
    {
        PlayDamageAudio(damageType);
    }

    void OnCriticalHealth()
    {
        if (!isPlayingCriticalHeartbeat && criticalHeartbeatSound != null)
        {
            criticalAudioSource.Play();
            isPlayingCriticalHeartbeat = true;
        }
    }

    void OnLeaveCriticalHealth()
    {
        if (isPlayingCriticalHeartbeat)
        {
            criticalAudioSource.Stop();
            isPlayingCriticalHeartbeat = false;
        }
    }

    void OnDeath()
    {
        // Play death sound
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Stop critical heartbeat
        if (isPlayingCriticalHeartbeat)
        {
            criticalAudioSource.Stop();
            isPlayingCriticalHeartbeat = false;
        }
    }

    void OnHealed(float amount)
    {
        if (healingSound != null)
        {
            audioSource.PlayOneShot(healingSound, 0.5f);
        }
    }

    void PlayDamageAudio(DamageType damageType)
    {
        if (damageSounds != null && damageSounds.Length > 0)
        {
            AudioClip clipToPlay = damageSounds[Random.Range(0, damageSounds.Length)];
            audioSource.PlayOneShot(clipToPlay, damageAudioVolume);
        }
    }

    // Public methods for external access
    public void PlayCustomSound(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    public void SetFootstepInterval(float interval)
    {
        footstepInterval = interval;
    }

    public void SetBreathingVolume(float volume)
    {
        breathingVolumeMax = Mathf.Clamp01(volume);
    }

    public void SetHeartbeatVolume(float volume)
    {
        heartbeatVolumeMax = Mathf.Clamp01(volume);
    }

    public void SetDamageAudioVolume(float volume)
    {
        damageAudioVolume = Mathf.Clamp01(volume);
    }

    public void SetCriticalHeartbeatVolume(float volume)
    {
        criticalHeartbeatVolume = Mathf.Clamp01(volume);
    }
}