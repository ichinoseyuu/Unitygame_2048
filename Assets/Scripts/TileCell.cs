using UnityEngine;

public class TileCell : MonoBehaviour
{
    public Vector2Int coordinates { get; set; } // 坐标
    public Tile tile { get; set; } // 关联的tile

    public bool empty => tile == null; // 是否为空
    public bool occupied => tile != null; // 是否被占用 
}
