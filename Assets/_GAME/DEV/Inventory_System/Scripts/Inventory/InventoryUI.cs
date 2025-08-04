using System.Collections.Generic;
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
    public GameObject inventoryPanel;      // Main panel containing the entire inventory UI
    public Transform itemListParent;       // Parent object holding item slots (VerticalLayoutGroup)
    public GameObject itemSlotPrefab;      // Prefab representing an inventory slot (Text + Icon)

    #endregion

    #region Private Fields

    private bool isInventoryOpen = false;

    #endregion

    #region Unity Methods

    private void Start()
    {
        // Subscribe to inventory change callback
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.onInventoryChangedCallback += RefreshUI;
        else
            Debug.LogError("InventoryManager instance not found!");

        // Start with inventory hidden
        inventoryPanel.SetActive(false);
    }

    private void Update()
    {
        // Toggle inventory UI on 'I' key press
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    #endregion

    #region UI Logic

    /// <summary>
    /// Opens or closes the inventory with animation.
    /// </summary>
    public void ToggleInventory()
    {
        if (isInventoryOpen)
        {
            // Close: Animate shrinking and deactivate after
            LeanTween.scale(inventoryPanel, Vector3.zero, 0.3f)
                     .setEaseInBack()
                     .setOnComplete(() => inventoryPanel.SetActive(false));
        }
        else
        {
            // Open: Activate and scale in
            inventoryPanel.SetActive(true);
            inventoryPanel.transform.localScale = Vector3.zero;

            LeanTween.scale(inventoryPanel, Vector3.one, 0.3f)
                     .setEaseOutBack();

            // Show current inventory content
            RefreshUI();
        }

        isInventoryOpen = !isInventoryOpen;
    }

    /// <summary>
    /// Updates the inventory UI to reflect the current state of the inventory.
    /// </summary>
    public void RefreshUI()
    {
        for (int i = 0; i < itemListParent.childCount; i++)
        {
            Transform slot = itemListParent.GetChild(i);

            // Get UI elements
            var textComponent = slot.GetComponentInChildren<TextMeshProUGUI>();
            var imageComponent = slot.GetComponentInChildren<Image>();

            // Get item from InventoryManager
            InventoryItem item = InventoryManager.Instance.itemSlots[i];

            if (item != null)
            {
                // Show item name and quantity
                textComponent.text = $"{item.itemData.itemName} *{item.quantity}";
                imageComponent.sprite = item.itemData.icon;
                imageComponent.color = Color.white;
            }
            else
            {
                // Empty slot visuals
                textComponent.text = "Empty";
                imageComponent.sprite = null;
                imageComponent.color = new Color(1, 1, 1, 0); // Make image invisible
            }
        }
    }

    #endregion
}
