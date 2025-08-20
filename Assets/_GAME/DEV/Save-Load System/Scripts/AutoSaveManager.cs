using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSaveManager : MonoBehaviour
{
    private string savePath;
    public AutoSaveManager instance;

    [System.Serializable]
    
    public class SaveData
    {
        public int objectivesCompleted;
        public float playerPosX;
        public float playerPosY;
        public float playerPosZ;
        public string timestamp;
        public string lastSceneName;   // <-- add this

        // Store unique IDs of collected items
        public List<string> collectedItems = new();

        public List<PuzzleState> puzzles = new();

        public List<InventorySlotData> inventorySlots = new();
        
        public List<ObjectiveDataSO>  objectives = new();

        public List<DoorStateData> doors = new();
        public FlashlightSaveData flashlightData = new FlashlightSaveData();
        public static AutoSaveManager Instance { get; private set; }
    }

    [System.Serializable]
    public class PuzzleState
    {
        public string puzzleID;
        public bool isSolved;
    }

    [System.Serializable]

   
    public class DoorStateData
    {
        public string doorID;
        public DoorState doorState;
        public bool isOpen;
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
        public int slotIndex;
    }

    [System.Serializable]
public class FlashlightData
{
    public bool hasFlashlight;
    public bool isOn;
    public float currentBattery;
}
    public SaveData CurrentData { get; private set; }

    public void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

    }

    public void SaveAfterObjective(Transform player)
    {
        SaveData data = new SaveData
        {
            objectivesCompleted = GameProgressTracker.ObjectivesCompleted,
            playerPosX = player.position.x,
            playerPosY = player.position.y,
            playerPosZ = player.position.z,
            timestamp = System.DateTime.Now.ToString()
        };

        // Save current scene
        data.lastSceneName = SceneManager.GetActiveScene().name;

        // Ask every ISaveable to record its state
        foreach (var saveable in FindObjectsOfType<MonoBehaviour>(true))
        {
            if (saveable is ISaveable s)
                s.SaveState(ref data);
        }

        foreach (var objective in ObjectiveManager.Instance.completedObjectives)
        {
            if (objective is ISaveable s)
            {
                s.SaveState(ref data);
                Debug.Log("Saved objective: " + objective.dialogDisplay);
            }
        }



        CurrentData = data;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("Game Autosaved!");
        GameService.Instance.UIService.ShowMessage("Game Autosaved!", 1.5f);
    }

    public void LoadGame(Transform player)
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found.");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
      
        CurrentData = data;
        data.lastSceneName = SceneManager.GetActiveScene().name;

        // Restore player position
        player.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
        GameProgressTracker.ObjectivesCompleted = data.objectivesCompleted;

        // Ask every ISaveable to restore its state
        foreach (var saveable in FindObjectsOfType<MonoBehaviour>(true))
        {
            if (saveable is ISaveable s)
                s.LoadState(data);
        }
        
        Debug.Log("Game Loaded from save.");
    }

    public void LoadObjectives()
    {
        foreach (var objective in GameManager.Instance.completedObjectives)
        {
            if (objective is ISaveable s)
            {
                s.LoadState(CurrentData);
                
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
}
