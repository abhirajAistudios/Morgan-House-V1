using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages auto-saving and loading of game data (player position, objectives, inventory, puzzles, etc.)
/// </summary>
public class AutoSaveManager : MonoBehaviour
{
    #region Singleton
    private string savePath;
    public AutoSaveManager instance;
    #endregion

    #region Save Data Classes
    [System.Serializable]
    public class SaveData
    {
        public int objectivesCompleted;              // Number of objectives completed
        public float playerPosX, playerPosY, playerPosZ; // Player's position in world
        public string timestamp;                     // Save timestamp
        public string lastSceneName;                 // Scene where save occurred

        // Store different gameplay states
        public List<string> collectedItems = new();          // Unique IDs of collected items
        public List<PuzzleState> puzzles = new();            // Puzzle states
        public List<InventorySlotData> inventorySlots = new(); // Player inventory
        public List<ObjectiveDataSO> objectives = new();     // Objective states
        public List<DoorStateData> doors = new();            // Door states
        public FlashlightSaveData flashlightData = new();    // Flashlight state
    }

    [System.Serializable]
    public class PuzzleState
    {
        public string puzzleID;   // Unique puzzle identifier
        public bool isSolved;     // Whether the puzzle is solved
    }

    [System.Serializable]
    public class DoorStateData
    {
        public string doorID;     // Unique door identifier
        public DoorState doorState;
        public bool isOpen;       // Open/Closed state
    }

    [System.Serializable]
    public class FlashlightSaveData
    {
        public bool hasFlashlight;
        public bool requiresBattery;
        public float currentBattery;
        public bool isOn;
    }

    [System.Serializable]
    public class InventorySlotData
    {
        public string itemName;
        public int quantity;
        public int slotIndex; // Slot index in the inventory UI
    }

    [System.Serializable]
    public class FlashlightData
    {
        public bool hasFlashlight;
        public bool isOn;
        public float currentBattery;
    }
    #endregion

    #region Properties
    public SaveData CurrentData { get; private set; }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Define path for save file
        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        // Singleton setup
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    #region Save Methods
    /// <summary>
    /// Saves progress after completing an objective.
    /// </summary>
    public void SaveAfterObjective(Transform player)
    {
        // Create save data
        SaveData data = new SaveData
        {
            objectivesCompleted = GameProgressTracker.ObjectivesCompleted,
            playerPosX = player.position.x,
            playerPosY = player.position.y,
            playerPosZ = player.position.z,
            timestamp = System.DateTime.Now.ToString(),
            lastSceneName = SceneManager.GetActiveScene().name // Save current scene
        };

        // Save all ISaveable states
        foreach (var saveable in FindObjectsOfType<MonoBehaviour>(true))
        {
            if (saveable is ISaveable s)
                s.SaveState(ref data);
        }

        // Save objectives
        foreach (var objective in ObjectiveManager.Instance.completedObjectives)
        {
            if (objective is ISaveable s)
            {
                s.SaveState(ref data);
                Debug.Log("✅ Saved objective: " + objective.dialogDisplay);
            }
        }

        // Write to JSON
        CurrentData = data;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("💾 Game Autosaved!");
        GameService.Instance.UIService.ShowMessage("Game Autosaved!", 1.5f);
    }
    #endregion

    #region Load Methods
    /// <summary>
    /// Loads saved game data (player position, objectives, states).
    /// </summary>
    public void LoadGame(Transform player)
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("⚠ No save file found.");
            return;
        }

        // Read save data from file
        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        CurrentData = data;
        data.lastSceneName = SceneManager.GetActiveScene().name;

        // Restore player position & progress
        player.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
        GameProgressTracker.ObjectivesCompleted = data.objectivesCompleted;

        // Restore ISaveable states
        foreach (var saveable in FindObjectsOfType<MonoBehaviour>(true))
        {
            if (saveable is ISaveable s)
                s.LoadState(data);
        }

        Debug.Log("✅ Game Loaded from save.");
    }

    /// <summary>
    /// Reloads saved objectives and their child objectives.
    /// </summary>
    public void LoadObjectives()
    {
        foreach (var objective in GameManager.Instance.completedObjectives)
        {
            if (objective is ISaveable s)
            {
                s.LoadState(CurrentData);

                // Restore child objectives
                if (objective.ChildObjectives != null && objective.ChildObjectives.Count > 0)
                {
                    foreach (var child in objective.ChildObjectives)
                    {
                        child.LoadState(CurrentData);
                    }
                }
            }
        }
    }
    #endregion
}
