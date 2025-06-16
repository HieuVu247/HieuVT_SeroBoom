using UnityEngine;

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
        UpdateWorldPosition();
        UpdateSortingOrder();
    }

    public bool Push(Vector2Int direction)
    {
        Vector2Int nextPosition = gridPosition + direction;

        if (levelManager.IsWallAt(nextPosition))
        {
            return false; // Bị chặn bởi tường
        }

        FoodItem foodInNextPos = levelManager.GetFoodAt(nextPosition);
        if (foodInNextPos != null)
        {
            // Có một món ăn khác ở phía trước, thử đẩy nó đi
            bool nextFoodPushed = foodInNextPos.Push(direction);
            if (!nextFoodPushed)
            {
                return false; // Món ăn phía trước không di chuyển được, vậy mình cũng không di chuyển
            }
        }
        
        // Vị trí phía trước đã trống (hoặc đã được dọn dẹp), di chuyển chính mình
        levelManager.UpdateFoodPosition(gridPosition, nextPosition);
        gridPosition = nextPosition;
        UpdateWorldPosition();
        UpdateSortingOrder();
        return true;
    }

    public void Consume()
    {
        Destroy(gameObject);
    }

    private void UpdateWorldPosition()
    {
        transform.position = grid.GetCellCenterWorld((Vector3Int)gridPosition);
    }

    private void UpdateSortingOrder()
    {
        if (TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.sortingOrder = -gridPosition.y * 10 + 5;
        }
    }
}