using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Handles auto-saving and loading of game state (player, inventory, puzzles, doors, flashlight, etc.)
/// Uses JSON persistence at Application.persistentDataPath.
/// </summary>
public class AutoSaveManager : MonoBehaviour
{
    #region Singleton
    private string savePath;                   // File path for save file
    [HideInInspector]
    public AutoSaveManager instance;           // Singleton instance (not static for inspector visibility)
    #endregion

    // Reference to the current save data
    public SaveData CurrentData { get; private set; } = new SaveData();
    

    #region Properties
    // CurrentData is now defined above for better organization
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Build save path once
        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        // Ensure only one AutoSaveManager exists
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    #region Save Methods
    /// <summary>
    /// Creates a save file after completing an objective.
    /// Stores player state, puzzles, inventory, etc.
    /// </summary>
    public void SaveGame(Transform player)
    {
        // Build save data snapshot
        SaveData data = new SaveData
        {
            playerPosX = player.position.x,
            playerPosY = player.position.y,
            playerPosZ = player.position.z,
            timestamp = System.DateTime.Now.ToString(),
            lastSceneName = SceneManager.GetActiveScene().name,
            sceneIndex = SceneManager.GetActiveScene().buildIndex,
        };

        // Save states of all objects implementing ISaveable
        foreach (var saveable in FindObjectsOfType<MonoBehaviour>(true))
        {
            if (saveable is ISaveable s)
                s.SaveState(ref data);
        }

        // Save objective states
        foreach (var objective in ObjectiveManager.Instance.completedObjectives)
        {
            if (objective is ISaveable s)
            {
                s.SaveState(ref data);
               
            }
        }

        // Convert to JSON and write to disk
        CurrentData = data;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log(" Game Autosaved!");
        GameService.Instance.UIService.ShowMessage("Game Autosaved!", 1.5f);
    }
    #endregion

    #region Load Methods
    /// <summary>
    /// Loads player and world state from save file.
    /// </summary>
    public void LoadGame(Transform player)
    {
        if (!File.Exists(savePath))
            return; // Nothing to load yet

        // Read and deserialize save
        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        CurrentData = data;

        // Ensure scene name matches
        data.lastSceneName = SceneManager.GetActiveScene().name;

        // Restore player state
        player.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);

        // Restore states of ISaveable objects
        foreach (var saveable in FindObjectsOfType<MonoBehaviour>(true))
        {
            if (saveable is ISaveable s)
                s.LoadState(data);
        }

    }
    

    /// <summary>
    /// Reloads saved objectives and child objectives from CurrentData.
    /// </summary>
    public void LoadObjectives()
    {
        foreach (var objective in GameManager.Instance.completedObjectives)
        {
            if (objective is ISaveable s)
            {
                s.LoadState(CurrentData);

                // Restore child objectives too
                if (objective.ChildObjectives != null && objective.ChildObjectives.Count > 0)
                {
                    foreach (var child in objective.ChildObjectives)
                        child.LoadState(CurrentData);
                }
            }
        }
    }
    #endregion
}
