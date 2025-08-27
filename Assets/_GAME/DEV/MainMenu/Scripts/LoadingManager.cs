using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;
    public static bool ResumeRequested = false;

    [Header("UI refs (assign in Inspector)")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image loadingImage; //  Image component to display tips/art
    [SerializeField] private List<Sprite> loadingImages = new List<Sprite>(); //  Assign multiple sprites here

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1.5f;   //  longer so fade-out is visible
    [SerializeField] private float minShowTimeAfterReady = 0.5f;
    [SerializeField] private float fakeLoadSpeed = 0.2f; // Lower to make it "slower"

    public bool IsLoading { get; private set; } = false;

    private CanvasGroup loadingCanvasGroup;

    private void Awake()
    {
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

        // Ensure we have a CanvasGroup for fading
        if (loadingPanel != null)
        {
            loadingCanvasGroup = loadingPanel.GetComponent<CanvasGroup>();
            if (loadingCanvasGroup == null)
            {
                loadingCanvasGroup = loadingPanel.AddComponent<CanvasGroup>();
            }
            loadingPanel.SetActive(false);
            loadingCanvasGroup.alpha = 0f;
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!ResumeRequested) return;

        ResumeRequested = false;
        StartCoroutine(WaitAndRestore());
    }

    private IEnumerator WaitAndRestore()
    {
        // wait until Player is spawned in the new scene
        GameObject playerObj = null;
        while (playerObj == null)
        {
            playerObj = GameObject.FindWithTag("Player");
            yield return null; // wait a frame
        }

        var player = playerObj.transform;
        var autosavemanager = FindObjectOfType<AutoSaveManager>();
        if (autosavemanager != null)
        {
            autosavemanager.LoadGame(player);
        }
        else
        {
            Debug.LogWarning("⚠️ No AutoSaveManager found in scene!");
        }
    }

    public void LoadSceneByName(string sceneName)
    {
        if (!IsLoading)
            StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        IsLoading = true;

        // Reset UI elements before showing
        progressBar.value = 0f;
        progressText.text = "0%";

        // Show a random image from the list
        if (loadingImages.Count > 0 && loadingImage != null)
        {
            int randomIndex = Random.Range(0, loadingImages.Count);
            loadingImage.sprite = loadingImages[randomIndex];
        }

        // Fade in loading screen at the beginning
        yield return StartCoroutine(FadeLoadingScreen(1f, true));

        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float fakeProgress = 0f;
        while (fakeProgress < 0.9f)
        {
            fakeProgress += Time.unscaledDeltaTime * fakeLoadSpeed;
            progressBar.value = Mathf.Clamp01(fakeProgress);
            progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
            yield return null;

            if (op.progress >= 0.9f && fakeProgress >= 0.9f)
                break;
        }

        while (progressBar.value < 1f)
        {
            progressBar.value = Mathf.MoveTowards(progressBar.value, 1f, Time.unscaledDeltaTime * fakeLoadSpeed);
            progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
            yield return null;
        }

        yield return new WaitForSecondsRealtime(minShowTimeAfterReady);

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        // --- FADE OUT HAPPENS HERE ---
        yield return StartCoroutine(FadeLoadingScreen(0f, false));

        IsLoading = false;
    }

    /// <summary>
    /// Fades the loading screen to a target alpha.
    /// </summary>
    private IEnumerator FadeLoadingScreen(float targetAlpha, bool isFadeIn)
    {
        if (loadingPanel != null && loadingCanvasGroup != null)
        {
            if (isFadeIn && !loadingPanel.activeSelf)
            {
                loadingPanel.SetActive(true);
            }

            float startAlpha = loadingCanvasGroup.alpha; // 👈 Use current alpha
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / fadeDuration);
                loadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            loadingCanvasGroup.alpha = targetAlpha;

            if (!isFadeIn && loadingPanel.activeSelf)
            {
                // small delay so fade-out is noticeable
                yield return new WaitForSecondsRealtime(0.1f);
                loadingPanel.SetActive(false);
            }
        }
        else
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(isFadeIn);
            }
        }
    }
}
