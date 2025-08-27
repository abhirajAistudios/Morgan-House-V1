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
    [SerializeField] private Image loadingImage; // 👈 Image component to display tips/art
    [SerializeField] private List<Sprite> loadingImages = new List<Sprite>(); // 👈 Assign multiple sprites here

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float minShowTimeAfterReady = 0.5f;
    [SerializeField] private float fakeLoadSpeed = 0.2f; //  Lower to make it "slower"

    public bool IsLoading { get; private set; } = false;

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

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
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
            Debug.LogWarning(" No AutoSaveManager found in scene!");
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

        if (loadingPanel != null) loadingPanel.SetActive(true);

        progressBar.value = 0f;
        progressText.text = "0%";

        // Show a random image from the list
        if (loadingImages.Count > 0 && loadingImage != null)
        {
            int randomIndex = Random.Range(0, loadingImages.Count);
            loadingImage.sprite = loadingImages[randomIndex];
        }

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

        if (loadingPanel != null) loadingPanel.SetActive(false);
        IsLoading = false;
    }
}