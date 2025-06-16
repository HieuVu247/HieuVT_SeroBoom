using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Linq;

public class SnakeController : MonoBehaviour
{
    private List<Vector2Int> snakeSegments;
    private Direction currentDirection;
    private SnakeVisuals snakeVisuals;
    private LevelManager levelManager;
    private Vector2Int lastTailPosition;

    void Awake()
    {
        snakeSegments = new List<Vector2Int>();
        snakeVisuals = GetComponent<SnakeVisuals>();
    }

    void Update()
    {
        HandleInput();
    }

    public void Initialize(LevelData levelData, Grid grid, LevelManager manager)
    {
        this.levelManager = manager;
        this.currentDirection = levelData.initialDirection;
        snakeSegments.Clear();
        Vector2Int currentPosition = levelData.snakeStartPosition;
        for (int i = 0; i < levelData.initialSnakeLength; i++)
        {
            snakeSegments.Add(currentPosition);
            currentPosition -= GetVectorForDirection(this.currentDirection);
        }
        if (snakeSegments.Count > 0) { lastTailPosition = snakeSegments[snakeSegments.Count - 1]; }
        snakeVisuals.Initialize(snakeSegments, grid);
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && currentDirection != Direction.Down) { AttemptMove(Direction.Up); }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && currentDirection != Direction.Up) { AttemptMove(Direction.Down); }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && currentDirection != Direction.Right) { AttemptMove(Direction.Left); }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && currentDirection != Direction.Left) { AttemptMove(Direction.Right); }
    }
    
    private void AttemptMove(Direction direction)
    {
        Vector2Int nextHeadPos = snakeSegments[0] + GetVectorForDirection(direction);
        for (int i = 1; i < snakeSegments.Count; i++)
        {
            if (snakeSegments[i] == nextHeadPos) return;
        }
        this.currentDirection = direction;
        FoodItem food = levelManager.GetFoodAt(nextHeadPos);
        if (food != null)
        {
            if (food.Push(GetVectorForDirection(currentDirection))) { MoveSnake(); }
            else { EatFood(food); }
        }
        else if (levelManager.IsPositionWalkable(nextHeadPos)) { MoveSnake(); }
    }

    private void EatFood(FoodItem food)
    {
        Direction moveDirection = this.currentDirection;
        levelManager.RemoveFoodAt(food.gridPosition);
        if (food.foodType == FoodType.Banana) { Grow(); }
        food.Consume();
        MoveSnake();
        if (food.foodType == FoodType.RainbowPotion) { ApplyRainbowPush(moveDirection); }
    }

    private void ApplyRainbowPush(Direction moveDirection)
    {
        Vector2Int pushDirection = GetVectorForDirection(moveDirection) * -1;
        
        while(true)
        {
            List<Vector2Int> pushedObjectsPositions = new List<Vector2Int>(snakeSegments);
            HashSet<FoodItem> foodToPush = new HashSet<FoodItem>();

            bool collisionDetected = false;
            
            // Lặp vô hạn để tìm tất cả các đối tượng bị đẩy theo chuỗi
            while(true)
            {
                int initialPushedCount = pushedObjectsPositions.Count;
                List<Vector2Int> nextIterationPushedObjects = new List<Vector2Int>(pushedObjectsPositions);

                foreach (var pos in pushedObjectsPositions)
                {
                    Vector2Int nextPos = pos + pushDirection;
                    FoodItem foodInNextPos = levelManager.GetFoodAt(nextPos);
                    if(foodInNextPos != null && !foodToPush.Contains(foodInNextPos))
                    {
                        nextIterationPushedObjects.Add(nextPos);
                        foodToPush.Add(foodInNextPos);
                    }
                }
                
                pushedObjectsPositions = nextIterationPushedObjects;
                if(pushedObjectsPositions.Count == initialPushedCount) break;
            }
            
            // Kiểm tra xem tất cả các đối tượng bị đẩy có thể di chuyển không
            foreach (var pos in pushedObjectsPositions)
            {
                Vector2Int nextPos = pos + pushDirection;
                if(levelManager.IsWallAt(nextPos) && !pushedObjectsPositions.Contains(nextPos))
                {
                    collisionDetected = true;
                    break;
                }
            }

            if(collisionDetected)
            {
                break; // Dừng vòng lặp đẩy chính
            }
            
            // Di chuyển tất cả
            for(int i = 0; i < snakeSegments.Count; i++) { snakeSegments[i] += pushDirection; }
            foreach(var food in foodToPush) { food.Push(pushDirection); }
        }
        
        snakeVisuals.UpdateVisuals();
    }

    private void Grow()
    {
        snakeSegments.Add(lastTailPosition);
    }

    private void MoveSnake()
    {
        if (snakeSegments.Count == 0) return;
        lastTailPosition = snakeSegments[snakeSegments.Count - 1];
        Vector2Int nextHeadPosition = snakeSegments[0] + GetVectorForDirection(currentDirection);
        for (int i = snakeSegments.Count - 1; i > 0; i--)
        {
            snakeSegments[i] = snakeSegments[i - 1];
        }
        snakeSegments[0] = nextHeadPosition;
        snakeVisuals.UpdateVisuals();
    }

    private Vector2Int GetVectorForDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Vector2Int.up;
            case Direction.Down: return Vector2Int.down;
            case Direction.Left: return Vector2Int.left;
            case Direction.Right: return Vector2Int.right;
            default: return Vector2Int.zero;
        }
    }
}