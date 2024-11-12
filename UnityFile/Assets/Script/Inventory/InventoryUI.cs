using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;//Reference to the inventory panel
    public Inventory playerInventory;      //Reference to the inventory 
    public GameObject itemSlotPrefab;     //Reference to the item slot prefab
    public Transform itemSlotContainer;

    //Test Item 
    public List<Item> items;


    // Start is called before the first frame update
    void Start()
    {
        //Hide the inventory panel when the game starts
        inventoryPanel.SetActive(false);
        UpdateInventoryUI();
    }

    public void ToggleInventory()
    {
        //Toggle the visibility of the inventory panel
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    public void AddItemtoInventory(Item item)
    {
        Debug.Log("playerInventory: " + playerInventory);
        Debug.Log("testItem: " + item);

        if (playerInventory != null && item != null)
        {
            playerInventory.AddItem(item); //Add the test item to the inventory
            Debug.Log("Added item: " + item.itemName);
            UpdateInventoryUI(); // Update the inventory UI
        }
        else
        {
            Debug.LogError("Inventory or Test Item is missing.");
        }
    }

    public void UpdateInventoryUI()
    {
        // Destroy all existing Item Slot
        foreach (Transform child in itemSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // Create a new Item Slot for each item in the inventory
        foreach (Item item in playerInventory.items)
        {
            GameObject itemSlot = Instantiate(itemSlotPrefab, itemSlotContainer);
            ItemSlot slot = itemSlot.GetComponent<ItemSlot>();
            if (slot != null)
            {
                slot.Setup(item); // Set up the Item Slot with the item data
            }
        }
    }

}
