using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGame : Singleton<TilemapGame>
{
    [SerializeField] List<BuildingCategory> categoriesToCreateTilemapFor;
    [SerializeField] Transform grid;

    private void Start()
    {
        CreateMaps();
    }

    private void CreateMaps()
    {
        foreach (BuildingCategory category in categoriesToCreateTilemapFor)
        {
            // Create new GameObject
            GameObject obj = new GameObject("Tilemap_" + category.name);

            // Assign Tilemap Features
            Tilemap map = obj.AddComponent<Tilemap>();
            TilemapRenderer tr = obj.AddComponent<TilemapRenderer>();

            // Set Parent
            obj.transform.SetParent(grid);

            // Set sorting order
            tr.sortingOrder = category.SortingOrder;

            // Save reference to the Tilemap in the category
            category.Tilemap = map;

            // Add collider and other components based on category
            if (category.name == "Wall")
            {
                AddWallCollider(obj);
            }
        }
    }

    private void AddWallCollider(GameObject tilemapGameObject)
    {
        // Add TilemapCollider2D
        TilemapCollider2D tilemapCollider = tilemapGameObject.AddComponent<TilemapCollider2D>();

        // Add a Rigidbody2D to freeze the tilemap in place
        Rigidbody2D rb = tilemapGameObject.AddComponent<Rigidbody2D>();
    
        // Set the Rigidbody to be Static so it doesn't move
        rb.bodyType = RigidbodyType2D.Static;


        Debug.Log("Wall Tilemap with Collider and Rigidbody added: " + tilemapGameObject.name);
    }
    
}