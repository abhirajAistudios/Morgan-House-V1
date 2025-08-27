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

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1.5f;        // Fade in/out duration
    [SerializeField] private float minShowTimeAfterReady = 0.5f; // Delay before hiding after load
    [SerializeField] private float LoadSpeed = 0.2f;       // Controls fake loading speed

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
            Debug.LogWarning("⚠️ No AutoSaveManager found in scene!");
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

        // Reset UI
        progressBar.value = 0f;
        progressText.text = "0%";

        // Show a random image (if available)
        if (loadingImages.Count > 0 && loadingImage != null)
        {
            int randomIndex = Random.Range(0, loadingImages.Count);
            loadingImage.sprite = loadingImages[randomIndex];
        }

        // Fade in loading screen
        yield return StartCoroutine(FadeLoadingScreen(1f, true));

        // Begin async load
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Fake progress until real loading hits 90%
        float fakeProgress = 0f;
        while (fakeProgress < 0.9f)
        {
            fakeProgress += Time.unscaledDeltaTime * LoadSpeed;
            progressBar.value = Mathf.Clamp01(fakeProgress);
            progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
            yield return null;

            if (op.progress >= 0.9f && fakeProgress >= 0.9f)
                break;
        }

        // Smoothly complete to 100%
        while (progressBar.value < 1f)
        {
            progressBar.value = Mathf.MoveTowards(progressBar.value, 1f, Time.unscaledDeltaTime * LoadSpeed);
            progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
            yield return null;
        }

        // Ensure loading screen shows for a minimum duration
        yield return new WaitForSecondsRealtime(minShowTimeAfterReady);

        // Allow scene activation
        op.allowSceneActivation = true;

        // Wait until fully loaded
        while (!op.isDone)
            yield return null;

        // Fade out loading screen
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
