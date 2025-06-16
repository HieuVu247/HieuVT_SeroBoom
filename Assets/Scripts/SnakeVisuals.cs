using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using DG.Tweening;
using System;

public class SnakeVisuals : MonoBehaviour
{
    [System.Serializable]
    public class SnakeSpriteSet {
        public Sprite HeadUp, HeadDown, HeadLeft, HeadRight;
        public Sprite TailUp, TailDown, TailLeft, TailRight;
        public Sprite BodyVertical, BodyHorizontal, BodyCornerTopRight, BodyCornerTopLeft, BodyCornerBottomRight, BodyCornerBottomLeft;
    }

    [Header("Asset References")]
    public GameObject segmentPrefab;
    public SnakeSpriteSet snakeSprites;
    [Header("Animation Settings")]
    public float moveDuration = 0.3f;
    public float segmentMoveDelay = 0.025f;

    private List<GameObject> snakeSegmentObjects = new List<GameObject>();
    private List<Vector2Int> segmentPositions;
    private Grid grid;
    private Transform visualContainer;
    private SnakeFaceController faceController;

    void Awake() { faceController = GetComponent<SnakeFaceController>(); }

    public void Initialize(List<Vector2Int> initialSegmentPositions, Grid targetGrid)
    {
        this.segmentPositions = initialSegmentPositions;
        this.grid = targetGrid;
        if (visualContainer == null) { visualContainer = new GameObject("SnakeVisualContainer").transform; visualContainer.SetParent(this.transform); }
        foreach (var segment in snakeSegmentObjects) { Destroy(segment); }
        snakeSegmentObjects.Clear();
        UpdateVisuals_Instant();
    }

    public void UpdateVisuals_Instant()
    {
        SyncSegmentCount();
        for (int i = 0; i < snakeSegmentObjects.Count; i++)
        {
            UpdateSegmentSprites(i);
            GameObject segmentObject = snakeSegmentObjects[i];
            Vector2Int currentPos = segmentPositions[i];
            segmentObject.transform.position = grid.GetCellCenterWorld((Vector3Int)currentPos);
        }
    }

    public Sequence CreatePushAnimation()
    {
        SyncSegmentCount();
        Sequence moveSequence = DOTween.Sequence();
        for (int i = 0; i < snakeSegmentObjects.Count; i++)
        {
            GameObject segmentObject = snakeSegmentObjects[i];
            Vector2Int currentPos = segmentPositions[i];
            float startTime = (snakeSegmentObjects.Count - 1 - i) * segmentMoveDelay;
            Tweener moveTween = segmentObject.transform.DOMove(grid.GetCellCenterWorld((Vector3Int)currentPos), moveDuration);
            moveSequence.Insert(startTime, moveTween);
        }
        return moveSequence;
    }

    private void UpdateSegmentSprites(int index)
    {
        if (index >= snakeSegmentObjects.Count || index < 0) return;
        GameObject segmentObject = snakeSegmentObjects[index];
        Vector2Int currentPos = segmentPositions[index];
        Transform faceTransform = segmentObject.transform.Find("Face");
        if (faceTransform != null)
        {
            faceTransform.gameObject.SetActive(index == 0);
            if(index == 0) { faceController.InitializeFace(segmentObject.transform); }
        }
        SpriteRenderer sr = segmentObject.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 20 - currentPos.y;
        if (index == 0) {
            Vector2Int nextPos = segmentPositions[index + 1];
            sr.sprite = GetHeadSprite(currentPos - nextPos);
        } else if (index == segmentPositions.Count - 1) {
            Vector2Int prevPos = segmentPositions[index - 1];
            sr.sprite = GetTailSprite(currentPos - prevPos);
        } else {
            Vector2Int prevPos = segmentPositions[index - 1];
            Vector2Int nextPos = segmentPositions[index + 1];
            sr.sprite = GetBodySprite(prevPos, currentPos, nextPos);
        }
    }

    private void SyncSegmentCount()
    {
        while (snakeSegmentObjects.Count < segmentPositions.Count) { CreateNewSegment(); }
        while (snakeSegmentObjects.Count > segmentPositions.Count) { Destroy(snakeSegmentObjects[snakeSegmentObjects.Count - 1]); snakeSegmentObjects.RemoveAt(snakeSegmentObjects.Count - 1); }
    }

    private void CreateNewSegment() { GameObject newSegment = Instantiate(segmentPrefab, visualContainer); snakeSegmentObjects.Add(newSegment); }
    private Sprite GetHeadSprite(Vector2Int direction) { if (direction == Vector2Int.up) return snakeSprites.HeadUp; if (direction == Vector2Int.down) return snakeSprites.HeadDown; if (direction == Vector2Int.left) return snakeSprites.HeadLeft; if (direction == Vector2Int.right) return snakeSprites.HeadRight; return null; }
    private Sprite GetTailSprite(Vector2Int direction) { if (direction == Vector2Int.up) return snakeSprites.TailDown; if (direction == Vector2Int.down) return snakeSprites.TailUp; if (direction == Vector2Int.left) return snakeSprites.TailRight; if (direction == Vector2Int.right) return snakeSprites.TailLeft; return null; }
    private Sprite GetBodySprite(Vector2Int prevPos, Vector2Int currentPos, Vector2Int nextPos)
    {
        Vector2Int prevDir = currentPos - prevPos; Vector2Int nextDir = nextPos - currentPos;
        Vector2Int up = Vector2Int.up; Vector2Int down = Vector2Int.down; Vector2Int left = Vector2Int.left; Vector2Int right = Vector2Int.right;
        if ((prevDir == up && nextDir == up) || (prevDir == down && nextDir == down)) return snakeSprites.BodyVertical;
        if ((prevDir == right && nextDir == right) || (prevDir == left && nextDir == left)) return snakeSprites.BodyHorizontal;
        if ((prevDir == right && nextDir == up) || (prevDir == down && nextDir == left)) return snakeSprites.BodyCornerBottomRight;
        if ((prevDir == left && nextDir == up) || (prevDir == down && nextDir == right)) return snakeSprites.BodyCornerBottomLeft;
        if ((prevDir == up && nextDir == right) || (prevDir == left && nextDir == down)) return snakeSprites.BodyCornerTopLeft;
        if ((prevDir == right && nextDir == down) || (prevDir == up && nextDir == left)) return snakeSprites.BodyCornerTopRight;
        return null;
    }
}