using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public ItemObject[] allItems;

    public static ItemDatabase Instance { get; private set; }

    private void OnEnable()
    {
        Instance = this;
    }

    public ItemObject GetItemByName(string name)
    {
        foreach (var item in allItems)
        {
            if (item != null && item.itemName == name)
                return item;
        }
        Debug.LogWarning($"⚠ Item '{name}' not found in ItemDatabase.");
        return null;
    }
}
