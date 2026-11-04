using UnityEngine;

public class TileCell : MonoBehaviour
{
    /// <summary>获取或设置单元格在棋盘网格中的坐标。</summary>
    public Vector2Int coordinates { get; set; } // 坐标
    /// <summary>获取或设置占据此单元格的方块。</summary>
    public Tile tile { get; set; } // 关联的tile

    /// <summary>指示此单元格当前是否为空。</summary>
    public bool empty => tile == null; // 是否为空
    /// <summary>指示此单元格当前是否被方块占据。</summary>
    public bool occupied => tile != null; // 是否被占用 
}
