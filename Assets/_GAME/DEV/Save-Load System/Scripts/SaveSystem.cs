using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    // Path where the save file will be stored (persistentDataPath works across platforms)
    private string saveFilePath;

    private void Awake()
    {
        // Generate a consistent save file path at startup
        saveFilePath = Path.Combine(Application.persistentDataPath, "savefile.json");
    }

    private void Start()
    {
        // Find the AutoSaveManager in the scene
        var saveSystem = FindAnyObjectByType<AutoSaveManager>();

        // Find the PlayerController in the scene (to restore position/state)
        var player = FindAnyObjectByType<PlayerController>()?.transform;

        // If both are found, load the saved game data into the player
        if (saveSystem != null && player != null)
        {
            saveSystem.LoadGame(player);
        }
    }
}