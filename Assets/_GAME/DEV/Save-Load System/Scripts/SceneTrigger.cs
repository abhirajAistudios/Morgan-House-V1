using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles scene transition when player completes objectives and enters trigger.
/// Also manages auto-saving after scene load.
/// </summary>
public class SceneTrigger : MonoBehaviour
{
    // ---------------- INSPECTOR VARIABLES ----------------
    [SerializeField] private string nextSceneName; // The name of the next scene to load

    // ---------------- PRIVATE VARIABLES ------------------
    private bool hasTriggered = false; // Prevents multiple triggers

    // ---------------- UNITY METHODS ----------------------

    private void Awake()
    {
        // Keep this trigger object persistent across scene loads
        DontDestroyOnLoad(gameObject);

        // Subscribe to sceneLoaded event (needed to reset triggers in specific scenes)
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Currently unused but kept for possible future initialization
    }

    private void OnDestroy()
    {
        // Unsubscribe from sceneLoaded event to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prevent triggering more than once
        if (hasTriggered) return;

        // Only trigger if the player collides and all objectives are completed
        if (other.CompareTag("Player") && ObjectiveManager.Instance.AllObjectivesCompleted())
        {
            hasTriggered = true;

            // Load the next scene using LoadingManager
            LoadingManager.Instance.LoadSceneByName(nextSceneName);

            // Attempt to autosave after the new scene loads
            AutoSaveAfterSceneLoad();
        }
    }

    // ---------------- CUSTOM METHODS ----------------------

    /// <summary>
    /// Handles auto-saving player progress after a scene load.
    /// </summary>
    private void AutoSaveAfterSceneLoad()
    {
        AutoSaveManager saveManager = FindObjectOfType<AutoSaveManager>();
        GameObject player = GameObject.FindWithTag("Player");

        if (saveManager != null && player != null)
        {
            Transform playerpos = FindAnyObjectByType<PlayerController>().transform;

            saveManager.SaveAfterObjective(playerpos);
            Debug.Log(" AutoSave triggered after scene load: " + SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.LogWarning(" AutoSave skipped - SaveManager or Player not found in scene: "
                             + SceneManager.GetActiveScene().name);
        }
    }

    /// <summary>
    /// Resets trigger state when a specific scene is loaded.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == nextSceneName)
        {
            hasTriggered = false;
            Debug.Log(" SceneTrigger reset in " + scene.name);
        }
    }
}
