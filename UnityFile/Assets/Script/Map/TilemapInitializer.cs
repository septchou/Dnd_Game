using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class TilemapInitializer : Singleton<TilemapInitializer> {
    [SerializeField] List<BuildingCategory> categoriesToCreateTilemapFor; // List of categories for which Tilemaps will be created
    [SerializeField] Transform grid; // Reference to the Grid object in the scene
   public static event Action OnInitialized;

    protected override void Awake() {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Initialize();
        OnInitialized?.Invoke(); // Notify when initialization is complete
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Initialize() {
        CreateMaps();
    }

    private void CreateMaps() {
        foreach (BuildingCategory category in categoriesToCreateTilemapFor) {
            // Create a new GameObject for the Tilemap
            GameObject obj = new GameObject("Tilemap_" + category.name);

            // Add Tilemap and TilemapRenderer components
            Tilemap map = obj.AddComponent<Tilemap>();
            TilemapRenderer tr = obj.AddComponent<TilemapRenderer>();

            // Set the parent to the grid (to ensure proper hierarchy)
            obj.transform.SetParent(grid);

            // Configure rendering settings (e.g., sorting order)
            tr.sortingOrder = category.SortingOrder;

            // Store the Tilemap reference in the category for future use
            category.Tilemap = map;
        }
    }
}