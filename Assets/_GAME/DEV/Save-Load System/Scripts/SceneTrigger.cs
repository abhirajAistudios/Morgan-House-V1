using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player") && ObjectiveManager.Instance.AllObjectivesCompleted())
        {
            hasTriggered = true;

            // Subscribe to sceneLoaded event
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Load next scene
            SceneLoader.Instance.LoadNextScene();
        }
    }

    // This runs only AFTER the new scene has finished loading
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AutoSaveAfterSceneLoad();

        // Unsubscribe so it doesn’t run multiple times
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void AutoSaveAfterSceneLoad()
    {
        AutoSaveManager saveManager = FindObjectOfType<AutoSaveManager>();
        GameObject player = GameObject.FindWithTag("Player");

        if (saveManager != null && player != null)
        {
            player.transform.position = saveManager.spawnPoint.position;
            saveManager.SaveGame(player.transform);
        }
        else
        {
            Debug.LogWarning("⚠ AutoSave skipped - SaveManager or Player not found in scene: "
                             + SceneManager.GetActiveScene().name);
        }
    }
}