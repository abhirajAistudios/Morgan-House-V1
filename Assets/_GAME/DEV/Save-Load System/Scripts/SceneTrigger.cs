using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Morgan_HouseTestScene"; // Next scene name
    [SerializeField] private GameObject loadingScreenPrefab;  // Assign your loading screen prefab

    private bool hasTriggered = false;
    private GameObject loadingScreenInstance;

    private void Awake()
    {
        // Make this trigger persistent across all scenes
        DontDestroyOnLoad(gameObject);

        // Subscribe to sceneLoaded so we can reset in specific scenes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void Start()
    {
        
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;  // Prevent multiple calls

        if (other.CompareTag("Player") && ObjectiveManager.Instance.AllObjectivesCompleted())
        {
            hasTriggered = true;

            // Start loading the next scene
            LoadingManager.Instance.LoadSceneByName(nextSceneName);

            // ✅ Autosave after the scene has fully loaded
            AutoSaveAfterSceneLoad();
        }
    }

    private void AutoSaveAfterSceneLoad()
    {
        AutoSaveManager saveManager = FindObjectOfType<AutoSaveManager>();
        GameObject player = GameObject.FindWithTag("Player");

        if (saveManager != null && player != null)
        {
            Transform playerpos = FindAnyObjectByType<PlayerController>().transform;

            saveManager.SaveAfterObjective(playerpos);
            Debug.Log("✅ AutoSave triggered after scene load: " + SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.LogWarning("⚠ AutoSave skipped - SaveManager or Player not found in scene: " + SceneManager.GetActiveScene().name);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset the trigger flag when we enter Morgan_HouseTestScene
        if (scene.name == "Morgan_HouseTestScene")
        {
            hasTriggered = false;
            Debug.Log(" SceneTrigger reset in " + scene.name);
        }
    }
}
