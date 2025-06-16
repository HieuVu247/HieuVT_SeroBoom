using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Data")]
    public LevelData currentLevelData;

    [Header("Scene References")]
    public Grid grid;
    public GameObject tilemapPrefab;
    public GameObject snakeControllerPrefab;

    [Header("Tile Assets")]
    public TileBase floorTileA;
    public TileBase floorTileB;
    public TileBase wallTile;
    public TileBase pitTile;

    [Header("Food Prefabs")]
    public GameObject bananaPrefab;
    public GameObject rainbowPotionPrefab;

    private SnakeController snakeController;
    private Transform levelContainer;
    private Dictionary<Vector2Int, FoodItem> foodItemsOnGrid = new Dictionary<Vector2Int, FoodItem>();

    void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        if (currentLevelData == null || grid == null || tilemapPrefab == null || snakeControllerPrefab == null)
        {
            Debug.LogError("LEVEL MANAGER: One or more critical references are not assigned in the Inspector!");
            return;
        }

        if (levelContainer != null) { Destroy(levelContainer.gameObject); }
        levelContainer = new GameObject("LevelContainer").transform;
        levelContainer.SetParent(grid.transform);

        foodItemsOnGrid.Clear();
        GenerateLayerPerRow(currentLevelData.floorLayout, "Floor");
        GenerateLayerPerRow(currentLevelData.wallLayout, "Wall");
        SpawnFoodItems();
        
        if (snakeController != null) { Destroy(snakeController.gameObject); }
        GameObject snakeInstance = Instantiate(snakeControllerPrefab, Vector3.zero, Quaternion.identity);
        snakeController = snakeInstance.GetComponent<SnakeController>();
        snakeController.Initialize(currentLevelData, grid, this);
    }

    private void GenerateLayerPerRow(List<string> layout, string layerName)
    {
        if (layout == null) return;
        Transform layerContainer = new GameObject(layerName + "Container").transform;
        layerContainer.SetParent(levelContainer);
        
        for (int y = 0; y < layout.Count; y++)
        {
            GameObject tilemapInstance = Instantiate(tilemapPrefab, layerContainer);
            tilemapInstance.name = $"{layerName}_Row_{y}";
            
            Tilemap tilemap = tilemapInstance.GetComponent<Tilemap>();
            TilemapRenderer renderer = tilemapInstance.GetComponent<TilemapRenderer>();
            renderer.sortingOrder = -y * 10;
            if(layerName == "Wall") { renderer.sortingOrder += 5; }

            string row = layout[y];
            for (int x = 0; x < row.Length; x++)
            {
                char tileChar = row[x];
                TileBase selectedTile = null;
                switch (tileChar)
                {
                    case 'F': selectedTile = ((x + y) % 2 == 0) ? floorTileA : floorTileB; break;
                    case 'W': selectedTile = wallTile; break;
                    case 'P': selectedTile = pitTile; break;
                }
                if (selectedTile != null) { tilemap.SetTile(new Vector3Int(x, y, 0), selectedTile); }
            }
        }
    }

    private void SpawnFoodItems()
    {
        if (currentLevelData.foodPlacements == null) return;
        Transform foodContainer = new GameObject("FoodContainer").transform;
        foodContainer.SetParent(levelContainer);

        foreach (var foodPlacement in currentLevelData.foodPlacements)
        {
            GameObject prefabToSpawn = null;
            switch(foodPlacement.foodType)
            {
                case FoodType.Banana: prefabToSpawn = bananaPrefab; break;
                case FoodType.RainbowPotion: prefabToSpawn = rainbowPotionPrefab; break;
            }

            if (prefabToSpawn != null)
            {
                if(foodItemsOnGrid.ContainsKey(foodPlacement.position)) { continue; }
                GameObject foodInstance = Instantiate(prefabToSpawn, foodContainer);
                FoodItem foodItem = foodInstance.GetComponent<FoodItem>();
                if(foodItem == null) { continue; }
                foodItem.Initialize(foodPlacement.position, foodPlacement.foodType, grid, this);
                foodItemsOnGrid.Add(foodPlacement.position, foodItem);
            }
        }
    }

    public bool IsPositionWalkable(Vector2Int position)
    {
        if (position.y < 0 || position.y >= currentLevelData.wallLayout.Count || position.x < 0 || position.x >= currentLevelData.wallLayout[position.y].Length) { return false; }
        if (foodItemsOnGrid.ContainsKey(position)) return false;
        if (IsWallAt(position)) return false;
        return true;
    }

    public bool IsWallAt(Vector2Int position)
    {
        if (position.y < 0 || position.y >= currentLevelData.wallLayout.Count || position.x < 0 || position.x >= currentLevelData.wallLayout[position.y].Length)
        {
            return false; // Coi bên ngoài bản đồ là KHÔNG có tường
        }
        return currentLevelData.wallLayout[position.y][position.x] == 'W';
    }

    public FoodItem GetFoodAt(Vector2Int position)
    {
        foodItemsOnGrid.TryGetValue(position, out FoodItem foodItem);
        return foodItem;
    }

    public void UpdateFoodPosition(Vector2Int oldPos, Vector2Int newPos)
    {
        if (foodItemsOnGrid.TryGetValue(oldPos, out FoodItem foodItem))
        {
            foodItemsOnGrid.Remove(oldPos);
            foodItemsOnGrid.Add(newPos, foodItem);
        }
    }
    
    public void RemoveFoodAt(Vector2Int pos)
    {
        if (foodItemsOnGrid.ContainsKey(pos))
        {
            foodItemsOnGrid.Remove(pos);
        }
    }
}