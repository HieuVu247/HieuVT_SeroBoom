using UnityEngine;
using DG.Tweening;

public class FoodItem : MonoBehaviour
{
    public Vector2Int gridPosition;
    public FoodType foodType;
    
    private Grid grid;
    private LevelManager levelManager;

    public void Initialize(Vector2Int startPosition, FoodType type, Grid grid, LevelManager manager)
    {
        this.gridPosition = startPosition;
        this.foodType = type;
        this.grid = grid;
        this.levelManager = manager;
        UpdateWorldPosition_Instant();
        UpdateSortingOrder();
    }

    public bool Push(Vector2Int direction)
    {
        Vector2Int nextPosition = gridPosition + direction;

        if (levelManager.IsPitAt(nextPosition))
        {
            GameEvents.TriggerFoodDroppedInPit();
            
            levelManager.RemoveFoodAt(this.gridPosition);
            Consume(); 
            return true; 
        }

        if (levelManager.IsWallAt(nextPosition))
        {
            return false;
        }

        FoodItem foodInNextPos = levelManager.GetFoodAt(nextPosition);
        if (foodInNextPos != null)
        {
            bool canNextFoodBePushed = foodInNextPos.Push(direction);
            if (!canNextFoodBePushed)
            {
                return false; 
            }
        }
        
        levelManager.UpdateFoodPosition(gridPosition, nextPosition);
        gridPosition = nextPosition;
        UpdateWorldPosition_Instant();
        UpdateSortingOrder();
        return true;
    }

    public void Consume() { Destroy(gameObject); }
    public void UpdateWorldPosition_Instant() { transform.position = grid.GetCellCenterWorld((Vector3Int)gridPosition); }
    private void UpdateSortingOrder() { if (TryGetComponent<SpriteRenderer>(out var sr)) { sr.sortingOrder = 20 - gridPosition.y; } }
    
    public Tween GetMoveTween(Vector2Int newPosition, float duration)
    {
        return transform.DOMove(grid.GetCellCenterWorld((Vector3Int)newPosition), duration);
    }
}