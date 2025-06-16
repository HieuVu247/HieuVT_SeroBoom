using UnityEngine;
public class Wormhole : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;
    
    private SpriteRenderer spriteRenderer;
    private bool isActive = false;
    private Vector2Int gridPosition;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2Int position, Grid grid)
    {
        this.gridPosition = position;
        transform.position = grid.GetCellCenterWorld((Vector3Int)position);
        spriteRenderer.sprite = closedSprite;
        spriteRenderer.sortingOrder = 0 - position.y;
        isActive = false;
    }

    public void Activate()
    {
        isActive = true;
        spriteRenderer.sprite = openSprite;
    }

    public bool IsAtPosition(Vector2Int position) { return this.gridPosition == position; }
    public bool IsActive() { return isActive; }
}