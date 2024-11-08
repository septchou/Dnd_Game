using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using Firebase.Auth;
using Firebase.Database;
using Firebase;
using Firebase.Extensions;


public class PlayerNameManager : MonoBehaviour
{
    [SerializeField] TMP_InputField usernameInput;

    private FirebaseAuth auth;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;

                if (auth.CurrentUser != null)
                {
                    LoadUsername();
                }
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
    }

    void LoadUsername()
    {
        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            //get the username from the display name 
            string username = user.DisplayName;
            usernameInput.text = username;
        }
        else
        {
                Debug.LogWarning("No user is currently logged in.");
        }
       
    }


}
