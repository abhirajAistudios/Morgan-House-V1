using System.Collections.Generic;
using static AutoSaveManager;

[System.Serializable]
public class SaveData
{
    public int objectivesCompleted;
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public string timestamp;

    public System.Collections.Generic.List<string> collectedItems = new();
    public System.Collections.Generic.List<PuzzleState> puzzles = new();
    public System.Collections.Generic.List<InventorySlotData> inventorySlots = new();
    public FlashlightSaveData flashlightData = new FlashlightSaveData();

    public List<DoorStateData> doors = new();
}
