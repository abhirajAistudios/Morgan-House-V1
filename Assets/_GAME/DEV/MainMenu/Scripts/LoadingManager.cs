using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles scene loading with fade-in/out UI, progress bar, and optional random loading image.
/// Supports ResumeRequested flag for restoring game state after loading.
/// </summary>
public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;
    public static bool ResumeRequested = false;

    [Header("UI References (Assign in Inspector)")]
    [SerializeField] private GameObject loadingPanel;       // Panel for the loading screen
    [SerializeField] private Slider progressBar;            // Progress bar
    [SerializeField] private TextMeshProUGUI progressText;  // Text to show percentage
    [SerializeField] private Image loadingImage;            // Optional loading artwork/tip
    [SerializeField] private List<Sprite> loadingImages = new List<Sprite>(); // Pool of images

    [Header("Hint System")]
    [SerializeField] private TextMeshProUGUI hintText;      // Text component for hints
    [SerializeField] private List<string> hints = new List<string>(); // List of hint texts

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1.5f;        // Fade in/out duration
    [SerializeField] private float minShowTimeAfterReady = 0.5f; // Delay before hiding after load
    
    [Header("Sound")]
    [SerializeField] private Sounds loadingScreenBGM;
   

    public bool IsLoading { get; private set; } = false;

    private CanvasGroup loadingCanvasGroup; // For smooth fading

    #region Unity Lifecycle
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup CanvasGroup for fading
        if (loadingPanel != null)
        {
            loadingCanvasGroup = loadingPanel.GetComponent<CanvasGroup>();
            if (loadingCanvasGroup == null)
                loadingCanvasGroup = loadingPanel.AddComponent<CanvasGroup>();

            loadingPanel.SetActive(false);
            loadingCanvasGroup.alpha = 0f;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion

    #region Scene Restore
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!ResumeRequested) return;

        ResumeRequested = false;
        StartCoroutine(WaitAndRestore());
    }

    /// <summary>
    /// Waits for the Player to spawn, then restores saved state if AutoSaveManager exists.
    /// </summary>
    private IEnumerator WaitAndRestore()
    {
        GameObject playerObj = null;

        // Wait until player exists in the new scene
        while (playerObj == null)
        {
            playerObj = GameObject.FindWithTag("Player");
            yield return null;
        }

        var autosaveManager = FindObjectOfType<AutoSaveManager>();
        if (autosaveManager != null)
        {
            autosaveManager.LoadGame(playerObj.transform);
        }
        else
        {
            Debug.LogWarning("⚠ No AutoSaveManager found in scene!");
        }
    }
    #endregion

    #region Public API
    /// <summary>
    /// Call this to load a scene by name with loading screen.
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        if (!IsLoading)
            StartCoroutine(LoadSceneAsync(sceneName));
    }
    #endregion

    #region Loading Logic
    /// <summary>
    /// Handles asynchronous scene loading with fake progress and fade transitions.
    /// </summary>
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        IsLoading = true;

        SoundService.Instance.PlayMusic(loadingScreenBGM);
        // Reset UI
        progressBar.value = 0f;
        progressText.text = "0%";

        // Random loading image
        if (loadingImages.Count > 0 && loadingImage != null)
            loadingImage.sprite = loadingImages[Random.Range(0, loadingImages.Count)];

        // Random hint
        if (hints.Count > 0 && hintText != null)
            hintText.text = hints[Random.Range(0, hints.Count)];

        // Fade in
        yield return StartCoroutine(FadeLoadingScreen(1f, true));

        // Start async load
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Smooth progress update
        while (!op.isDone)
        {
            // Target progress (0 → 1 based on 0 → 0.9 real progress)
            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);

            // Smoothly move progress bar towards target
            progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.unscaledDeltaTime);
            progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";

            // Scene is loaded, just waiting for activation
            if (progressBar.value >= 1f && op.progress >= 0.9f)
            {
                yield return new WaitForSecondsRealtime(minShowTimeAfterReady);
                op.allowSceneActivation = true;
            }

            yield return null;
        }

        // Fade out
        yield return StartCoroutine(FadeLoadingScreen(0f, false));

        IsLoading = false;
    }
    #endregion

    #region Fade Effect
    /// <summary>
    /// Fades the loading screen in/out using CanvasGroup alpha.
    /// </summary>
    private IEnumerator FadeLoadingScreen(float targetAlpha, bool isFadeIn)
    {
        if (loadingPanel != null && loadingCanvasGroup != null)
        {
            if (isFadeIn && !loadingPanel.activeSelf)
                loadingPanel.SetActive(true);

            float startAlpha = loadingCanvasGroup.alpha;
            float elapsedTime = 0f;

            // Smooth fade
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / fadeDuration);
                loadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            // Final set
            loadingCanvasGroup.alpha = targetAlpha;

            // After fade-out, disable panel
            if (!isFadeIn && loadingPanel.activeSelf)
            {
                yield return new WaitForSecondsRealtime(0.1f);
                loadingPanel.SetActive(false);
            }
        }
        else if (loadingPanel != null)
        {
            // Fallback if CanvasGroup is missing
            loadingPanel.SetActive(isFadeIn);
        }
    }
    #endregion
}