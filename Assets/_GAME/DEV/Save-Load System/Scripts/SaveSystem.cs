using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    // Path where the save file will be stored in Documents/Horror_Engine/Save_File/
    private string saveFilePath;
    private string saveFolderPath;

    private void Awake()
    {
        // Get the Documents folder path
        string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

        // Create the custom save folder path
        saveFolderPath = Path.Combine(documentsPath, "Horror_Engine", "Save_File");

        // Create the directory if it doesn't exist
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
            Debug.Log("Created save directory: " + saveFolderPath);
        }

        // Generate the full save file path
        saveFilePath = Path.Combine(saveFolderPath, "savefile.json");

        Debug.Log("Save file will be stored at: " + saveFilePath);
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

    // Optional: Method to get the save folder path for other scripts
    public string GetSaveFolderPath()
    {
        return saveFolderPath;
    }

    // Optional: Method to get the save file path for other scripts
    public string GetSaveFilePath()
    {
        return saveFilePath;
    }
}