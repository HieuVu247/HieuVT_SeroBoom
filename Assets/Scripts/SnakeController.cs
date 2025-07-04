using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Linq;
using DG.Tweening;
using UnityEngine.InputSystem;

public class SnakeController : MonoBehaviour
{
    private List<Vector2Int> snakeSegments;
    private Direction currentDirection;
    private SnakeVisuals snakeVisuals;
    private LevelManager _levelManager;
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

    public void Initialize(LevelData levelData, Grid grid, LevelManager manager)
    {
        this._levelManager = manager;
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

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!context.performed || isAnimating || GameManager.Instance.CurrentStatus != GameManager.GameStatus.Playing) return;

        Vector2 moveVector = context.ReadValue<Vector2>();

        if (Mathf.Abs(moveVector.x) > 0.5f)
        {
            if (moveVector.x > 0.5f && currentDirection != Direction.Left) AttemptMove(Direction.Right);
            else if (moveVector.x < -0.5f && currentDirection != Direction.Right) AttemptMove(Direction.Left);
        }
        else if (Mathf.Abs(moveVector.y) > 0.5f)
        {
            if (moveVector.y > 0.5f && currentDirection != Direction.Down) AttemptMove(Direction.Up);
            else if (moveVector.y < -0.5f && currentDirection != Direction.Up) AttemptMove(Direction.Down);
        }
    }

    public void OnUndo(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Undo();
    }

    public void OnRestart(InputAction.CallbackContext context)
    {
        if (!context.performed || _levelManager == null) return;
        _levelManager.GenerateLevel();
    }

    public void AttemptMove(Direction direction)
    {
        faceController.ResetToNormalFace();
        Vector2Int nextHeadPos = snakeSegments[0] + GetVectorForDirection(direction);
        for (int i = 1; i < snakeSegments.Count; i++) { if (snakeSegments[i] == nextHeadPos) return; }
        if (_levelManager.IsWormholeAt(nextHeadPos)) { GameManager.Instance.WinLevel(); return; }

        this.currentDirection = direction;
        FoodItem food = _levelManager.GetFoodAt(nextHeadPos);
        if (food != null)
        {
            SaveState();
            if (food.Push(GetVectorForDirection(currentDirection))) { MoveSnake(); }
            else { EatFood(food); }
        }
        else if (_levelManager.IsPositionWalkable(nextHeadPos))
        {
            SaveState();
            MoveSnake();
        }
    }

    public void Undo()
    {
        if (history.Count > 0)
        {
            isAnimating = false;
            LoadState(history.Pop());
            GameManager.Instance.StartLevel();
        }
    }
    
    private void EatFood(FoodItem food)
    {
        AudioManager.Instance.PlaySFX("SnakeEat");
        
        Direction moveDirection = this.currentDirection;
        _levelManager.RemoveFoodAt(food.gridPosition);
        if (food.foodType == FoodType.Banana) { Grow(); faceController.SetFace(FaceType.EatBanana); }

        food.Consume();
        MoveSnake();

        if (food.foodType == FoodType.RainbowPotion) { StartCoroutine(ApplyRainbowPushRoutine(moveDirection)); }
    }

    private IEnumerator ApplyRainbowPushRoutine(Direction moveDirection)
    {
        isAnimating = true;
        Vector2Int pushDirection = GetVectorForDirection(moveDirection) * -1;

        while (true)
        {
            bool canPushThisStep = true;
            HashSet<FoodItem> foodToPushThisStep = new HashSet<FoodItem>();

            foreach (var segment in snakeSegments)
            {
                Vector2Int nextPos = segment + pushDirection;
                if (_levelManager.IsWallAt(nextPos))
                {
                    canPushThisStep = false;
                    break;
                }
                FoodItem food = _levelManager.GetFoodAt(nextPos);
                if (food != null)
                {
                    foodToPushThisStep.Add(food);
                }
            }

            if (!canPushThisStep) { break; }

            foreach (var foodItem in foodToPushThisStep)
            {
                if (!foodItem.Push(pushDirection))
                {
                    canPushThisStep = false;
                    break;
                }
            }

            if (!canPushThisStep) { break; }

            lastTailPosition = snakeSegments.Last();
            for (int i = 0; i < snakeSegments.Count; i++)
            {
                snakeSegments[i] += pushDirection;
            }

            Sequence stepSequence = snakeVisuals.CreatePushAnimation();
            yield return stepSequence.WaitForCompletion();
        }

        snakeVisuals.UpdateVisuals_Instant();
        faceController.UpdateFaceRotation(this.currentDirection);
        CheckForDeath();
        isAnimating = false;
    }

    private void HandleFoodDropped() { droppedFoodCounter++; if (droppedFoodCounter == 1) { faceController.SetFace(FaceType.FailPush1); } else { faceController.SetFace(FaceType.FailPush2); } }
    private void Grow() { snakeSegments.Add(lastTailPosition); }

    private void MoveSnake()
    {
        AudioManager.Instance.PlaySFX("SnakeMove");
        
        if (snakeSegments.Count == 0) return;
        lastTailPosition = snakeSegments[snakeSegments.Count - 1];
        Vector2Int nextHeadPosition = snakeSegments[0] + GetVectorForDirection(currentDirection);
        for (int i = snakeSegments.Count - 1; i > 0; i--) { snakeSegments[i] = snakeSegments[i - 1]; }
        snakeSegments[0] = nextHeadPosition;
        snakeVisuals.UpdateVisuals_Instant();
        faceController.UpdateFaceRotation(currentDirection);
        CheckForDeath();
    }

    private void CheckForDeath() { foreach (var segment in snakeSegments) { if (!_levelManager.IsPitAt(segment)) { return; } } faceController.SetFace(FaceType.Fall); GameManager.Instance.LoseLevel(); }
    private void SaveState() { GameState currentState = new GameState { snakeSegmentPositions = new List<Vector2Int>(snakeSegments), snakeDirection = this.currentDirection, snakeLastTailPosition = this.lastTailPosition, foodStates = _levelManager.GetCurrentFoodStates() }; history.Push(currentState); }
    private void LoadState(GameState state) { this.currentDirection = state.snakeDirection; this.lastTailPosition = state.snakeLastTailPosition; this.snakeSegments = new List<Vector2Int>(state.snakeSegmentPositions); this.droppedFoodCounter = 0; _levelManager.LoadFoodState(state.foodStates); snakeVisuals.Initialize(this.snakeSegments, _levelManager.grid); faceController.SetFace(FaceType.Normal); faceController.UpdateFaceRotation(currentDirection); }
    private Vector2Int GetVectorForDirection(Direction dir) { switch (dir) { case Direction.Up: return Vector2Int.up; case Direction.Down: return Vector2Int.down; case Direction.Left: return Vector2Int.left; case Direction.Right: return Vector2Int.right; default: return Vector2Int.zero; } }
}