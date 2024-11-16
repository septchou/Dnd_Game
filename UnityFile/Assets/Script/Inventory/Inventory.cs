using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System;


[CreateAssetMenu(fileName = "NewInventory", menuName = "Character Creation/Inventory")]
public class Inventory : ScriptableObject
{
    public List<Item> items; //List of items in the inventory
    private DatabaseReference databaseReference;
    private string userId;

    public Inventory()
    {
        items = new List<Item>();
    }

    //initialize firebase   
    public void InitializeFirebase(FirebaseAuth auth, DatabaseReference dbReference, string specifiedUserId)
    {
        databaseReference = dbReference;
        userId = specifiedUserId;
    }

    //Add an item to the inventory
    public void AddItem(Item item, string specifiedUserId = null)
    {
        //Check that want to add item to the specified user's inventory if not , add to your own inventory
        string targetUserId = specifiedUserId ?? userId;
        Debug.Log("Target User ID: " + targetUserId);
        if (databaseReference != null && !string.IsNullOrEmpty(targetUserId))
        {
            //Check if the item is already in the inventory
            Item existingItem = items.Find(i => i.itemID == item.itemID);
            if (existingItem != null)
            {
                //If the item is already in the inventory, increase the quantity
                existingItem.quantity++;
                Debug.Log("Added " + item.quantity + " " + item.itemName + " to inventory");
            }
            else
            {
                //If the item is not in the inventory, add it to the list
                item.quantity = 1;
                items.Add(item);
                Debug.Log("Added " + item.quantity + " " + item.itemName + " to inventory");
            }
            //UpdateItemInFirebase(existingItem, targetUserId);
        }
    }

    //Update an item in Firebase
    public void UpdateItemInFirebase(Item item, string targetUserId)
    {
        //Convert item to ItemData
        ItemData itemData = ItemData.FromItem(item);

        string json = JsonUtility.ToJson(itemData);

        //Update the item in Firebase
        databaseReference.Child("inventory").Child(targetUserId).Child(itemData.itemID).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"Item {item.itemName} updated in Firebase for user: {targetUserId}");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("Failed to add item to Firebase: " + task.Exception);
            }
        });
    }

    //Remove an item from the inventory
    public void RemoveItem(string itemID, int quantity, string specifiedUserId = null) {
        string targetUserId = specifiedUserId ?? userId;
        Item item = items.Find(i => i.itemID == itemID);
        if (item != null)
        {
            item.quantity -= quantity;
            if (item.quantity <= 0)
            {
                items.Remove(item); //Remove the item from the inventory if the quantity is 0 or less
                DeleteItemFromFirebase(itemID, targetUserId);
                Debug.Log("Removed " + item.itemName + " from inventory");
            }
            else
            {
                UpdateItemInFirebase(item, targetUserId);
                Debug.Log("Removed " + quantity + " " + item.itemName + " from inventory");
            }
        }
    }

    //Delete an item from Firebase
    private void DeleteItemFromFirebase(string itemID, string specifiedUserId = null)
    {
        string targetUserId = specifiedUserId ?? userId;
        databaseReference.Child("inventory").Child(targetUserId).Child(itemID).RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Item removed from Firebase");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("Failed to remove item from Firebase: " + task.Exception);
            }
        });
    }

    public Item GetItem(string itemID)
    {
        return items.Find(i => i.itemID == itemID);
    }
}