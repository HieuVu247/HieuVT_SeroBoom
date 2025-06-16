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
    private Stack<GameState> history = new Stack<GameState>();
    
    private SnakeFaceController faceController;
    private int droppedFoodCounter = 0;
    private bool isAnimating = false;

    void Awake()
    {
        snakeSegments = new List<Vector2Int>();
        snakeVisuals = GetComponent<SnakeVisuals>();
        faceController = GetComponent<SnakeFaceController>();
    }

    void OnEnable() { GameEvents.OnFoodDroppedInPit += HandleFoodDropped; }
    void OnDisable() { GameEvents.OnFoodDroppedInPit -= HandleFoodDropped; }

    void Update()
    {
        HandleInput();
    }

    public void Initialize(LevelData levelData, Grid grid, LevelManager manager)
    {
        this.levelManager = manager;
        this.currentDirection = levelData.initialDirection;
        history.Clear();
        snakeSegments.Clear();
        droppedFoodCounter = 0;
        isAnimating = false;
        Vector2Int currentPosition = levelData.snakeStartPosition;
        for (int i = 0; i < levelData.initialSnakeLength; i++)
        {
            snakeSegments.Add(currentPosition);
            currentPosition -= GetVectorForDirection(this.currentDirection);
        }
        if (snakeSegments.Count > 0) { lastTailPosition = snakeSegments[snakeSegments.Count - 1]; }
        snakeVisuals.Initialize(snakeSegments, grid);
        faceController.SetFace(FaceType.Normal);
        faceController.UpdateFaceRotation(currentDirection);
    }

    private void HandleInput()
    {
        if (isAnimating) return;
        if (Input.GetKeyDown(KeyCode.Z)) { Undo(); return; }
        if (GameManager.Instance.CurrentStatus != GameManager.GameStatus.Playing) return;
        if (Input.GetKeyDown(KeyCode.UpArrow) && currentDirection != Direction.Down) { AttemptMove(Direction.Up); }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && currentDirection != Direction.Up) { AttemptMove(Direction.Down); }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && currentDirection != Direction.Right) { AttemptMove(Direction.Left); }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && currentDirection != Direction.Left) { AttemptMove(Direction.Right); }
    }
    
    private void AttemptMove(Direction direction)
    {
        faceController.ResetToNormalFace();
        Vector2Int nextHeadPos = snakeSegments[0] + GetVectorForDirection(direction);
        for (int i = 1; i < snakeSegments.Count; i++) { if (snakeSegments[i] == nextHeadPos) return; }
        if (levelManager.IsWormholeAt(nextHeadPos)) { GameManager.Instance.WinLevel(); return; }
        
        this.currentDirection = direction;
        FoodItem food = levelManager.GetFoodAt(nextHeadPos);
        bool hasMoved = false;
        if (food != null)
        {
            SaveState();
            if (food.Push(GetVectorForDirection(currentDirection))) { MoveSnake(); hasMoved = true; }
            else { EatFood(food); hasMoved = true; }
        }
        else if (levelManager.IsPositionWalkable(nextHeadPos)) 
        {
            SaveState();
            MoveSnake();
            hasMoved = true;
        }
        if(!hasMoved) { history.Pop(); }
    }

    private void EatFood(FoodItem food)
    {
        Direction moveDirection = this.currentDirection;
        levelManager.RemoveFoodAt(food.gridPosition);
        if (food.foodType == FoodType.Banana) { Grow(); faceController.SetFace(FaceType.EatBanana); }
        
        food.Consume();
        MoveSnake();

        if (food.foodType == FoodType.RainbowPotion) { ApplyRainbowPush(moveDirection); }
    }

    private void ApplyRainbowPush(Direction moveDirection)
    {
        Vector2Int pushDirection = GetVectorForDirection(moveDirection) * -1;
        
        while(true)
        {
            bool canPush = true;
            foreach (var segment in snakeSegments)
            {
                Vector2Int nextPos = segment + pushDirection;
                if (levelManager.IsWallAt(nextPos)) { canPush = false; break; }

                FoodItem food = levelManager.GetFoodAt(nextPos);
                if (food != null)
                {
                    if (!food.Push(pushDirection)) { canPush = false; break; }
                }
            }
            
            if (!canPush) { break; }

            for (int i = 0; i < snakeSegments.Count; i++)
            {
                snakeSegments[i] += pushDirection;
            }
            snakeVisuals.UpdateVisuals_Instant();
            faceController.UpdateFaceRotation(this.currentDirection);
        }
        
        CheckForDeath();
    }

    private void HandleFoodDropped() { droppedFoodCounter++; if (droppedFoodCounter == 1) { faceController.SetFace(FaceType.FailPush1); } else { faceController.SetFace(FaceType.FailPush2); } }
    private void Grow() { snakeSegments.Add(lastTailPosition); }

    private void MoveSnake()
    {
        if (snakeSegments.Count == 0) return;
        lastTailPosition = snakeSegments[snakeSegments.Count - 1];
        Vector2Int nextHeadPosition = snakeSegments[0] + GetVectorForDirection(currentDirection);
        for (int i = snakeSegments.Count - 1; i > 0; i--) { snakeSegments[i] = snakeSegments[i - 1]; }
        snakeSegments[0] = nextHeadPosition;
        snakeVisuals.UpdateVisuals_Instant();
        faceController.UpdateFaceRotation(currentDirection);
        CheckForDeath();
    }

    private void CheckForDeath() { foreach (var segment in snakeSegments) { if (!levelManager.IsPitAt(segment)) { return; } } faceController.SetFace(FaceType.Fall); GameManager.Instance.LoseLevel(); }
    private void SaveState() { GameState currentState = new GameState { snakeSegmentPositions = new List<Vector2Int>(snakeSegments), snakeDirection = this.currentDirection, snakeLastTailPosition = this.lastTailPosition, foodStates = levelManager.GetCurrentFoodStates() }; history.Push(currentState); }
    private void LoadState(GameState state) { this.currentDirection = state.snakeDirection; this.lastTailPosition = state.snakeLastTailPosition; this.snakeSegments = new List<Vector2Int>(state.snakeSegmentPositions); this.droppedFoodCounter = 0; levelManager.LoadFoodState(state.foodStates); snakeVisuals.Initialize(this.snakeSegments, levelManager.grid); faceController.SetFace(FaceType.Normal); faceController.UpdateFaceRotation(currentDirection); }
    private void Undo() { if (history.Count > 0) { isAnimating = false; LoadState(history.Pop()); GameManager.Instance.StartLevel(); } }
    private Vector2Int GetVectorForDirection(Direction dir) { switch (dir) { case Direction.Up: return Vector2Int.up; case Direction.Down: return Vector2Int.down; case Direction.Left: return Vector2Int.left; case Direction.Right: return Vector2Int.right; default: return Vector2Int.zero; } }
}