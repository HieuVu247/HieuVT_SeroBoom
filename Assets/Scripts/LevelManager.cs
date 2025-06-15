using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    [Header("Level Data")]
    public LevelData currentLevelData;

    [Header("Scene References")]
    [Tooltip("The Tilemap for drawing floors and pits.")]
    public Tilemap floorTilemap;
    [Tooltip("The Tilemap for drawing walls, which will appear on top of the floor.")]
    public Tilemap wallTilemap;
    public GameObject snakeControllerPrefab;

    [Header("Tile Assets")]
    [Tooltip("The single Tile/RuleTile used for the floor. Can be a CheckerboardRuleTile.")]
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase pitTile;

    private SnakeController snakeController;

    void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        if (currentLevelData == null || floorTilemap == null || wallTilemap == null || snakeControllerPrefab == null)
        {
            Debug.LogError("One or more references are not assigned in the LevelManager!");
            return;
        }

        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        GenerateLayer(floorTilemap, currentLevelData.floorLayout);
        GenerateLayer(wallTilemap, currentLevelData.wallLayout);
        
        if (snakeController != null)
        {
            Destroy(snakeController.gameObject);
        }
        GameObject snakeInstance = Instantiate(snakeControllerPrefab, Vector3.zero, Quaternion.identity);
        snakeController = snakeInstance.GetComponent<SnakeController>();
        snakeController.Initialize(currentLevelData, floorTilemap);
    }

    private void GenerateLayer(Tilemap targetTilemap, System.Collections.Generic.List<string> layout)
    {
        for (int y = 0; y < layout.Count; y++)
        {
            string row = layout[y];
            for (int x = 0; x < row.Length; x++)
            {
                char tileChar = row[x];
                TileBase selectedTile = null;

                switch (tileChar)
                {
                    case 'F':
                        selectedTile = floorTile; // Logic caro đã được chuyển vào trong Tile
                        break;
                    case 'W':
                        selectedTile = wallTile;
                        break;
                    case 'P':
                        selectedTile = pitTile;
                        break;
                }

                if (selectedTile != null)
                {
                    targetTilemap.SetTile(new Vector3Int(x, y, 0), selectedTile);
                }
            }
        }
    }
}