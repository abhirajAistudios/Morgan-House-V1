using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("UI refs (assign in Inspector)")]
    public GameObject loadingPanel;
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float fadeDuration = 0.3f;
    public float minShowTimeAfterReady = 0.5f;

    [Tooltip("Controls how fast the fake loading progresses (higher = faster).")]
    [SerializeField] private float fakeLoadSpeed = 0.5f; // <-- Adjustable speed in Inspector

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

    public void LoadSceneByName(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingPanel.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        progressBar.value = 0f;
        progressText.text = "0%";

        yield return null; // Ensure UI is rendered first

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float fakeProgress = 0f;
        while (fakeProgress < 0.9f)
        {
            fakeProgress += Time.unscaledDeltaTime * fakeLoadSpeed; // ✅ Uses adjustable speed
            progressBar.value = fakeProgress;
            progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
            yield return null;

            if (op.progress >= 0.9f && fakeProgress >= 0.9f)
                break;
        }

        // Fill to 100%
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

        loadingPanel.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / Mathf.Max(0.0001f, duration));
            yield return null;
        }
        cg.alpha = to;
    }
}
