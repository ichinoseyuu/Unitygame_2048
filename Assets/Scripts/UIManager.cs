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

    /// <summary>获取窗口向上滑出时的目标位置。</summary>
    /// <returns>窗口的向上滑出位置。</returns>
    public Vector2 GetUpwardWindowPos() { return upwardWindowPos; }
    /// <summary>获取窗口向下滑出时的目标位置。</summary>
    /// <returns>窗口的向下滑出位置。</returns>
    public Vector2 GetDownWindowPos() { return downWindowPos; }
    /// <summary>获取窗口向右滑出时的目标位置。</summary>
    /// <returns>窗口的向右滑出位置。</returns>
    public Vector2 GetRightWindowPos() { return rightWindowPos; }
    /// <summary>获取窗口向左滑出时的目标位置。</summary>
    /// <returns>窗口的向左滑出位置。</returns>
    public Vector2 GetLeftWindowPos() { return leftWindowPos; }


    /// <summary>
    /// 将窗口滑动到目标位置，并在动画结束时设置其激活状态。
    /// </summary>
    /// <param name="window">要移动的窗口。</param>
    /// <param name="target">动画结束时窗口的锚点位置。</param>
    /// <param name="state">动画结束后窗口是否保持激活。</param>
    /// <param name="duration">动画时长，单位为秒。</param>
    public void WindowSlideOut(Image window, Vector2 target, bool state = true, float duration = 0.5f)
    {
        // 动画结束后再设置激活状态，避免窗口提前消失。
        RectTransform imageRectTransform = window.rectTransform;
        imageRectTransform.DOAnchorPos(target, duration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            window.gameObject.SetActive(state);
        });

    }

    /// <summary>
    /// 激活窗口并将其滑动到目标位置。
    /// </summary>
    /// <param name="window">要显示并移动的窗口。</param>
    /// <param name="target">动画结束时窗口的锚点位置。</param>
    /// <param name="duration">动画时长，单位为秒。</param>
    public void WindowSlideIn(Image window, Vector2 target, float duration = 0.5f)
    {
        // 先激活窗口，确保其可以参与位置动画。
        window.gameObject.SetActive(true);
        RectTransform imageRectTransform = window.rectTransform;
        imageRectTransform.DOAnchorPos(target, duration).SetEase(Ease.InOutQuad);
    }
}
