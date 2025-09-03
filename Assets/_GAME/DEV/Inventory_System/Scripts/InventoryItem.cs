[System.Serializable]
public class InventoryItem
{
    public ItemObject itemData;
    public int quantity;

    public InventoryItem(ItemObject data)
    {
        itemData = data;
        quantity = 1;
    }

    public InventoryItem(ItemObject data, int qty)   // 👈 NEW constructor
    {
        itemData = data;
        quantity = qty;
    }

    public void AddQuantity(int amount)
    {
        quantity += amount;
    }

    public void Use()
    {
        // Example: reduce count by 1
        quantity--;
    }
}
