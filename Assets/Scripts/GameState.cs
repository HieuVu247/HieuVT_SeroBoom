using System.Collections.Generic;
using UnityEngine;

// Dùng struct để lưu trạng thái của một món ăn
[System.Serializable]
public struct FoodState
{
    public FoodType foodType;
    public Vector2Int position;
}

// Dùng class để lưu trạng thái của toàn bộ màn chơi tại một thời điểm
public class GameState
{
    public List<Vector2Int> snakeSegmentPositions;
    public Direction snakeDirection;
    public Vector2Int snakeLastTailPosition;
    public List<FoodState> foodStates;
}