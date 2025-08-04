using UnityEngine;

public class CollectibleItem : MonoBehaviour, ISaveable
{
    [Header("Reference to ItemObject ScriptableObject")]
    public ItemObject itemData;

    private bool isCollected = false;

    // Use itemData name as ID
    public string ItemName => itemData != null ? itemData.itemName : gameObject.name;

    public void Collect()
    {
        if (isCollected) 
            return;

        isCollected = true;
        gameObject.SetActive(false);

        Debug.Log("Collected: " + ItemName);
    }

    public void SaveState(ref AutoSaveManager.SaveData data)
    {

        if (isCollected && !data.collectedItems.Contains(ItemName))
        {
            data.collectedItems.Add(ItemName);
            Debug.Log("Saving collected item: " + ItemName);
        }

    }

    public void LoadState(AutoSaveManager.SaveData data)
    {
       
        // Check if this item was collected before
        if (data.collectedItems.Contains(ItemName))
        {
            isCollected = true;
            gameObject.SetActive(false); // Hide already collected items

        }
    }
}
