using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class Tile : MonoBehaviour
{
    public TileState state { get; private set; } // 状态，用于处理数字和背景颜色
    public TileCell cell { get; private set; } // 当前在哪个单元格上
    public bool locked { get; set; } // 是否被锁定 (当逻辑判定改单元格需要发生变化的时候，就会被锁定，防止其他逻辑再用它)

    private Image background; // 背景色
    private TextMeshProUGUI text;// 数字

    private void Awake()
    {
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// 设置状态
    /// </summary>
    /// <param name="state"></param>
    public void SetState(TileState state)
    {
        this.state = state;
        
        background.color = state.backgroundColor;
        text.color = state.textColor;
        text.text = state.number.ToString();
    }

    /// <summary>
    /// 关联Cell，并设置坐标
    /// </summary>
    /// <param name="cell"></param>
    public void LinkCell(TileCell cell)
    {
        if (cell == null)
        {
            Debug.LogError("Failed to link tile to cell: cell is null");
            return;
        }

        // 先解除已有的绑定
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        // 关联现在的Cell
        this.cell = cell;
        this.cell.tile = this;

        // 设置到对应的位置
        transform.position = cell.transform.position;
    }

    /// <summary>
    /// 将Tile移动到某个Cell的位置上
    /// </summary>
    /// <param name="cell"></param>
    public void MoveTo(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        // 用协程做一个简单的动画效果
        //StartCoroutine(MoveAnimate(cell.transform.position, false));
        MoveAnimate(false, cell.transform.position);
    }

    /// <summary>
    /// 将Tile合并到某个Cell的位置上
    /// </summary>
    /// <param name="cell"></param>
    public void Merge(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = null;
        cell.tile.locked = true;

        // 和移动的区别是最后一个参数，控制是否是合并过去的
        //StartCoroutine(MoveAnimate(cell.transform.position, true));
        MoveAnimate(true, cell.transform.position);
    }

    //旧的动画方法
    //private IEnumerator MoveAnimate(Vector3 to, bool merging)
    //{
    //    float elapsed = 0f;
    //    float duration = 0.1f;

    //    Vector3 from = transform.position;

    //    // 在持续的时间内，用插值的方式将位置移动过去
    //    while (elapsed < duration)
    //    {
    //        //transform.position = Vector3.Lerp(from, to, elapsed / duration);
    //        transform.position = Vector3.LerpUnclamped(from, to, elapsed / duration);
    //        elapsed += Time.deltaTime;
    //        yield return null;
    //    }

    //    // 防止动画和时间有误差而导致没对齐，动画执行完之后直接把坐标设置到对于位置来保底
    //    transform.position = to;

    //    // 如果是合并过去的，完成之后需要把自己删除
    //    if (merging)
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    /// <summary>
    /// 移动动画
    /// </summary>
    /// <param name="merging">是否合并</param>
    /// <param name="to">目标位置</param>
    private void MoveAnimate(bool merging, Vector3 to)
    {
        transform.DOMove(to, 0.15f).OnComplete(() =>
        {
            transform.position = to;

            if (merging)
            {
                Destroy(gameObject);
            }
        });
    }
}
