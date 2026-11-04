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
    private readonly Stack<BoardSnapshot> undoHistory = new Stack<BoardSnapshot>();
    private DG.Tweening.Sequence undoTween;

    private sealed class BoardSnapshot
    {
        public TileState[] states;
        public int[] tileIds;
        public int score;
        public readonly List<MergeRecord> merges = new List<MergeRecord>();
    }

    private sealed class MergeRecord
    {
        public int sourceIndex;
        public int targetIndex;
    }

    //触屏开始和结束的位置
    private Vector2 startPos;
    private Vector2 endPos;

    /// <summary>
    /// 获取棋盘网格组件并初始化方块集合。
    /// </summary>
    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>();
    }

    /// <summary>
    /// 清空移动协程、撤销历史、单元格关联和当前所有方块。
    /// </summary>
    public void ClearBoard()
    {
        // 先停止动画和异步操作，避免清理期间继续修改棋盘。
        StopAllCoroutines();
        undoTween?.Kill();
        undoTween = null;
        waiting = false;
        undoHistory.Clear();

        // 清除所有单元格的占用状态，再销毁场景中的方块对象。
        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }
        // 销毁所有本局游戏中所创建的Tile
        foreach (var tile in tiles)
        {
            tile.transform.DOKill();
            Destroy(tile.gameObject);
        }
        tiles.Clear();
    }

    /// <summary>
    /// 在随机空单元格中生成一个新方块，并播放出现动画。
    /// </summary>
    public void CreateTile()
    {
        TileCell cell = grid.GetRandomEmptyCell();
        if (cell == null) return;

        // 从预制体创建方块，并随机指定初始状态。
        Tile tile = Instantiate(tilePrefab, grid.transform);
        //tile.SetState(tileStates[0]); // 这里总是会生成2，其实可以根据一些规则设置为别的数字
        tile.SetState(Random.Range(0, 1f) > 0.1f ? tileStates[0] : tileStates[1]);//和其他2048类似，有几率会随机产生4
        tile.LinkCell(cell); // 生成到一个随机的空的单元上
        tile.transform.DOScale(scale, 0.1f).OnComplete(() =>
        {
            tile.transform.DOScale(Vector3.one, 0.15f);
        });
        tiles.Add(tile);
    }

    /// <summary>
    /// 在棋盘可操作时，根据当前平台处理触控或键盘输入。
    /// </summary>
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
    /// 读取触控滑动方向，并将有效滑动转换为棋盘移动。
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

            // 忽略短距离触控，避免误触发移动。
            if (swipeDirection.magnitude > 50)
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
    /// 将方向键或 WASD 输入转换为棋盘移动。
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
    /// 按指定遍历方向移动方块，并为有效移动保存撤销快照。
    /// </summary>
    /// <param name="direction">方块移动的方向。</param>
    /// <param name="startX">开始遍历的列索引。</param>
    /// <param name="incrementX">每次遍历列索引的步长。</param>
    /// <param name="startY">开始遍历的行索引。</param>
    /// <param name="incrementY">每次遍历行索引的步长。</param>
    private void Move(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        // 移动前保存棋盘和分数状态，仅当局面实际变化时才保留该快照。
        BoardSnapshot snapshot = CaptureSnapshot();
        bool changed = false;

        for (int x = startX; x >= 0 && x < grid.width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);

                // 只有被占用的Cell才会需要移动
                if (cell.occupied)
                {
                    changed |= MoveTile(cell.tile, direction, snapshot);
                }
            }
        }

        if (changed)
        {
            undoHistory.Push(snapshot);
            StartCoroutine(WaitForChanges());
        }
    }

    /// <summary>
    /// 沿指定方向移动方块，直到遇到边界、障碍或可合并方块。
    /// </summary>
    /// <param name="tile">要移动的方块。</param>
    /// <param name="direction">方块移动的方向。</param>
    /// <param name="snapshot">本次移动前保存的棋盘快照，用于记录合并信息。</param>
    /// <returns>方块位置发生变化或发生合并时返回 <see langword="true"/>。</returns>
    private bool MoveTile(Tile tile, Vector2Int direction, BoardSnapshot snapshot)
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
                    MergeTiles(tile, adjacent.tile, snapshot);
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
    /// 判断两个方块是否具有相同状态，且目标方块尚未在本回合合并。
    /// </summary>
    /// <param name="a">尝试移动或合并的方块。</param>
    /// <param name="b">目标位置上的方块。</param>
    /// <returns>两个方块可以合并时返回 <see langword="true"/>。</returns>
    private bool CanMerge(Tile a, Tile b)
    {
        return a != null && b != null && a.state == b.state && !b.locked;
    }

    /// <summary>
    /// 恢复最近一次有效移动前的棋盘状态，并播放移动和分离动画。
    /// </summary>
    /// <returns>成功开始撤销时返回 <see langword="true"/>；正在处理操作或没有历史记录时返回 <see langword="false"/>。</returns>
    public bool Undo()
    {
        if (waiting || undoHistory.Count == 0) return false;

        // 取出最近一次移动快照，并锁定输入直到还原动画完成。
        BoardSnapshot snapshot = undoHistory.Pop();
        waiting = true;

        HashSet<int> mergedSources = new HashSet<int>();
        Dictionary<int, int> mergeTargets = new Dictionary<int, int>();
        foreach (MergeRecord merge in snapshot.merges)
        {
            mergedSources.Add(merge.sourceIndex);
            mergeTargets[merge.sourceIndex] = merge.targetIndex;
        }

        // 清空单元格占用后，再按快照重新建立方块关联。
        foreach (TileCell cell in grid.cells)
        {
            cell.tile = null;
        }

        foreach (Tile tile in tiles)
        {
            if (tile != null) tile.UnlinkCell();
        }

        undoTween = DOTween.Sequence();
        const float undoDuration = 0.2f;

        // 撤销时将本回合新生成的方块缩小并移除。
        HashSet<int> previousTileIds = new HashSet<int>(snapshot.tileIds);
        for (int i = 0; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            if (tile == null) continue;

            if (!previousTileIds.Contains(tile.Identity))
            {
                tile.transform.DOKill();
                undoTween.Join(tile.transform.DOScale(Vector3.zero, undoDuration)
                    .OnComplete(() =>
                    {
                        tiles.Remove(tile);
                        Destroy(tile.gameObject);
                    }));
            }
        }

        // 恢复原有方块位置；合并产生的方块从合并目标处动画分离。
        for (int i = 0; i < snapshot.states.Length; i++)
        {
            TileState state = snapshot.states[i];
            if (state == null) continue;

            if (mergedSources.Contains(i))
            {
                int targetIndex = mergeTargets[i];
                Tile mergeTarget = FindTile(snapshot.tileIds[targetIndex]);
                Vector3 startPosition = mergeTarget != null
                    ? mergeTarget.transform.position
                    : grid.cells[targetIndex].transform.position;

                Tile splitTile = Instantiate(tilePrefab, grid.transform);
                splitTile.SetState(state);
                splitTile.SetIdentity(snapshot.tileIds[i]);
                splitTile.transform.position = startPosition;
                splitTile.transform.localScale = Vector3.zero;
                tiles.Add(splitTile);
                undoTween.Join(splitTile.AnimateToCell(grid.cells[i], undoDuration));
                undoTween.Join(splitTile.transform.DOScale(Vector3.one, undoDuration));
                continue;
            }

            Tile tile = FindTile(snapshot.tileIds[i]);
            if (tile == null)
            {
                tile = Instantiate(tilePrefab, grid.transform);
                tile.SetIdentity(snapshot.tileIds[i]);
                tile.transform.position = grid.cells[i].transform.position;
                tile.transform.localScale = Vector3.zero;
                tiles.Add(tile);
            }

            tile.SetState(state);
            undoTween.Join(tile.AnimateToCell(grid.cells[i], undoDuration));
            undoTween.Join(tile.transform.DOScale(Vector3.one, undoDuration));
        }

        gameManager.RestoreScore(snapshot.score);
        undoTween.OnComplete(() =>
        {
            foreach (Tile tile in tiles)
            {
                if (tile != null) tile.locked = false;
            }

            waiting = false;
            undoTween = null;
        });
        return true;
    }

    /// <summary>
    /// 记录当前每个单元格中的方块状态、身份标识和游戏分数。
    /// </summary>
    /// <returns>可用于撤销的当前棋盘快照。</returns>
    private BoardSnapshot CaptureSnapshot()
    {
        BoardSnapshot snapshot = new BoardSnapshot
        {
            states = new TileState[grid.cells.Length],
            tileIds = new int[grid.cells.Length],
            score = gameManager.Score
        };

        for (int i = 0; i < grid.cells.Length; i++)
        {
            if (grid.cells[i].occupied)
            {
                snapshot.states[i] = grid.cells[i].tile.state;
                snapshot.tileIds[i] = grid.cells[i].tile.Identity;
            }
        }

        return snapshot;
    }

    /// <summary>合并动画使用的缩放倍率。</summary>
    private Vector3 scale = new Vector3(1.2f, 1.2f, 1);

    /// <summary>
    /// 将源方块合并到目标方块，更新合并状态、动画和分数。
    /// </summary>
    /// <param name="a">被合并并移除的源方块。</param>
    /// <param name="b">保留并升级状态的目标方块。</param>
    /// <param name="snapshot">用于记录此次合并关系的移动快照。</param>
    private void MergeTiles(Tile a, Tile b, BoardSnapshot snapshot)
    {
        // 保存合并前的两个方块位置关系，供撤销时还原分离。
        snapshot.merges.Add(new MergeRecord
        {
            sourceIndex = FindTileIndex(snapshot, a),
            targetIndex = FindTileIndex(snapshot, b)
        });

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

    /// <summary>
    /// 查找方块在给定快照中对应的单元格索引。
    /// </summary>
    /// <param name="snapshot">要搜索的棋盘快照。</param>
    /// <param name="tile">要查找的方块。</param>
    /// <returns>方块所在的单元格索引；未找到时返回 -1。</returns>
    private int FindTileIndex(BoardSnapshot snapshot, Tile tile)
    {
        for (int i = 0; i < snapshot.tileIds.Length; i++)
        {
            if (snapshot.tileIds[i] == tile.Identity) return i;
        }

        return -1;
    }

    /// <summary>
    /// 根据身份标识查找当前仍存在于棋盘中的方块。
    /// </summary>
    /// <param name="identity">要查找的方块身份标识。</param>
    /// <returns>匹配的方块；未找到时返回 <see langword="null"/>。</returns>
    private Tile FindTile(int identity)
    {
        foreach (Tile tile in tiles)
        {
            if (tile != null && tile.Identity == identity) return tile;
        }

        return null;
    }

    /// <summary>
    /// 查找指定方块状态在状态配置数组中的索引。
    /// </summary>
    /// <param name="state">要查找的方块状态。</param>
    /// <returns>状态所在索引；未找到时返回 -1。</returns>
    private int IndexOf(TileState state)
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (state == tileStates[i])
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 等待移动动画完成，解锁方块，生成新方块并检查游戏结束状态。
    /// </summary>
    /// <returns>用于 Unity 协程调度的迭代器。</returns>
    private IEnumerator WaitForChanges()
    {
        // 暂停输入，避免动画过程中重复处理棋盘。
        waiting = true;

        yield return new WaitForSeconds(0.15f);

        waiting = false;

        foreach (var tile in tiles)
        {
            tile.locked = false;
        }

        // 每次有效移动后只生成一个新方块。
        foreach (var cell in grid.cells)
        {
            if (cell.empty)
            {
                CreateTile();
                break;
            }
        }

        if (CheckForGameOver())
        {
            gameManager.GameOver();
        }
    }

    /// <summary>
    /// 检查棋盘是否已无空位且不存在可合并的相邻方块。
    /// </summary>
    /// <returns>没有可用移动时返回 <see langword="true"/>。</returns>
    public bool CheckForGameOver()
    {
        foreach (var cell in grid.cells)
        {
            if (cell.empty) return false;

            Tile tile = cell.tile;
            TileCell up = grid.GetAdjacentCell(cell, Vector2Int.up);
            TileCell right = grid.GetAdjacentCell(cell, Vector2Int.right);

            if (up != null && up.occupied && CanMerge(tile, up.tile))
            {
                return false;
            }

            if (right != null && right.occupied && CanMerge(tile, right.tile))
            {
                return false;
            }
        }

        return true;
    }

}
