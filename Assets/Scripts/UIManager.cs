using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image overWindow; // 游戏结束窗口
    public Image helpWindow;// 帮助窗口
    public Image settingsWindow;// 设置窗口
    public Image startWindow;// 开始界面
    public TextMeshProUGUI scoreText; // 分数
    public TextMeshProUGUI hiscoreText;// 最高分数
    public Slider BGMSlider;//音乐音量滚动条
    public Slider soundEffectsSlider; //音效音量滚动条
    public TextMeshProUGUI BGMText; //音乐音量文本
    public TextMeshProUGUI soundEffectsText;//音效音量文本

    private Vector2 upwardWindowPos = new Vector2(0, 1200f);
    private Vector2 downWindowPos = new Vector2(0, -1200f);
    private Vector2 rightWindowPos = new Vector2(1200f, 0);
    private Vector2 leftWindowPos = new Vector2(-1200f, 0);
    private bool isHelpWindowOn;

    public Vector2 GetUpwardWindowPos() {return upwardWindowPos;}
    public Vector2 GetDownWindowPos() {return downWindowPos;}
    public Vector2 GetRightWindowPos() {return rightWindowPos;}
    public Vector2 GetLeftWindowPos() {return leftWindowPos;}
    

    /// <summary>
        /// 窗口动画关闭
        /// </summary>
        /// <param name="window">目标窗口</param>
        /// <param name="target">目标位置</param>
        /// <param name="state">结束状态</param>
        /// <param name="duration">动画时间</param>
    public void MindowSlideOut(Image window, Vector2 target, bool state = true, float duration = 0.5f)
    {
        RectTransform imageRectTransform = window.rectTransform;
        imageRectTransform.DOAnchorPos(target, duration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            window.gameObject.SetActive(state);
        });

    }

    /// <summary>
    /// 窗口动画打开
    /// </summary>
    /// <param name="window">目标窗口</param>
    /// <param name="target">目标位置</param>
    /// <param name="duration">动画时间</param>
    public void MindowSlideIn(Image window, Vector2 target, float duration = 0.5f)
    {
        window.gameObject.SetActive(true);
        RectTransform imageRectTransform = window.rectTransform;
        imageRectTransform.DOAnchorPos(target, duration).SetEase(Ease.InOutQuad);
    }
}
