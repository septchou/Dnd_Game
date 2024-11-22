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

public class SaveHandler : Singleton<SaveHandler> {
    Dictionary<string, Tilemap> tilemaps = new Dictionary<string, Tilemap>();
    Dictionary<TileBase, BuildingObjectBase> tileBaseToBuildingObject = new Dictionary<TileBase, BuildingObjectBase>();
    Dictionary<string, TileBase> guidToTileBase = new Dictionary<string, TileBase>();

    [SerializeField] BoundsInt bounds;
    

    private DatabaseReference databaseReference;
    private FirebaseAuth auth;

    [Header("Save/Load")]
    [SerializeField] List<string> mapName = new List<string>();
    [SerializeField] string filename;
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
    
        //loadButton.onClick.AddListener(OnLoadSelectedMapFromFirebase);
        //dropdownLoad.onValueChanged.AddListener(delegate { OnLoadSelectedMapFromFirebase(); });
        
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

    public void OnLoadSelectedMapFromFirebase()
    {
        if (dropdownLoad.options.Count == 0)
        {
            Debug.LogWarning("No maps available to load.");
            return;
        }

        int selectedIndex = dropdownLoad.value;
        string selectedMapKey = mapName[selectedIndex]; 

        Debug.Log($"Selected map key: {selectedMapKey}");

        
        OnLoadMapFromFirebase(selectedMapKey);
    }


    public void OnSaveToFirebase(string mapKey = null)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }

        string userId = auth.CurrentUser.UserId;

        string mapName = inputSave.text;

        // If it new map create key
        if (string.IsNullOrEmpty(mapKey))
        {
            mapKey = GenerateUniqueKey();
            Debug.Log("Generated new mapKey: " + mapKey);
        }

        /*TilemapData mapData = new TilemapData
        {
            key = mapKey,
            tiles = new List<TileInfo>()
        };*/
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
        string json = JsonUtility.ToJson(data);

        Debug.Log("Generated JSON: " + json);


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
                mapName.Clear();

                foreach (DataSnapshot keySnapshot in snapshot.Children)
                {
                    string mapKey = keySnapshot.Key;
                    string mapName = keySnapshot.Child("name").Value.ToString();

                    Debug.Log($"Found map key: {mapKey}, name: {mapName}");

                    Tilemap tilemap = FindTilemapByName(mapName);
                    if (tilemap != null)
                    {
                        tilemaps[mapKey] = tilemap;
                        dropdownLoad.options.Add(new TMP_Dropdown.OptionData(mapName));
                        this.mapName.Add(mapKey);
                    }
                    else
                    {
                        Debug.LogWarning($"Tilemap not found in scene for name: {mapName}");
                    }
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

        string userId = auth.CurrentUser.UserId;

        databaseReference.Child("tilemap_data").Child(userId).Child(mapKey).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists)
                {
                    Debug.LogWarning($"No tilemap data found for key: {mapKey}");
                    return;
                }

                TilemapData mapData = JsonUtility.FromJson<TilemapData>(snapshot.GetRawJsonValue());
                Debug.Log($"Loaded map data for key: {mapKey}");

                // Apply data to tilemaps
                Debug.Log("Applying Tilemap Data for key: " + mapData.key);
                ApplyTilemapData(mapData);
            }
            else
            {
                Debug.LogError("Failed to load tilemap data: " + task.Exception);
            }
        });
    }

    private void ApplyTilemapData(TilemapData mapData)
    {
        Debug.Log($"Applying Tilemap Data for key: {mapData.key}");

        if (!tilemaps.ContainsKey(mapData.key))
        {
            Debug.LogWarning($"No Tilemap found for key: {mapData.key}");
            return;
        }

        Tilemap map = tilemaps[mapData.key];
        map.ClearAllTiles();

        foreach (var tileInfo in mapData.tiles)
        {
            if (guidToTileBase.ContainsKey(tileInfo.guidForBuildable))
            {
                map.SetTile(tileInfo.position, guidToTileBase[tileInfo.guidForBuildable]);
            }
            else
            {
                Debug.LogError($"Tile not found for GUID: {tileInfo.guidForBuildable}");
            }
        }
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




}


[Serializable]
public class TilemapData {
    public string key; // the key of your dictionary for the tilemap - here: the name of the map in the hierarchy
    public List<TileInfo> tiles = new List<TileInfo>();
}

// [Serializable]
// public class TilemapDataListWrapper
// {
//     public List<TilemapData> tilemaps = new List<TilemapData>();
// }

[Serializable]
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