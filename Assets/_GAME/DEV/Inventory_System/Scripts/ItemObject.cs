using UnityEngine;

/// <summary>
/// Represents an item in the inventory system.
/// Each item is defined as a ScriptableObject asset.
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemObject : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;   // Name of the item (e.g., "Battery,Key")
    public Sprite icon;       // Icon displayed in the inventory UI

    [Header("Item Properties")]
    public ItemType type;     // The category/type of this item (Key,Battery etc.)
    [TextArea]
    public string description; // Short description (tooltip or lore text)
}

