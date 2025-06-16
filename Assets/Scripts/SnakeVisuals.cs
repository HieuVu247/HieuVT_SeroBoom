using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class SnakeVisuals : MonoBehaviour
{
    [System.Serializable]
    public class SnakeSpriteSet
    {
        [Header("Head Sprites")]
        public Sprite HeadUp;
        public Sprite HeadDown;
        public Sprite HeadLeft;
        public Sprite HeadRight;

        [Header("Tail Sprites")]
        public Sprite TailUp;
        public Sprite TailDown;
        public Sprite TailLeft;
        public Sprite TailRight;

        [Header("Body Sprites")]
        public Sprite BodyVertical;
        public Sprite BodyHorizontal;
        public Sprite BodyCornerTopRight;
        public Sprite BodyCornerTopLeft;
        public Sprite BodyCornerBottomRight;
        public Sprite BodyCornerBottomLeft;
    }

    [Header("Asset References")]
    [Tooltip("The Prefab for a single snake segment GameObject.")]
    public GameObject segmentPrefab;
    [Tooltip("The collection of all required sprites for the snake.")]
    public SnakeSpriteSet snakeSprites;

    private List<GameObject> snakeSegmentObjects = new List<GameObject>();
    private List<Vector2Int> segmentPositions;
    private Grid grid;
    private Transform visualContainer;

    public void Initialize(List<Vector2Int> initialSegmentPositions, Grid targetGrid)
    {
        this.segmentPositions = initialSegmentPositions;
        this.grid = targetGrid;

        if (visualContainer == null)
        {
            visualContainer = new GameObject("SnakeVisualContainer").transform;
            visualContainer.SetParent(targetGrid.transform);
        }

        foreach (var segment in snakeSegmentObjects)
        {
            Destroy(segment);
        }
        snakeSegmentObjects.Clear();
        
        for (int i = 0; i < segmentPositions.Count; i++)
        {
            CreateNewSegment();
        }

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        SyncSegmentCount();
        for (int i = 0; i < snakeSegmentObjects.Count; i++)
        {
            UpdateSegment(i);
        }
    }

    private void SyncSegmentCount()
    {
        while (snakeSegmentObjects.Count < segmentPositions.Count)
        {
            CreateNewSegment();
        }
        while (snakeSegmentObjects.Count > segmentPositions.Count)
        {
            Destroy(snakeSegmentObjects[snakeSegmentObjects.Count - 1]);
            snakeSegmentObjects.RemoveAt(snakeSegmentObjects.Count - 1);
        }
    }

    private void CreateNewSegment()
    {
        GameObject newSegment = Instantiate(segmentPrefab, visualContainer);
        snakeSegmentObjects.Add(newSegment);
    }
    
    private void UpdateSegment(int index)
    {
        GameObject segmentObject = snakeSegmentObjects[index];
        Vector2Int currentPos = segmentPositions[index];
        
        segmentObject.transform.position = grid.GetCellCenterWorld((Vector3Int)currentPos);

        SpriteRenderer sr = segmentObject.GetComponent<SpriteRenderer>();
        
        // THAY ĐỔI LOGIC SORTING Ở ĐÂY
        sr.sortingOrder = -currentPos.y * 10 + 5;

        if (index == 0)
        {
            Vector2Int nextPos = segmentPositions[index + 1];
            Vector2Int direction = currentPos - nextPos;
            sr.sprite = GetHeadSprite(direction);
        }
        else if (index == segmentPositions.Count - 1)
        {
            Vector2Int prevPos = segmentPositions[index - 1];
            Vector2Int direction = currentPos - prevPos;
            sr.sprite = GetTailSprite(direction);
        }
        else
        {
            Vector2Int prevPos = segmentPositions[index - 1];
            Vector2Int nextPos = segmentPositions[index + 1];
            sr.sprite = GetBodySprite(prevPos, currentPos, nextPos);
        }
    }
    
    private Sprite GetHeadSprite(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return snakeSprites.HeadUp;
        if (direction == Vector2Int.down) return snakeSprites.HeadDown;
        if (direction == Vector2Int.left) return snakeSprites.HeadLeft;
        if (direction == Vector2Int.right) return snakeSprites.HeadRight;
        return null;
    }

    private Sprite GetTailSprite(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return snakeSprites.TailDown;
        if (direction == Vector2Int.down) return snakeSprites.TailUp;
        if (direction == Vector2Int.left) return snakeSprites.TailRight;
        if (direction == Vector2Int.right) return snakeSprites.TailLeft;
        return null;
    }

    private Sprite GetBodySprite(Vector2Int prevPos, Vector2Int currentPos, Vector2Int nextPos)
    {
        Vector2Int prevDir = currentPos - prevPos;
        Vector2Int nextDir = nextPos - currentPos;
        
        Vector2Int up = Vector2Int.up;
        Vector2Int down = Vector2Int.down;
        Vector2Int left = Vector2Int.left;
        Vector2Int right = Vector2Int.right;

        if ((prevDir == up && nextDir == up) || (prevDir == down && nextDir == down)) return snakeSprites.BodyVertical;
        if ((prevDir == right && nextDir == right) || (prevDir == left && nextDir == left)) return snakeSprites.BodyHorizontal;

        if ((prevDir == right && nextDir == up) || (prevDir == down && nextDir == left)) return snakeSprites.BodyCornerBottomRight;
        if ((prevDir == left && nextDir == up) || (prevDir == down && nextDir == right)) return snakeSprites.BodyCornerBottomLeft;
        if ((prevDir == up && nextDir == right) || (prevDir == left && nextDir == down)) return snakeSprites.BodyCornerTopLeft;
        if ((prevDir == right && nextDir == down) || (prevDir == up && nextDir == left)) return snakeSprites.BodyCornerTopRight;
        
        return null;
    }
}