using UnityEngine;

[CreateAssetMenu(menuName = "Tile State")]
public class TileState : ScriptableObject
{
    /// <summary>获取或设置此方块状态的背景颜色。</summary>
    public Color backgroundColor; // 背景颜色
    /// <summary>获取或设置此方块状态的数字文字颜色。</summary>
    public Color textColor; // 文字颜色
    /// <summary>获取或设置此方块状态代表的数值。</summary>
    public int number; // 数字
}
