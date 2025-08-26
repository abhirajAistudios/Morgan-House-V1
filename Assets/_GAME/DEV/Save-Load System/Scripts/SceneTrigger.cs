using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles scene transition when player completes objectives and enters trigger.
/// Also manages auto-saving after scene load.
/// </summary>
public class SceneTrigger : MonoBehaviour
{
    // ---------------- PRIVATE VARIABLES ------------------
    private bool hasTriggered = false; // Prevents multiple triggers

    // ---------------- UNITY METHODS ----------------------

    private void Awake()
    {
        // Keep this trigger object persistent across scene loads
        DontDestroyOnLoad(gameObject);
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
            SceneLoader.Instance.LoadNextScene();

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
        GameObject spawnPoint = GameObject.FindWithTag("Spawn Point");

        if (saveManager != null && player != null)
        {
            player.transform.position = spawnPoint.transform.position;

            saveManager.SaveGame(player.transform);
            Debug.Log(" AutoSave triggered after scene load: " + SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.LogWarning(" AutoSave skipped - SaveManager or Player not found in scene: "
                             + SceneManager.GetActiveScene().name);
        }
    }
}
