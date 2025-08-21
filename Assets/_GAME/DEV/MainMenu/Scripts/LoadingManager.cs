using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("UI refs (assign in Inspector)")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float minShowTimeAfterReady = 0.5f;
    [SerializeField] private float fakeLoadSpeed = 0.5f;

    public bool IsLoading { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    /// <summary>
    /// Call this to load ANY scene by name (works for multiple levels).
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        if (!IsLoading) // ✅ Prevent multiple loads
            StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        IsLoading = true;

        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        progressBar.value = 0f;
        progressText.text = "0%";

        yield return null; // let UI update at least one frame

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

        if (loadingPanel != null) loadingPanel.SetActive(false);
        IsLoading = false;
    }
}
