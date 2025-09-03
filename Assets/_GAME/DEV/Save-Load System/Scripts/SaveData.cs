using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the complete game state that gets saved to disk as JSON.
/// </summary>
[System.Serializable]
public class SaveData
{
    // ----------------- Player State -----------------

    // Player position in the world
    public float playerPosX, playerPosY, playerPosZ;

    // Timestamp when the save was created
    public string timestamp;

    // Scene name where the player last was
    public string lastSceneName;

    public int sceneIndex;

    // ----------------- Game State -----------------

    // List of collected item IDs/names
    public List<string> collectedItems = new();

    // Stores the state of all puzzles (solved or not)
    public List<PuzzleState> puzzles = new();

    // Stores player inventory slots
    public List<InventorySlotData> inventorySlots = new();

    // Objectives (quest progress) � Data from ScriptableObjects
    public List<ObjectiveDataSO> objectives = new();

    // Triggers that track whether objectives were completed
    public List<ObjectiveTrigger> objectiveTriggers = new();

    // Door states (locked/unlocked, open/closed)
    public List<DoorStateData> doors = new();

    // Flashlight state (battery, on/off, ownership)
    public FlashlightSaveData flashlightData = new();
}

/// <summary>
/// Stores puzzle progress (e.g., puzzle1 solved = true).
/// </summary>
[System.Serializable]
public class PuzzleState
{
    public string puzzleID;  // Unique puzzle identifier
    public bool isSolved;    // Whether the puzzle is solved
}

/// <summary>
/// Stores door state (open/closed, locked/unlocked).
/// </summary>
[System.Serializable]
public class DoorStateData
{
    public string doorID;      // Unique door identifier
    public DoorState doorState; // Enum describing door state
    public bool isOpen;        // Is the door currently open?
    public float lastOpenDirection;
    public float currentYRotation;
}

/// <summary>
/// Stores flashlight save data.
/// </summary>
[System.Serializable]
public class FlashlightSaveData
{
    public bool hasFlashlight;    // Does the player own a flashlight?
    public bool requiresBattery;  // Does it need a battery to work?
    public float currentBattery;  // Current battery percentage/charge
    public bool isOn;             // Is flashlight currently on?
}

/// <summary>
/// Stores data for a single inventory slot.
/// </summary>
[System.Serializable]
public class InventorySlotData
{
    public string itemName;  // Item stored in this slot
    public int quantity;     // How many items in this slot
    public int slotIndex;    // Slot index in the inventory grid
}
