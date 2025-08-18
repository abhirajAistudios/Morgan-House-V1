using UnityEngine;

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

            // 🔹 Destroy the trigger so it won’t fire again
            Destroy(gameObject);
        }
    }
}
