using UnityEngine;

public enum FaceType
{
    Normal,
    EatBanana,
    FailPush1,
    FailPush2,
    Fall
}

public class SnakeFaceController : MonoBehaviour
{
    [System.Serializable]
    public class FaceSprites
    {
        public Sprite Normal;
        public Sprite EatBanana;
        public Sprite FailPush1;
        public Sprite FailPush2;
        public Sprite Fall;
    }

    [Header("Face Sprite Assets")]
    public FaceSprites faceSpriteSet;

    private SpriteRenderer faceRenderer;
    private Transform faceTransform;
    private bool isSpecialFaceActive = false;

    public void InitializeFace(Transform headSegment)
    {
        faceTransform = headSegment.Find("Face");
        if(faceTransform != null)
        {
            faceRenderer = faceTransform.GetComponent<SpriteRenderer>();
        }
    }

    public void SetFace(FaceType faceType)
    {
        if (faceRenderer == null) return;

        isSpecialFaceActive = (faceType != FaceType.Normal);

        switch (faceType)
        {
            case FaceType.Normal:
                faceRenderer.sprite = faceSpriteSet.Normal;
                break;
            case FaceType.EatBanana:
                faceRenderer.sprite = faceSpriteSet.EatBanana;
                break;
            case FaceType.FailPush1:
                faceRenderer.sprite = faceSpriteSet.FailPush1;
                break;
            case FaceType.FailPush2:
                faceRenderer.sprite = faceSpriteSet.FailPush2;
                break;
            case FaceType.Fall:
                faceRenderer.sprite = faceSpriteSet.Fall;
                break;
        }
    }

    public void UpdateFaceRotation(Direction direction)
    {
        if (faceTransform == null) return;
        
        float angle = 0f;
        switch (direction)
        {
            case Direction.Up: angle = 180; break;
            case Direction.Down: angle = 0; break;
            case Direction.Left: angle = -90; break;
            case Direction.Right: angle = 90; break;
        }
        faceTransform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    public void ResetToNormalFace()
    {
        if (isSpecialFaceActive)
        {
            SetFace(FaceType.Normal);
        }
    }
}