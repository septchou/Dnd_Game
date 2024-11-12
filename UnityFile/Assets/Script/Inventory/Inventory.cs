using System.Collections;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "NewInventory", menuName = "Character Creation/Inventory")]
public class Inventory : ScriptableObject
{
    public List<Item> items; //List of items in the inventory

    public Inventory()
    {
        items = new List<Item>();
    }

    //Add an item to the inventory
    public void AddItem(Item item)
    {
        //Check if the item is already in the inventory
        Item existingItem = items.Find(i => i.itemID == item.itemID);
        if (existingItem != null)
        {
            //If the item is already in the inventory, increase the quantity
            existingItem.quantity = existingItem.quantity + 1;
            Debug.Log("Added " + item.quantity + " " + item.itemName + " to inventory");
        }
        else
        {
            //If the item is not in the inventory, add it to the list
            items.Add(item);
            Debug.Log("Added " + item.quantity + " " + item.itemName + " to inventory");
        }
    }

    //Remove an item from the inventory
    public void RemoveItem(string itemID, int quantity) {
        Item item = items.Find(i => i.itemID == itemID);
        if (item != null)
        {
            item.quantity -= quantity;
            if (item.quantity <= 0)
            {
                items.Remove(item); //Remove the item from the inventory if the quantity is 0 or less
                Debug.Log("Removed " + item.itemName + " from inventory");
            }
        }
    }

    public Item GetItem(string itemID)
    {
        return items.Find(i => i.itemID == itemID);
    }
}