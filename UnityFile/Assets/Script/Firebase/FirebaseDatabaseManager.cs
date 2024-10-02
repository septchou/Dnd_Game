using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseDatabaseManager : MonoBehaviour
{
    private DatabaseReference reference;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                // ??????? URL ??? Realtime Database
                reference = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
    }

    public void WriteData(string path, string json)
    {
        reference.Child(path).SetRawJsonValueAsync(json);
    }

    public void ReadData(string path)
    {
        reference.Child(path).GetValueAsync().ContinueWithOnMainThread(task => {
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
