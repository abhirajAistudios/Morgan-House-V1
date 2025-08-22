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

    private void Update()
    {
        // Press 'P' to restart the game (for debugging/testing)
        if (Input.GetKeyDown(KeyCode.P))
        {
            RestartGame();
        }
    }

    /// <summary>
    /// Saves game data into a JSON file.
    /// </summary>
    public void SaveGameText(ref SaveData data)
    {
       
        // Optional: Show confirmation message on UI (uncomment if GameService is implemented)
        // GameService.Instance.UIService.ShowMessage("Game Autosaved!", 2f);

    }

    /// <summary>
    /// Deletes the save file (used when restarting or resetting progress).
    /// </summary>
    public void ResetSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }
    }

    /// <summary>
    /// Restarts the game by deleting save and reloading current scene.
    /// </summary>
    public void RestartGame()
    {
        ResetSave();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
