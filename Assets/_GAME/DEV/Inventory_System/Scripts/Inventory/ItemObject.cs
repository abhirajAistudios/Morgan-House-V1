using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemObject : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    [Header("Item Properties")]
    public ItemType type;  // ? Add this field

    public string description;
}
