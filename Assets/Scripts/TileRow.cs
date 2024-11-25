using UnityEngine;

public class TileRow : MonoBehaviour
{
    public TileCell[] cells { get; private set; }// 当前行所管辖的单元格
    private void Awake()
    {
        // 通过获取子节点中所有带有TileCell的对象
        cells = GetComponentsInChildren<TileCell>();
    }

}

