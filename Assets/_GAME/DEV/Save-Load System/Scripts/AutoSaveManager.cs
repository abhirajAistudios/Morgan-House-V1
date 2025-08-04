using UnityEngine;
using System.IO;
using System.Collections.Generic;

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

        // Store unique IDs of collected items
        public System.Collections.Generic.List<string> collectedItems = new();

        public System.Collections.Generic.List<PuzzleState> puzzles = new();

        public System.Collections.Generic.List<InventorySlotData> inventorySlots = new();

        public static AutoSaveManager Instance { get; private set; }
    }


    

    [System.Serializable]
    public class PuzzleState
    {
        public string puzzleID;
        public bool isSolved;
    }

    [System.Serializable]
    public class InventorySlotData
    {
        public string itemName;
        public int quantity;
        public int slotIndex;
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

        // Ask every ISaveable to record its state
        foreach (var saveable in FindObjectsOfType<MonoBehaviour>(true))
        {
            if (saveable is ISaveable s)
                s.SaveState(ref data);
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
}
