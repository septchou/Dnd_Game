using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;
using UnityEngine.SceneManagement;


public class ToolController : Singleton<ToolController> {
    List<Tilemap> tilemaps = new List<Tilemap>();
    public static event Action OnInitialized;

    protected override void Awake() {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
      if (scene.name == "MapMaker") {
            Initialize();
            OnInitialized?.Invoke(); // Notify when initialization is complete
        }
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Initialize() {
        Start();
    }
    
    private void Start() {
        // Ensure TilemapInitializer is ready before proceeding
        TilemapInitializer initializer = TilemapInitializer.GetInstance();
        if (initializer == null) {
            Debug.LogError("TilemapInitializer is not initialized!");
            return;
        }

        // Collect and sort Tilemaps
        List<Tilemap> maps = FindObjectsOfType<Tilemap>().ToList();
        maps.ForEach(map => {
            if (map.name != "BuildPreview") {
                tilemaps.Add(map);
            }
        });

        tilemaps.Sort((a, b) => {
            TilemapRenderer aRenderer = a.GetComponent<TilemapRenderer>();
            TilemapRenderer bRenderer = b.GetComponent<TilemapRenderer>();
            return bRenderer.sortingOrder.CompareTo(aRenderer.sortingOrder);
        });
    }

    public void Eraser(Vector3Int position) {
        tilemaps.Any(map => {
            if (map.HasTile(position)) {
                map.SetTile(position, null);
                return true;
            }
            return false;
        });
    }
}