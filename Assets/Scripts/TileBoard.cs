using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    public GameManager gameManager; // 游戏管理器
    public Tile tilePrefab; // tile的预制
    public TileState[] tileStates;// 所有的tile状态配置

    private TileGrid grid;  // 网格
    private List<Tile> tiles; // 当前已经创建的Tile
    private bool waiting;   // 是否处于等待操作的状态，用来控制

    //触屏开始和结束的位置
    private Vector2 startPos;
    private Vector2 endPos;

    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>();
    }

    // 清除所有游戏状态
    public void ClearBoard()
    {
        // 清除所有Cell和Tile的链接关系
        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }
        // 销毁所有本局游戏中所创建的Tile
        foreach (var tile in tiles) 
        {
            Destroy(tile.gameObject); 
        }
        tiles.Clear();
    }
    
    // 随机生成一个Tile
    public void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);// 根据模板实例化一个Tile出来
        //tile.SetState(tileStates[0]); // 这里总是会生成2，其实可以根据一些规则设置为别的数字
        tile.SetState(Random.Range(0, 1f) > 0.1f ? tileStates[0] : tileStates[1]);//和其他2048类似，有几率会随机产生4
        tile.LinkCell(grid.GetRandomEmptyCell()); // 生成到一个随机的空的单元上
        tile.transform.DOScale(scale, 0.1f).OnComplete(() =>
        {
            tile.transform.DOScale(Vector3.one, 0.15f);
        });
        tiles.Add(tile);
    }

    private void Update()
    {
        if (waiting) return;
        // 判断当前平台是移动设备还是其他平台
        if (Application.platform == RuntimePlatform.Android)
        {
            HandleTouchInput();
        }
        else
        {
            HandleKeyboardInput();
        }
    }
    /// <summary>
    /// 移动端移动方法
    /// </summary>
    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            startPos = touch.position;
        }

        if (touch.phase == TouchPhase.Ended)
        {
            endPos = touch.position;

            Vector2 swipeDirection = endPos - startPos;

            if (swipeDirection.magnitude > 50) // 设置一个阈值来判断滑动距离是否足够大
            {
                //判断滑动方向
                if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
                {
                    if (swipeDirection.x > 0)
                    {
                        Move(Vector2Int.right, grid.width - 2, -1, 0, 1);
                    }
                    else
                    {
                        Move(Vector2Int.left, 1, 1, 0, 1);
                    }
                }
                else
                {
                    if (swipeDirection.y > 0)
                    {
                        Move(Vector2Int.up, 0, 1, 1, 1);
                    }
                    else
                    {
                        Move(Vector2Int.down, 0, 1, grid.height - 2, -1);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 按键移动方法
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {  
            Move(Vector2Int.up, 0, 1, 1, 1);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {  
            Move(Vector2Int.left, 1, 1, 0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {  
            Move(Vector2Int.down, 0, 1, grid.height - 2, -1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(Vector2Int.right, grid.width - 2, -1, 0, 1);
        }
    }

    /// <summary>
    /// 移动的方向 移动的初始X,Y和步长 用这些信息来决定哪些Cell应该移动
    /// </summary>
    /// <param name="direction">移动的方向</param>
    /// <param name="startX"></param>
    /// <param name="incrementX">每次循环的步长</param>
    /// <param name="startY"></param>
    /// <param name="incrementY">每次循环的步长</param>
    private void Move(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;

        for (int x = startX; x >= 0 && x < grid.width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);
                
                // 只有被占用的Cell才会需要移动
                if (cell.occupied) 
                {
                    changed |= MoveTile(cell.tile, direction);
                }
            }
        }

        if (changed) 
        {
            StartCoroutine(WaitForChanges());
        }
    }

    /// <summary>
    /// 朝指定的方向移动单元格
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        // 如果有相邻的Cell 
        while (adjacent != null)
        {
            if (adjacent.occupied) // 相邻的Cell里面有Tile 
            {
                // 如果正好他们俩可以合并，那么就执行合并，本次Tile的移动处理就结束了
                if (CanMerge(tile, adjacent.tile)) 
                {
                    MergeTiles(tile, adjacent.tile);
                    return true;
                }
                // 如果不能合并 也就到此结束了，什么都不用做 
                break;
            }

            // 执行到这里，说明他相邻的格子为空，那么就把当前相邻的格子作为一个目标，再去判断相邻格子的相邻格子情况
            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(adjacent, direction);
        }
        
        // 执行完之后就知道是否有最终的新格子，将Tile移动到最终的格子上去
        if (newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 判定两个单元格是否能合并
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private bool CanMerge(Tile a, Tile b)
    {
        // 状态相等，并且b不在动画锁定状态
        return a.state == b.state && !b.locked;
    }

    /// <summary>
    /// 将A单元格合并到B上
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    private Vector3 scale = new Vector3(1.2f, 1.2f, 1);
    private void MergeTiles(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.Merge(b.cell);
        SoundManager.Instance.playSound(SoundManager.Instance.mergeClip);
        // 找到合并之后的新状态
        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        TileState newState = tileStates[index];
        b.transform.DOScale(scale, 0.1f)
                .OnComplete(() => b.transform.DOScale(Vector3.one, 0.15f)
                .OnComplete(() => b.SetState(newState)));
        // 增加分数
        gameManager.IncreaseScore(newState.number);
    }

    // 返回某个state的模板序号
    private int IndexOf(TileState state)
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (state == tileStates[i]) {
                return i;
            }
        }

        return -1;
    }

    private IEnumerator WaitForChanges()
    {
        // 两次操作之间 间隔0.1秒，用于做动画表现
        
        waiting = true;

        yield return new WaitForSeconds(0.15f);

        waiting = false;

        foreach (var tile in tiles) {
            tile.locked = false;
        }

        if (tiles.Count != grid.size) {
            CreateTile();
        }

        if (CheckForGameOver()) {
            gameManager.GameOver();
        }
    }

    /// <summary>
    /// 检查游戏是否结束
    /// </summary>
    /// <returns></returns>
    public bool CheckForGameOver()
    {
        // tile数量不够单元格
        if (tiles.Count != grid.size) return false;

        // 通过循环判定某个单元格的四个方向，任意一个方向可以合并，游戏都未结束
        foreach (var tile in tiles)
        {
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

            if (up != null && CanMerge(tile, up.tile)) {
                return false;
            }

            if (down != null && CanMerge(tile, down.tile)) {
                return false;
            }

            if (left != null && CanMerge(tile, left.tile)) {
                return false;
            }

            if (right != null && CanMerge(tile, right.tile)) {
                return false;
            }
        }

        return true;
    }

}
