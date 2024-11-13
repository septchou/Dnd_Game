using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;//Reference to the inventory panel
    public Inventory playerInventory;      //Reference to the inventory 
    public GameObject itemSlotPrefab;     //Reference to the item slot prefab
    public Transform itemSlotContainer;

    [Header("Firebase")]
    public DatabaseReference databaseReference;
    public FirebaseAuth auth;

    //item
    public List<Item> items;

    // Start is called before the first frame update
    void Start()
    {
        //Hide the inventory panel when the game starts
        inventoryPanel.SetActive(false);

        //Connect to firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                LoadInventoryFromFirebase();
                string userId = auth.CurrentUser.UserId;
                playerInventory.InitializeFirebase(auth, databaseReference, userId);
                Debug.Log("Firebase is ready to use(InventoryUI)");
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
        UpdateInventoryUI();

    }

    //Load inventory items from firebase
    public void LoadInventoryFromFirebase()
    {
        //Clear old inventory
        playerInventory.items.Clear();

        databaseReference.Child("inventory").Child(auth.CurrentUser.UserId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (Transform child in itemSlotContainer)
                {
                    Destroy(child.gameObject); // Clear existing slots
                }

                foreach (DataSnapshot itemSnapshot in snapshot.Children)
                {
                    string json = itemSnapshot.GetRawJsonValue();
                    Debug.Log("json: " + json);

                    Item item = JsonUtility.FromJson<Item>(json);
                    Debug.Log("item: " + item);
                    if (item != null)
                    {
                        Debug.Log("item: " + item);
                        AddItemtoInventory(item);
                        Debug.Log("Loaded item: " + item.itemName);
                    }
                    else
                    {
                        Debug.LogError("Failed to load item from Firebase");
                    }
                }
                
            }
            else
            {
                Debug.LogError("Failed to load inventory: " + task.Exception);
            }
        });
    }

    public void ToggleInventory()
    {
        //Toggle the visibility of the inventory panel
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    public void AddItemtoInventory(Item item, string specifiedUserId = null)
    {
        Debug.Log("playerInventory: " + playerInventory);
        Debug.Log("testItem: " + item);

        if (playerInventory != null && item != null)
        {
            if(specifiedUserId != null)
            {
                playerInventory.AddItem(item, specifiedUserId);
                Debug.Log("Added item: " + item.itemName + "to" + specifiedUserId);
            }
            else
            {
                playerInventory.AddItem(item);
                Debug.Log("Added item: " + item.itemName + "to your Self");
            }
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
