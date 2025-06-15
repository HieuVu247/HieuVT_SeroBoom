using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Checkerboard Rule Tile", menuName = "Puzzle Snake/Checkerboard Rule Tile")]
public class CheckerboardRuleTile : Tile
{
    [Tooltip("The first sprite for the checkerboard pattern (e.g., for even positions).")]
    public Sprite spriteA;
    [Tooltip("The second sprite for the checkerboard pattern (e.g., for odd positions).")]
    public Sprite spriteB;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        // Logic to choose sprite based on grid position
        if ((position.x + position.y) % 2 == 0)
        {
            tileData.sprite = this.spriteA;
        }
        else
        {
            tileData.sprite = this.spriteB;
        }
    }
}