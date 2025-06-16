using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

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

    [Header("Item Prefabs")]
    public GameObject bananaPrefab;
    public GameObject rainbowPotionPrefab;
    public GameObject wormholePrefab;

    private SnakeController snakeController;
    private Transform levelContainer;
    private Dictionary<Vector2Int, FoodItem> foodItemsOnGrid = new Dictionary<Vector2Int, FoodItem>();
    private Transform foodContainer;
    private Wormhole activeWormhole;

    void Start()
    {
        GenerateLevel();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateLevel();
        }
    }
    
    public Dictionary<Vector2Int, FoodItem> GetAllFoodItems()
    {
        return foodItemsOnGrid;
    }

    public void GenerateLevel()
    {
        if (currentLevelData == null || grid == null || tilemapPrefab == null || snakeControllerPrefab == null) { return; }
        if (levelContainer != null) { Destroy(levelContainer.gameObject); }
        levelContainer = new GameObject("LevelContainer").transform;
        levelContainer.SetParent(grid.transform);
        if(activeWormhole != null) { Destroy(activeWormhole.gameObject); }
        GenerateLayerPerRow(currentLevelData.floorLayout, "Floor");
        GenerateLayerPerRow(currentLevelData.wallLayout, "Wall");
        foodItemsOnGrid.Clear();
        SpawnFoodItems(currentLevelData.foodPlacements.Select(fp => new FoodState { foodType = fp.foodType, position = fp.position }).ToList());
        SpawnWormhole();
        if (snakeController != null) { Destroy(snakeController.gameObject); }
        GameObject snakeInstance = Instantiate(snakeControllerPrefab, Vector3.zero, Quaternion.identity);
        snakeController = snakeInstance.GetComponent<SnakeController>();
        snakeController.Initialize(currentLevelData, grid, this);
        GameManager.Instance.StartLevel();
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
            if(layerName == "Wall") { renderer.sortingOrder = 40 - y; }
            else { renderer.sortingOrder = -20 - y; }
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

    private void SpawnFoodItems(List<FoodState> foodToSpawn)
    {
        if (foodContainer != null) { Destroy(foodContainer.gameObject); }
        foodContainer = new GameObject("FoodContainer").transform;
        foodContainer.SetParent(levelContainer);
        foodItemsOnGrid.Clear();
        if (foodToSpawn == null) return;
        foreach (var foodState in foodToSpawn)
        {
            GameObject prefabToSpawn = null;
            switch(foodState.foodType)
            {
                case FoodType.Banana: prefabToSpawn = bananaPrefab; break;
                case FoodType.RainbowPotion: prefabToSpawn = rainbowPotionPrefab; break;
            }
            if (prefabToSpawn != null)
            {
                if(foodItemsOnGrid.ContainsKey(foodState.position)) { continue; }
                GameObject foodInstance = Instantiate(prefabToSpawn, foodContainer);
                FoodItem foodItem = foodInstance.GetComponent<FoodItem>();
                if(foodItem == null) { continue; }
                foodItem.Initialize(foodState.position, foodState.foodType, grid, this);
                foodItemsOnGrid.Add(foodState.position, foodItem);
            }
        }
    }

    private void SpawnWormhole()
    {
        if (activeWormhole != null) { Destroy(activeWormhole.gameObject); }
        if (wormholePrefab != null)
        {
            GameObject wormholeInstance = Instantiate(wormholePrefab, levelContainer);
            activeWormhole = wormholeInstance.GetComponent<Wormhole>();
            if (activeWormhole != null) { activeWormhole.Initialize(currentLevelData.wormholePosition, grid); }
        }
    }
    
    public void OnFoodEaten() { if (foodItemsOnGrid.Count == 0 && activeWormhole != null) { activeWormhole.Activate(); } }
    public bool IsWormholeAt(Vector2Int position) { if (activeWormhole == null) return false; return activeWormhole.IsAtPosition(position) && activeWormhole.IsActive(); }
    public List<FoodState> GetCurrentFoodStates()
    {
        List<FoodState> currentStates = new List<FoodState>();
        foreach(var foodItem in foodItemsOnGrid.Values) { currentStates.Add(new FoodState { foodType = foodItem.foodType, position = foodItem.gridPosition }); }
        return currentStates;
    }
    public void LoadFoodState(List<FoodState> foodStates)
    {
        if(activeWormhole != null) { activeWormhole.Initialize(currentLevelData.wormholePosition, grid); }
        SpawnFoodItems(foodStates);
        OnFoodEaten();
    }
    public bool IsPositionWalkable(Vector2Int position) { if (!IsPositionInBounds(position)) return false; if (foodItemsOnGrid.ContainsKey(position)) return false; if (IsWallAt(position)) return false; return true; }
    public bool IsWallAt(Vector2Int position) { if (!IsPositionInBounds(position)) return false; return currentLevelData.wallLayout[position.y][position.x] == 'W'; }
    public bool IsPitAt(Vector2Int position) { if (!IsPositionInBounds(position)) return true; return currentLevelData.floorLayout[position.y][position.x] == 'P'; }
    private bool IsPositionInBounds(Vector2Int position) { return position.y >= 0 && position.y < currentLevelData.floorLayout.Count && position.x >= 0 && position.x < currentLevelData.floorLayout[position.y].Length; }
    public FoodItem GetFoodAt(Vector2Int position) { foodItemsOnGrid.TryGetValue(position, out FoodItem foodItem); return foodItem; }
    public void UpdateFoodPosition(Vector2Int oldPos, Vector2Int newPos) { if (foodItemsOnGrid.TryGetValue(oldPos, out FoodItem foodItem)) { foodItemsOnGrid.Remove(oldPos); foodItemsOnGrid.Add(newPos, foodItem); } }
    public void RemoveFoodAt(Vector2Int pos) { if (foodItemsOnGrid.ContainsKey(pos)) { foodItemsOnGrid.Remove(pos); } OnFoodEaten(); }
}