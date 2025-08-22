using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    #region Save Data Classes
    [System.Serializable]
    public class SaveData
    {
        public int objectivesCompleted;              // Number of objectives completed
        public float playerPosX, playerPosY, playerPosZ; // Player world position
        public string timestamp;                     // When the save was created
        public string lastSceneName;                 // Scene where save occurred

        // Game state collections
        public List<string> collectedItems = new();              // IDs of collected items
        public List<PuzzleState> puzzles = new();                // Puzzle states
        public List<InventorySlotData> inventorySlots = new();   // Player inventory
        public List<ObjectiveDataSO> objectives = new();         // Completed objective data
        public List<ObjectiveTrigger> objectiveTriggers = new();      // Triggered objectives
        public List<DoorStateData> doors = new();                // Door states
        public FlashlightSaveData flashlightData = new();        // Flashlight state
    }

    [System.Serializable]
    public class PuzzleState
    {
        public string puzzleID;   // Unique puzzle identifier
        public bool isSolved;     // True if solved
    }

    [System.Serializable]
    public class DoorStateData
    {
        public string doorID;     // Unique door identifier
        public DoorState doorState; // Current door state enum
        public bool isOpen;       // True if door is currently open
    }

    [System.Serializable]
    public class FlashlightSaveData
    {
        public bool hasFlashlight;   // If player owns flashlight
        public bool requiresBattery; // Whether flashlight requires battery
        public float currentBattery; // Remaining battery %
        public bool isOn;            // If flashlight is active
    }

    [System.Serializable]
    public class InventorySlotData
    {
        public string itemName;  // Item name from ItemDatabase
        public int quantity;     // Amount stored
        public int slotIndex;    // Slot index in inventory UI
    }


   
    #endregion

    #region Properties
    public SaveData CurrentData { get; private set; }  // Holds last loaded or saved data
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
    public void SaveAfterObjective(Transform player)
    {
        // Build save data snapshot
        SaveData data = new SaveData
        {
            playerPosX = player.position.x,
            playerPosY = player.position.y,
            playerPosZ = player.position.z,
            timestamp = System.DateTime.Now.ToString(),
            lastSceneName = SceneManager.GetActiveScene().name
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
