using System.IO;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    private string saveFilePath;

    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "savefile.json");
        Debug.Log("Save file path: " + saveFilePath);
    }

    private void Start()
    {
        var saveSystem = FindAnyObjectByType<AutoSaveManager>();
        var player = FindAnyObjectByType<PlayerController>().transform;

        if (saveSystem != null)
            saveSystem.LoadGame(player);

    }


    private void Update()
    {
       if(Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }
    /// <summary>
    /// Saves game data into a JSON file.
    /// </summary>
    public void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true); // true = pretty print
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game Saved!");
        GameService.Instance.UIService.ShowMessage("Game Autosaved!", 2f); // UI popup for autosave
    }

    public void ResetSave()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("? Save data reset. Starting fresh next time!");
        }
        else
        {
            Debug.Log("?? No save file found to delete.");
        }
    }

    public void RestartGame()
    {
        ResetSave();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
