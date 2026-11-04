using UnityEngine;

public class TileRow : MonoBehaviour
{
    /// <summary>获取此行包含的所有单元格。</summary>
    public TileCell[] cells { get; private set; }// 当前行所管辖的单元格

    /// <summary>
    /// 收集此行子对象中的所有单元格。
    /// </summary>
    private void Awake()
    {
        // 通过获取子节点中所有带有TileCell的对象
        cells = GetComponentsInChildren<TileCell>();
    }

}
