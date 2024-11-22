using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.Timeline;
using UnityEngine.InputSystem;
using UnityEditor.U2D.Aseprite;

public class SaveHandler : Singleton<SaveHandler> {
    Dictionary<string, Tilemap> tilemaps = new Dictionary<string, Tilemap>();
    Dictionary<TileBase, BuildingObjectBase> tileBaseToBuildingObject = new Dictionary<TileBase, BuildingObjectBase>();
    Dictionary<string, TileBase> guidToTileBase = new Dictionary<string, TileBase>();

    [SerializeField] BoundsInt bounds;
    [SerializeField] string filename = "Map";

    private DatabaseReference databaseReference;
    private FirebaseAuth auth;


    private void Start() {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

                InitTilemaps();
                InitTileReferences();
                Debug.Log("Firebase initialized successfully(Map).");
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies.");
            }
        });
    }

    private void InitTileReferences()
    {
        BuildingObjectBase[] buildables = Resources.LoadAll<BuildingObjectBase>("Map/Scriptables/Buildables");

        foreach (BuildingObjectBase buildable in buildables)
        {
            if (!tileBaseToBuildingObject.ContainsKey(buildable.TileBase))
            {
                tileBaseToBuildingObject.Add(buildable.TileBase, buildable);
                guidToTileBase.Add(buildable.name, buildable.TileBase);
            }
            else
            {
                Debug.LogError("TileBase " + buildable.TileBase.name + " is already in use by " + tileBaseToBuildingObject[buildable.TileBase].name);
            }
        }
    }

    private void InitTilemaps() {
        // get all tilemaps from scene
        // and write to dictionary
        Tilemap[] maps = FindObjectsOfType<Tilemap>();

        // the hierarchy name must be unique
        // you might add some checks here to make sure
        foreach (var map in maps) {
            // if you have tilemaps you don't want to safe - filter them here
            tilemaps.Add(map.name, map);
        }
    }

    public void OnSaveToFirebase()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }

        string userId = auth.CurrentUser.UserId;

        // List that will later be saved
        TilemapDataListWrapper dataWrapper = new TilemapDataListWrapper();

        foreach (var mapObj in tilemaps)
        {
            TilemapData mapData = new TilemapData
            {
                key = mapObj.Key
            };

            BoundsInt boundsForThisMap = mapObj.Value.cellBounds;

            for (int x = boundsForThisMap.xMin; x < boundsForThisMap.xMax; x++)
            {
                for (int y = boundsForThisMap.yMin; y < boundsForThisMap.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tile = mapObj.Value.GetTile(pos);

                    if (tile != null && tileBaseToBuildingObject.ContainsKey(tile))
                    {
                        String guid = tileBaseToBuildingObject[tile].name;
                        TileInfo ti = new TileInfo(pos, guid);
                        mapData.tiles.Add(ti);
                    }
                }
            }

            dataWrapper.tilemaps.Add(mapData);
        }
        Debug.Log("DataWrapper: " + dataWrapper.tilemaps);
        string json = JsonUtility.ToJson(dataWrapper);
        Debug.Log("Generated JSON: " + json);


        Debug.Log("UserID:" + userId);
        // Check if mapKey exists, update or create a new one
        databaseReference.Child("tilemap_data").Child(userId).Child(filename).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log($"Tilemap data saved successfully to Firebase with name: {filename}");
            }
            else
            {
                Debug.LogError("Failed to save tilemap data: " + task.Exception);
                
            }
        });
    }

    public void OnLoadFromFirebase()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }

        string userId = auth.CurrentUser.UserId;

        databaseReference.Child("tilemap_data").Child(userId).Child(filename).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists)
                {
                    Debug.LogWarning("No tilemap data found for this user.");
                    return;
                }

                TilemapDataListWrapper dataWrapper = JsonUtility.FromJson<TilemapDataListWrapper>(snapshot.GetRawJsonValue());


                foreach (var mapData in dataWrapper.tilemaps)
                {
                    if (!tilemaps.ContainsKey(mapData.key))
                    {
                        Debug.LogError("Found saved data for tilemap called '" + mapData.key + "', but Tilemap does not exist in scene.");
                        continue;
                    }

                    var map = tilemaps[mapData.key];
                    map.ClearAllTiles();

                    if (mapData.tiles != null && mapData.tiles.Count > 0)
                    {
                        foreach (var tile in mapData.tiles)
                        {
                            if (guidToTileBase.ContainsKey(tile.guidForBuildable))
                            {
                                map.SetTile(tile.position, guidToTileBase[tile.guidForBuildable]);
                            }
                            else
                            {
                                Debug.LogError("Reference " + tile.guidForBuildable + " could not be found.");
                            }
                        }
                    }
                }

                Debug.Log("Tilemap data loaded successfully from Firebase.");
            }
            else
            {
                Debug.LogError("Failed to load tilemap data: " + task.Exception);
            }
        });
    }



}


[Serializable]
public class TilemapData {
    public string key; // the key of your dictionary for the tilemap - here: the name of the map in the hierarchy
    public List<TileInfo> tiles = new List<TileInfo>();
}

[Serializable]
public class TilemapDataListWrapper
{
    public List<TilemapData> tilemaps = new List<TilemapData>();
}


[Serializable]
public class TileInfo {
    public string guidForBuildable;
    public Vector3Int position;

    public TileInfo(Vector3Int pos, string guid) {
        position = pos;
        guidForBuildable = guid;
    }
}