using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    public Image itemIcon;
    public TMP_Text itemQuantity;
    public Button RemoveBT;
    public InventoryUI inventoryUI;

    public PlayerDropdown PlayerDropdown;

    // Set up the Item Slot with the item data
    public void Setup(Item item)
    {
        itemIcon.sprite = item.GetIcon();
        itemQuantity.text = item.quantity > 1 ? "Quantity: " + item.quantity.ToString() : ""; // Show quantity if greater than 1
        RemoveBT.onClick.RemoveAllListeners();
        RemoveBT.onClick.AddListener(() =>
        {
            string selectedUserID = PlayerDropdown.GetSelectedUserId();
            if (!string.IsNullOrEmpty(selectedUserID))
            {
                inventoryUI.RemoveItemFromInventory(item, selectedUserID);
            }
            else
            {
                inventoryUI.RemoveItemFromInventory(item);
            }
        });
    }
}

