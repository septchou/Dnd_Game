using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class all_ItemSlot : MonoBehaviour
{
    public Image itemIcon;
    public TMP_Text itemName;
    public Button addButton;
    public InventoryUI inventoryUI;

    public PlayerDropdown PlayerDropdown;


    // Set up the Item Slot with the item data
    public void Setup(Item item)
    {
        itemIcon.sprite = item.GetIcon();
        itemName.text = item.itemName;

        addButton.onClick.RemoveAllListeners();
        addButton.onClick.AddListener(() =>
        {
            string selectedUserID = PlayerDropdown.GetSelectedUserId();
            if (!string.IsNullOrEmpty(selectedUserID))
            {
                inventoryUI.AddItemtoInventory(item, 1, selectedUserID);
                Debug.Log("Added item: " + item.itemName + "to" + selectedUserID);
            }
            else
            {
                inventoryUI.AddItemtoInventory(item, 1);
                Debug.Log("Added item: " + item.itemName + "to your Self");
            }
        });

        addButton.onClick.AddListener(() =>
        {
            string selectedUserID = PlayerDropdown.GetSelectedUserId();
            inventoryUI.UpdateFirebase(selectedUserID);

        });
    }
}

