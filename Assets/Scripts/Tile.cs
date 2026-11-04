using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class Tile : MonoBehaviour
{
    private static int nextIdentity;

    public TileState state { get; private set; } // 状态，用于处理数字和背景颜色
    public TileCell cell { get; private set; } // 当前在哪个单元格上
    public int Identity { get; private set; }
    public bool locked { get; set; } // 是否被锁定 (当逻辑判定改单元格需要发生变化的时候，就会被锁定，防止其他逻辑再用它)

    private Image background; // 背景色
    private TextMeshProUGUI text;// 数字

    /// <summary>
    /// 初始化方块身份标识，并缓存其背景图和数字文本组件。
    /// </summary>
    private void Awake()
    {
        Identity = ++nextIdentity;
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// 设置方块身份标识，并同步静态计数器以避免后续标识重复。
    /// </summary>
    /// <param name="identity">要分配给方块的身份标识。</param>
    public void SetIdentity(int identity)
    {
        Identity = identity;
        nextIdentity = Mathf.Max(nextIdentity, identity);
    }

    /// <summary>
    /// 更新方块状态及其背景颜色、文字颜色和数字显示。
    /// </summary>
    /// <param name="state">要应用的方块状态。</param>
    public void SetState(TileState state)
    {
        this.state = state;

        background.color = state.backgroundColor;
        text.color = state.textColor;
        text.text = state.number.ToString();
    }

    /// <summary>
    /// 将方块关联到指定单元格，并立即对齐其位置。
    /// </summary>
    /// <param name="cell">要关联的单元格。</param>
    public void LinkCell(TileCell cell)
    {
        if (cell == null)
        {
            Debug.LogError("Failed to link tile to cell: cell is null");
            return;
        }

        // 先解除原单元格的反向引用，避免残留占用状态。
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
    /// 将方块的逻辑位置更新到指定单元格，并播放移动动画。
    /// </summary>
    /// <param name="cell">方块移动到的单元格。</param>
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
    /// 将方块关联到目标单元格，并以 DOTween 动画移动到该位置。
    /// </summary>
    /// <param name="cell">动画结束时方块所在的单元格。</param>
    /// <param name="duration">动画时长，单位为秒。</param>
    /// <returns>控制方块移动的 DOTween 动画。</returns>
    public Tween AnimateToCell(TileCell cell, float duration)
    {
        // 先更新逻辑占用关系，再播放视觉移动动画。
        this.cell = cell;
        cell.tile = this;
        transform.DOKill();
        return transform.DOMove(cell.transform.position, duration).SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// 解除方块与当前单元格之间的双向关联。
    /// </summary>
    public void UnlinkCell()
    {
        if (cell != null && cell.tile == this)
        {
            cell.tile = null;
        }

        cell = null;
    }

    /// <summary>
    /// 解除方块的当前关联，并播放合并到目标单元格的动画。
    /// </summary>
    /// <param name="cell">执行合并的目标单元格。</param>
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
    /// 播放方块移动动画，并在合并移动结束后销毁当前方块。
    /// </summary>
    /// <param name="merging"><see langword="true"/> 表示移动完成后销毁方块。</param>
    /// <param name="to">动画结束时的世界坐标。</param>
    private void MoveAnimate(bool merging, Vector3 to)
    {
        // 动画结束时再次对齐目标位置，避免浮点误差。
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
