using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Morgan_HouseTestScene"; // Next scene name
    [SerializeField] private GameObject loadingScreenPrefab;  // Assign your loading screen prefab

    private bool hasTriggered = false;
    private GameObject loadingScreenInstance;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;  // Prevent multiple calls

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

           
            // Start loading the next scene
            LoadingManager.Instance.LoadSceneByName(nextSceneName);

            // ✅ Autosave after the scene has fully loaded
            //AutoSaveAfterSceneLoad();
            
            // 🔹 Destroy the trigger so it won’t fire again
            Destroy(gameObject);
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
}
