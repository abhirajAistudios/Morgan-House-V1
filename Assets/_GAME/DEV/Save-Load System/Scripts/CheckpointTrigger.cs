using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Header("Assign Player Tag in Inspector")]
    public string playerTag = "Player";

    private bool hasTriggered = false;  // prevent multiple saves on the same checkpoint

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag(playerTag))
        {
            hasTriggered = true; // avoid duplicate triggers

            // Find the save system in the scene
            AutoSaveManager saveSystem = FindAnyObjectByType<AutoSaveManager>();

            if (saveSystem != null)
            {
                saveSystem.SaveAfterObjective(other.transform);
                Debug.Log("Checkpoint reached and game autosaved!");
            }
            else
            {
                Debug.LogWarning(" No AutoSaveManager found in the scene!");
            }
        }
    }
}
