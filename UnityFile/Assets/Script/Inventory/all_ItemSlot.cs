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
    public InventoryUI inventory;

    // Set up the Item Slot with the item data
    public void Setup(Item item)
    {
        itemIcon.sprite = item.icon;
        itemName.text = item.itemName;

        addButton.onClick.RemoveAllListeners();
        addButton.onClick.AddListener(() => inventory.AddItemtoInventory(item));
    }
}

