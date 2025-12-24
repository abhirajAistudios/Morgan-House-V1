using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the UI display of the inventory system,
/// including toggle animations and real-time refresh of item slots.
/// </summary>
public class UIInventory : MonoBehaviour
{
    #region Inspector Fields

    [Header("Inventory UI References")]
    [Tooltip("Main panel containing the entire inventory UI.")]
    public GameObject inventoryPanel;

    [Tooltip("Parent object holding item slots (Grid/VerticalLayoutGroup).")]
    public Transform itemListParent;

    [Tooltip("Prefab representing an inventory slot (Text + Icon).")]
    public GameObject itemSlotPrefab;

    #endregion

    #region Private Fields

    private bool isInventoryOpen = false;

    #endregion

    #region Unity Methods

    private void Start()
    {
        // Subscribe to inventory change callback
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChangedCallback += RefreshUI;
        }
        else
        {
            Debug.LogError("[UIInventory] InventoryManager instance not found!");
        }

        // Start hidden
        inventoryPanel.SetActive(false);

        // Ensure UI has slots equal to inventory size
        InitializeSlots();
    }

    private void Update()
    {
        // Toggle inventory UI on 'I' key press
        if (Input.GetKeyDown(KeyCode.I))
            ToggleInventory();
    }

    #endregion

    #region UI Logic

    /// <summary>
    /// Ensures UI has the correct number of slots based on InventoryManager.
    /// </summary>
    private void InitializeSlots()
    {
        if (InventoryManager.Instance == null) return;

        int requiredSlots = InventoryManager.Instance.slotCount;

        // If slots are fewer than required, instantiate new ones
        while (itemListParent.childCount < requiredSlots)
        {
            Instantiate(itemSlotPrefab, itemListParent);
        }

        // If extra slots exist (unlikely), disable them
        for (int i = 0; i < itemListParent.childCount; i++)
        {
            itemListParent.GetChild(i).gameObject.SetActive(i < requiredSlots);
        }
    }

    /// <summary>
    /// Opens or closes the inventory with animation.
    /// </summary>
    public void ToggleInventory()
    {
        if (isInventoryOpen)
        {
            // Close with shrink animation
            LeanTween.scale(inventoryPanel, Vector3.zero, 0.3f)
                     .setEaseInBack()
                     .setOnComplete(() => inventoryPanel.SetActive(false));
        }
        else
        {
            // Open with expand animation
            inventoryPanel.SetActive(true);
            inventoryPanel.transform.localScale = Vector3.zero;

            LeanTween.scale(inventoryPanel, Vector3.one, 0.3f)
                     .setEaseOutBack();

            RefreshUI();
        }

        isInventoryOpen = !isInventoryOpen;
    }

    /// <summary>
    /// Updates the inventory UI to reflect the current state of the inventory.
    /// </summary>
    public void RefreshUI()
    {
        if (InventoryManager.Instance == null) return;

        for (int i = 0; i < itemListParent.childCount; i++)
        {
            Transform slot = itemListParent.GetChild(i);
            
            Item slotItem = slot.GetComponent<Item>();

            // Get corresponding inventory item
            InventoryItem item = (i < InventoryManager.Instance.itemSlots.Length)
                                 ? InventoryManager.Instance.itemSlots[i]
                                 : null;

            if (item != null)
            {
                // Show item data
                slotItem.itemText.text = $"{item.quantity}";
                slotItem.itemImage.sprite = item.itemData.icon;
                slotItem.itemImage.color = Color.white;
            }
            else
            {
                // Show empty slot
                slotItem.itemText.text = "";
                slotItem.itemImage.sprite = null;
                slotItem.itemImage.color = new Color(1, 1, 1, 0); // Fully transparent
            }
        }
    }

    #endregion
}
