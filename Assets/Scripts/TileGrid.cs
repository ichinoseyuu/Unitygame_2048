using System.Collections.Generic;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    /// <summary>获取此棋盘中的所有行。</summary>
    public TileRow[] rows { get; private set; } // 一共有多少行
    /// <summary>获取棋盘中按层级顺序排列的所有单元格。</summary>
    public TileCell[] cells { get; private set; }// 一共有多少个cell单元

    /// <summary>获取网格中的单元格总数。</summary>
    public int size => cells.Length;  // 存储单元的总数量
    /// <summary>获取网格的行数。</summary>
    public int height => rows.Length; // 网格的高 其实就是行数
    /// <summary>获取网格的列数。</summary>
    public int width => size / height; // 网格的宽 就是总的Cell数量除以高

    /// <summary>
    /// 从子对象中收集棋盘的行和单元格。
    /// </summary>
    private void Awake()
    {
        // 在Awake中，获取所有行和单元的数量
        rows = GetComponentsInChildren<TileRow>();
        cells = GetComponentsInChildren<TileCell>();
    }

    /// <summary>
    /// 为每个单元格设置对应的网格坐标。
    /// </summary>
    private void Start()
    {
        // 给所有的Cell进行编号，设置坐标
        for (int i = 0; i < rows.Length; i++)
        {
            for (int j = 0; j < rows[i].cells.Length; j++)
            {
                rows[i].cells[j].coordinates = new Vector2Int(j, i); // 注意 这里是反的 j,i
            }
        }
    }

    /// <summary>
    /// 根据二维坐标获取对应的单元格。
    /// </summary>
    /// <param name="coordinates">单元格的列和行坐标。</param>
    /// <returns>坐标范围内的单元格；坐标越界时返回 <see langword="null"/>。</returns>
    public TileCell GetCell(Vector2Int coordinates)
    {
        return GetCell(coordinates.x, coordinates.y);
    }

    /// <summary>
    /// 根据列和行坐标获取对应的单元格。
    /// </summary>
    /// <param name="x">单元格的列索引。</param>
    /// <param name="y">单元格的行索引。</param>
    /// <returns>坐标范围内的单元格；坐标越界时返回 <see langword="null"/>。</returns>
    public TileCell GetCell(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height) {
            return rows[y].cells[x];
        } 
        else 
        {
            return null;
        }
    }

    /// <summary>
    /// 获取指定单元格沿某个方向的相邻单元格。
    /// </summary>
    /// <param name="cell">要查询邻居的单元格。</param>
    /// <param name="direction">查询方向。</param>
    /// <returns>相邻单元格；若邻居超出棋盘边界则返回 <see langword="null"/>。</returns>
    public TileCell GetAdjacentCell(TileCell cell, Vector2Int direction)
    {
        // 网格行坐标向下递增，因此纵向方向需要反转。
        Vector2Int coordinates = cell.coordinates;
        coordinates.x += direction.x;
        coordinates.y -= direction.y;

        return GetCell(coordinates);
    }

    /// <summary>
    /// 从当前所有空单元格中随机选取一个。
    /// </summary>
    /// <returns>随机空单元格；棋盘已满时返回 <see langword="null"/>。</returns>
    public TileCell GetRandomEmptyCell()
    {
        // 先收集空格，再从候选列表中均匀随机选取。
        List<TileCell> emptyCells = new List<TileCell>();
        foreach (var cell in cells)
        {
            if (cell.empty)
            {
                emptyCells.Add(cell);
            }
        }

        if (emptyCells.Count < 1)
        {
            return null;
        }

        return emptyCells[Random.Range(0, emptyCells.Count)];
        
        
        // 原来的随机算法，会导致一个问题 当随机到的某个值往上都被占用的时候，总是会找到第一个空格
        // 而当随机到的某个值往上有空格的时候，总是会找到其上面第一个空位格子
        // 中后期空格比较少的时候，总是会随机到同一个格子上
        // int index = Random.Range(0, cells.Length);
        // int startingIndex = index;
        //
        // while (cells[index].occupied)
        // {
        //     index++;
        //
        //     if (index >= cells.Length) {
        //         index = 0;
        //     }
        //
        //     // all cells are occupied
        //     if (index == startingIndex) {
        //         return null;
        //     }
        // }
        //
        // return cells[index];
    }

}
