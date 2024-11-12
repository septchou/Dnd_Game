using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;//Reference to the inventory panel
    public Inventory playerInventory;      //Reference to the inventory 
    public GameObject itemSlotPrefab;     //Reference to the item slot prefab
    public Transform itemSlotContainer;


    //item
    public List<Item> items;

    //List item panel

    public GameObject listItemPanel;
    public GameObject itemButtonPrefab;
    public Transform itemButtonContainer;


    // Start is called before the first frame update
    void Start()
    {
        //Hide the inventory panel when the game starts
        inventoryPanel.SetActive(false);
        LoadItems();
        UpdateInventoryUI();
        UpdateListItemPanel();
    }
    void LoadItems()
    {
        // Load items from resources or any other source
        Item[] loadedItems = Resources.LoadAll<Item>("Items"); // Adjust the path if needed
        items.AddRange(loadedItems);
    }

    public void ToggleInventory()
    {
        //Toggle the visibility of the inventory panel
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    public void ToggleListOfItems()
    {
        listItemPanel.SetActive(!listItemPanel.activeSelf);
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

    void UpdateListItemPanel()
    {
        // Clear existing items
        foreach (Transform child in itemButtonContainer)
        {
            Destroy(child.gameObject);
        }

        //Create a new button for each item
        // Populate the panel with items
        foreach (Item item in items)
        {
            // Instantiate a button for each item
            GameObject itemButton = Instantiate(itemButtonPrefab, itemButtonContainer);
            itemButton.GetComponentInChildren<Text>().text = item.name; // Set item name text

        }
    }
}

