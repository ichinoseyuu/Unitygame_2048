using System.Collections.Generic;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    public TileRow[] rows { get; private set; } // 一共有多少行
    public TileCell[] cells { get; private set; }// 一共有多少个cell单元

    public int size => cells.Length;  // 存储单元的总数量
    public int height => rows.Length; // 网格的高 其实就是行数
    public int width => size / height; // 网格的宽 就是总的Cell数量除以高

    private void Awake()
    {
        // 在Awake中，获取所有行和单元的数量
        rows = GetComponentsInChildren<TileRow>();
        cells = GetComponentsInChildren<TileCell>();
    }

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
    /// 通过坐标获取单元格
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns></returns>
    public TileCell GetCell(Vector2Int coordinates)
    {
        return GetCell(coordinates.x, coordinates.y);
    }

    /// <summary>
    /// 通过坐标获取单元格
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
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
    /// 获取一个单元格，在某个方向上的邻居
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public TileCell GetAdjacentCell(TileCell cell, Vector2Int direction)
    {
        Vector2Int coordinates = cell.coordinates;
        coordinates.x += direction.x;
        coordinates.y -= direction.y;

        return GetCell(coordinates);
    }

    /// <summary>
    /// 获取一个随机的空的单元格
    /// </summary>
    /// <returns></returns>
    public TileCell GetRandomEmptyCell()
    {
        // 遍历所有可生成的Cell
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
