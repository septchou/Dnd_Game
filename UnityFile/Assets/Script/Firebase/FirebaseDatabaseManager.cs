using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using UnityEngine;

public class FirebaseDatabaseManager : MonoBehaviour
{
    private DatabaseReference reference;

    void Start()
    {
        // Set the Database URL for Realtime Database
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri("https://dnd-game-518f0-default-rtdb.asia-southeast1.firebasedatabase.app/");

        // Access the root reference after setting the URL
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void WriteData(string path, string json)
    {
        reference.Child(path).SetRawJsonValueAsync(json);
    }

    public void ReadData(string path)
    {
        reference.Child(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error reading data: " + task.Exception.ToString());
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("Data: " + snapshot.GetRawJsonValue());
            }
        });
    }
}
