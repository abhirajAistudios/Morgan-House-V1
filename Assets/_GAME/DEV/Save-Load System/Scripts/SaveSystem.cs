using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    private string saveFilePath;

    private void Awake()
    {
        // Single consistent file path
        saveFilePath = Path.Combine(Application.persistentDataPath, "savefile.json");
    }

    private void Start()
    {
        var saveSystem = FindAnyObjectByType<AutoSaveManager>();
        var player = FindAnyObjectByType<PlayerController>()?.transform;

        if (saveSystem != null && player != null)
        {
            saveSystem.LoadGame(player); 
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            RestartGame();
        }
    }

    /// <summary>
    /// Saves game data into a JSON file.
    /// </summary>
    public void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        // Optional UI popup
        GameService.Instance.UIService.ShowMessage("Game Autosaved!", 2f);
    }

    public void ResetSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }
    }

    public void RestartGame()
    {
        ResetSave();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
