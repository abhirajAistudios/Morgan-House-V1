using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the complete game state that gets saved to disk.
/// </summary>
[System.Serializable]
public class SaveData
{
    // Player State
    public float playerPosX, playerPosY, playerPosZ;
    public string timestamp;
    public string lastSceneName;

    // Game State
    public List<string> collectedItems = new();
    public List<PuzzleState> puzzles = new();
    public List<InventorySlotData> inventorySlots = new();
    public List<ObjectiveDataSO> objectives = new();
    public List<ObjectiveTrigger> objectiveTriggers = new();
    public List<DoorStateData> doors = new();
    public FlashlightSaveData flashlightData = new();
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