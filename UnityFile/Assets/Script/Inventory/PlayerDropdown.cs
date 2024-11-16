using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun;
using TMPro;

public class PlayerDropdown : MonoBehaviourPunCallbacks
{
    public TMP_Dropdown playerDropdown;
    public Dictionary<string, string> playerList = new Dictionary<string, string>();
    private FirebaseAuth auth;
    private DatabaseReference databaseReference;
    public InventoryUI inventoryUI;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase is ready to use(PlayerDropdown)");
                LoadPlayerListFromFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });

        // Add Listener for dropdown value change
        playerDropdown.onValueChanged.AddListener(delegate { OnPlayerSelected(); });
    }

    public void OnPlayerSelected()
    {
        if (playerDropdown.options.Count > 0)
        {
            string selectedPlayerName = playerDropdown.options[playerDropdown.value].text;

            if (playerList.TryGetValue(selectedPlayerName, out string selectedUserId))
            {
                Debug.Log($"Selected Player: {selectedPlayerName}, User ID: {selectedUserId}");
                inventoryUI.LoadInventoryFromFirebase(selectedUserId); // Load inventory for selected player
            }
            else
            {
                Debug.LogWarning("Selected player not found in the player list.");
            }
        }
    }


    private void LoadPlayerListFromFirebase()
    {
        databaseReference.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if(task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                playerList.Clear();
                playerDropdown.ClearOptions();

                List<string> options = new List<string>();
                string currentUserDisplayName = null;

                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    string userId = childSnapshot.Key;
                    if (childSnapshot.HasChild("displayName"))
                    {
                        string displayName = childSnapshot.Child("displayName").Value.ToString();
                        playerList.Add(displayName, userId);
                        options.Add(displayName);

                        if (auth.CurrentUser != null && userId == auth.CurrentUser.UserId)
                        {
                            currentUserDisplayName = displayName;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"User {userId} does not have a displayName.");
                    }

                }
                if (options.Count == 0)
                {
                    options.Add("No players available");
                    playerDropdown.interactable = false;
                }
                else
                {
                    playerDropdown.interactable = true;
                }

                playerDropdown.AddOptions(options);

                //Set Dropdown value to current user
                if (currentUserDisplayName != null)
                {
                    int index = options.IndexOf(currentUserDisplayName);
                    if (index >= 0)
                    {
                        playerDropdown.value = index; // Set the dropdown value to the current user
                    }
                }
            }
            else
            {
                Debug.LogError("Could not load player list from Firebase: " + task.Exception);
            }
        });
    }

    public string GetSelectedUserId()
    {
        string selectedDisplayName = playerDropdown.options[playerDropdown.value].text;
        return playerList.ContainsKey(selectedDisplayName) ? playerList[selectedDisplayName] : null;
    }


}
