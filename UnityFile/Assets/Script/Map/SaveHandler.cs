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
using TMPro;
using Unity.UI;
using UnityEngine.UI;
using System.Xml.Serialization;
using Google.MiniJSON;

public class SaveHandler : Singleton<SaveHandler> {
    Dictionary<string, Tilemap> tilemaps = new Dictionary<string, Tilemap>();
    Dictionary<TileBase, BuildingObjectBase> tileBaseToBuildingObject = new Dictionary<TileBase, BuildingObjectBase>();
    Dictionary<string, TileBase> guidToTileBase = new Dictionary<string, TileBase>();

    [SerializeField] BoundsInt bounds;
    

    private DatabaseReference databaseReference;
    private FirebaseAuth auth;

    [Header("Save/Load")]
    [SerializeField] List<string> mapNames = new List<string>();
    [SerializeField] int selectedMapIndex;
    [SerializeField] TMP_InputField inputSave;
    [SerializeField] TMP_Dropdown dropdownLoad;
    [SerializeField] Button loadButton;

    private void Start() {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

                InitTilemaps();
                InitTileReferences();
                Debug.Log("Firebase initialized successfully(Map).");

                // Load map
                OnLoadFromFirebase();

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
        Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach (var map in maps) {
            string key = map.name; // ใช้ชื่อ Tilemap เป็น Key
            if (!tilemaps.ContainsKey(key)) {
                tilemaps.Add(key, map);
                Debug.Log($"Added Tilemap to dictionary: Key = {key}, Name = {map.name}");
            }
        }
    }
    string GenerateUniqueKey()
    {
        return Guid.NewGuid().ToString();
    }

    private void ResetMap()
    {
        foreach (var mapObj in tilemaps)
        {
            mapObj.Value.ClearAllTiles();//Clear all tiles
        }

        Debug.Log("Map has been reset.");
    }


    public void OnLoadSelectedMapFromFirebase()
    {
        if (dropdownLoad.options.Count == 0)
        {
            Debug.LogWarning("No maps available to load.");
            return;
        }

        selectedMapIndex = dropdownLoad.value;
        if (selectedMapIndex == 0) // "New map"
        {
            Debug.Log("Creating new map...");
            ResetMap(); //Reset map
            inputSave.text = ""; // Clear input field
        }
        else
        {
            string selectedMapKey = mapNames[selectedMapIndex];
            Debug.Log($"Loading map with key: {selectedMapKey}");
            OnLoadMapFromFirebase(selectedMapKey);
            inputSave.text = dropdownLoad.options[selectedMapIndex].text;
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

        string mapName = inputSave.text;

        // If selectedMapIndex is 0, then we are creating a new map
        string mapKey;
        if (selectedMapIndex == 0) // "New map"
        {
            mapKey = GenerateUniqueKey();
            Debug.Log("Generated new mapKey: " + mapKey);
        }
        else 
        {
            mapKey = mapNames[selectedMapIndex];
            Debug.Log("Updating existing mapKey: " + mapKey);
        }

        List<TilemapData> data = new List<TilemapData>();
        //Convert to json
        foreach (var mapObj in tilemaps)
        {
            TilemapData mapData = new TilemapData();
            mapData.key = mapObj.Key;

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
            data.Add(mapData);
        }

        // Wrap the TilemapData list into a TilemapDataWrapper
        TilemapDataWrapper wrapper = new TilemapDataWrapper(data);

        // Convert the wrapper into JSON
        string json = JsonUtility.ToJson(wrapper);

        //Save to database
        MapKeyData mapKeyData = new MapKeyData(mapName); //mapName = map.name
        string keyJson = JsonUtility.ToJson(mapKeyData);
        databaseReference.Child("tilemap_data").Child(userId).Child("keys").Child(mapKey).SetRawJsonValueAsync(keyJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"Key '{mapKey}' with name '{mapName}' saved successfully to Firebase.");
            }
            else
            {
                Debug.LogError("Failed to save key: " + task.Exception);
            }
        });

        databaseReference.Child("tilemap_data").Child(userId).Child(mapKey).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"Tilemap data saved successfully to Firebase with key: {mapKey}");
                
            }
            else
            {
                Debug.LogError("Failed to save tilemap data: " + task.Exception);
            }
        });

        OnLoadFromFirebase();
    }

    public void OnLoadFromFirebase()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }

        string userId = auth.CurrentUser.UserId;

        databaseReference.Child("tilemap_data").Child(userId).Child("keys").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                dropdownLoad.ClearOptions();
                mapNames.Clear();

                dropdownLoad.options.Add(new TMP_Dropdown.OptionData("New map"));
                mapNames.Add("New map");

                foreach (DataSnapshot keySnapshot in snapshot.Children)
                {
                    string mapKey = keySnapshot.Key;
                    string mapName = keySnapshot.Child("name").Value.ToString();

                    Debug.Log($"Found map key: {mapKey}, name: {mapName}");

                    dropdownLoad.options.Add(new TMP_Dropdown.OptionData(mapName));

                    mapNames.Add(mapKey);
                }

                dropdownLoad.RefreshShownValue();
            }
            else
            {
                Debug.LogError("Failed to load map keys: " + task.Exception);
            }
        });
    }

    public void OnLoadMapFromFirebase(string mapKey)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }

        if(selectedMapIndex == 0)
        {
            ResetMap();
            return;
        }

        string userId = auth.CurrentUser.UserId;

        databaseReference.Child("tilemap_data").Child(userId).Child(mapKey).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                // If data exists, retrieve the JSON
                string json = task.Result.GetRawJsonValue();

                // Deserialize the JSON into a TilemapDataWrapper
                TilemapDataWrapper wrapper = JsonUtility.FromJson<TilemapDataWrapper>(json);

                foreach (var mapData in wrapper.tilemapDataList)
                {
                    // if key does NOT exist in dictionary skip it
                    if (!tilemaps.ContainsKey(mapData.key))
                    {
                        Debug.LogError("Found saved data for tilemap called '" + mapData.key + "', but Tilemap does not exist in scene.");
                        continue;
                    }

                    // get according map
                    var map = tilemaps[mapData.key];

                    // clear map
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
                                Debug.LogError("Refernce " + tile.guidForBuildable + " could not be found.");
                            }

                        }
                    }
                }

            }
            else
            {
                Debug.LogError("Failed to load tilemap data or no data exists at the given key.");
            }
        });
    }

    private string GetTilemapNameFromKey(string key)
    {
        return tilemaps.ContainsKey(key) ? tilemaps[key].name : null;
    }

    private Tilemap FindTilemapByName(string name)
    {
        name = name.Trim(); // Delete spaces 
        Debug.Log($"Searching Tilemap by name: {name}");

        Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach (var map in maps)
        {
            Debug.Log($"Found Tilemap: {map.name}");
            if (map.name == name)
            {
                Debug.Log($"Match found: {map.name}");
                return map;
            }
        }

        Debug.LogWarning($"Tilemap not found: {name}");
        return null;
    }

    public void OnDeleteMapFromFirebase()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }

        if (dropdownLoad.options.Count == 0 || selectedMapIndex == 0)
        {
            Debug.LogWarning("No map selected to delete.");
            return;
        }

        selectedMapIndex = dropdownLoad.value;

        if (selectedMapIndex == 0) return;

        string selectedMapKey = mapNames[selectedMapIndex];

        string userId = auth.CurrentUser.UserId;

        // Confirm deletion
        Debug.Log($"Attempting to delete map with key: {selectedMapKey}");

        // Remove map key from Firebase
        databaseReference.Child("tilemap_data").Child(userId).Child("keys").Child(selectedMapKey).RemoveValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"Successfully deleted map key: {selectedMapKey} from Firebase.");
                }
                else
                {
                    Debug.LogError("Failed to delete map key: " + task.Exception);
                }
            });

        // Remove actual map data from Firebase
        databaseReference.Child("tilemap_data").Child(userId).Child(selectedMapKey).RemoveValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"Successfully deleted map data for key: {selectedMapKey} from Firebase.");

                    // Refresh map list
                    OnLoadFromFirebase();
                }
                else
                {
                    Debug.LogError("Failed to delete map data: " + task.Exception);
                }
            });
    }



}


[System.Serializable]
public class TilemapData {
    public string key; // the key of your dictionary for the tilemap - here: the name of the map in the hierarchy
    public List<TileInfo> tiles = new List<TileInfo>();
}


[System.Serializable]
public class MapKeyData
{
    public string name;

    public MapKeyData(string name)
    {
        this.name = name;
    }
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

[System.Serializable]
public class TilemapDataWrapper
{
    public List<TilemapData> tilemapDataList;

    // Constructor to initialize the list
    public TilemapDataWrapper(List<TilemapData> tilemapDataList)
    {
        this.tilemapDataList = tilemapDataList;
    }
}