using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class SnakeController : MonoBehaviour
{
    private List<Vector2Int> snakeSegments;
    private Direction currentDirection;

    private SnakeVisuals snakeVisuals;

    void Awake()
    {
        snakeSegments = new List<Vector2Int>();
        snakeVisuals = GetComponent<SnakeVisuals>();
    }

    void Update()
    {
        // Logic di chuyển theo thời gian đã được chuyển vào HandleInput
        HandleInput();
    }

    public void Initialize(LevelData levelData, Tilemap tilemap)
    {
        this.currentDirection = levelData.initialDirection;
        snakeSegments.Clear();

        Vector2Int currentPosition = levelData.snakeStartPosition;
        for (int i = 0; i < levelData.initialSnakeLength; i++)
        {
            snakeSegments.Add(currentPosition);
            currentPosition -= GetVectorForDirection(this.currentDirection);
        }

        snakeVisuals.Initialize(snakeSegments, tilemap);
    }

    private void HandleInput()
    {
        // Rắn chỉ di chuyển khi có input
        if (Input.GetKeyDown(KeyCode.UpArrow) && currentDirection != Direction.Down)
        {
            currentDirection = Direction.Up;
            Move();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && currentDirection != Direction.Up)
        {
            currentDirection = Direction.Down;
            Move();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && currentDirection != Direction.Right)
        {
            currentDirection = Direction.Left;
            Move();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && currentDirection != Direction.Left)
        {
            currentDirection = Direction.Right;
            Move();
        }
    }

    private void Move()
    {
        if (snakeSegments.Count == 0) return;

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