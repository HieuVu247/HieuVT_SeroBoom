using UnityEngine;
using System.Collections.Generic;

public enum TileType
{
    Floor,
    Wall,
    Pit
}

public enum FoodType
{
    Banana,
    RainbowPotion
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

[System.Serializable]
public class FoodPlacement
{
    public FoodType foodType;
    public Vector2Int position;
}

[CreateAssetMenu(fileName = "Level_New", menuName = "Puzzle Snake/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Identification")]
    [Tooltip("The name or number of the level for easy identification.")]
    public string levelName;

    [Header("Grid Dimensions")]
    [Tooltip("The width of the grid in number of cells.")]
    public int gridWidth = 10;
    [Tooltip("The height of the grid in number of cells.")]
    public int gridHeight = 10;

    [Header("Map Layout")]
    [Tooltip("Describes the layout of floors and pits. Use 'F' for Floor, 'P' for Pit.")]
    public List<string> floorLayout;
    [Tooltip("Describes the wall layout. Use 'W' for Wall and '.' or ' ' for empty space.")]
    public List<string> wallLayout;

    [Header("Snake Initial State")]
    [Tooltip("The starting position of the snake's head.")]
    public Vector2Int snakeStartPosition;
    [Tooltip("The number of segments the snake has at the start of the level.")]
    [Range(2, 10)]
    public int initialSnakeLength = 3;
    [Tooltip("The direction the snake is facing at the start.")]
    public Direction initialDirection = Direction.Up;

    [Header("Food and Items")]
    [Tooltip("A list of all food items and their positions for this level.")]
    public List<FoodPlacement> foodPlacements;
    
    [Header("Win Condition")]
    [Tooltip("The position where the wormhole will appear after all food is eaten.")]
    public Vector2Int wormholePosition;
}