using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.UI;

public class MyButtons : MonoBehaviour
{
    /// <summary>获取当前场景中的按钮控制器单例。</summary>
    public static MyButtons Instance { get; private set; }

    public Button[] buttons;
    public GameManager gameManager;
    public UIManager uiManager;
    public SoundManager soundManager;
    public AudioClip hoverSound; // 悬停音效
    public AudioClip pressSound; // 点击音效

    /// <summary>
    /// 初始化按钮控制器单例，并销毁重复实例。
    /// </summary>
    private void Awake()
    {
        // 确保只有一个实例存在
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // 保持该对象在场景切换时不被销毁
        }
        else
        {
            Destroy(gameObject); // 销毁多余的实例
        }
    }

    /// <summary>
    /// 为已配置的按钮添加并初始化按钮动画组件。
    /// </summary>
    private void Start()
    {
        // 复用已存在的动画组件，避免为同一按钮重复添加组件。
        foreach (Button button in buttons)
        {
            if (button != null)
            {
                // 检查是否已经存在 ButtonAnimator 组件，避免重复添加
                ButtonAnimator animator = button.GetComponent<ButtonAnimator>();
                if (animator == null)
                {
                    // 如果没有，添加 ButtonAnimator 组件
                    animator = button.gameObject.AddComponent<ButtonAnimator>();
                }

                // 配置 ButtonAnimator 组件
                animator.button = button;
            }
            else
            {
                Debug.LogWarning("MyButtons 脚本中的一个或多个按钮未被赋值。");
            }
        }
    }

    /// <summary>
    /// 开始新游戏并关闭开始界面。
    /// </summary>
    public void StartGame()
    {
        gameManager.NewGame();
        uiManager.WindowSlideOut(uiManager.startWindow, uiManager.GetLeftWindowPos(), false);
    }
    /// <summary>
    /// 关闭设置界面并显示主菜单。
    /// </summary>
    public void OpenMainMenu()
    {
        SettingsBack();
        uiManager.WindowSlideIn(uiManager.startWindow, Vector2.zero);
    }
    /// <summary>
    /// 显示帮助窗口并暂停棋盘操作。
    /// </summary>
    public void OpenHelpWindow()
    {
        uiManager.WindowSlideIn(uiManager.helpWindow, Vector2.zero);
        SetBoardState(false);
    }

    /// <summary>
    /// 显示设置窗口并暂停棋盘操作。
    /// </summary>
    public void OpenSettingsWindow()
    {
        uiManager.WindowSlideIn(uiManager.settingsWindow, Vector2.zero);
        SetBoardState(false);
    }
    /// <summary>
    /// 关闭帮助窗口；若设置窗口仍打开则保持棋盘暂停。
    /// </summary>
    public void HelpBack()
    {
        uiManager.WindowSlideOut(uiManager.helpWindow, uiManager.GetRightWindowPos(), false);
        if (uiManager.settingsWindow.IsActive()) return;
        SetBoardState(true);
    }
    /// <summary>
    /// 关闭设置窗口；若帮助窗口仍打开则保持棋盘暂停。
    /// </summary>
    public void SettingsBack()
    {
        uiManager.WindowSlideOut(uiManager.settingsWindow, uiManager.GetRightWindowPos(), false);
        if (uiManager.helpWindow.IsActive()) return;
        SetBoardState(true);
    }
    /// <summary>
    /// 开始新游戏，并在需要时关闭游戏结束窗口。
    /// </summary>
    public void NewGame()
    {
        gameManager.NewGame();
        if (uiManager.overWindow.IsActive()) uiManager.WindowSlideOut(uiManager.overWindow, uiManager.GetUpwardWindowPos(), false);
    }
    /// <summary>
    /// 重新开始游戏并关闭游戏结束窗口。
    /// </summary>
    public void TryAgain()
    {
        gameManager.NewGame();
        uiManager.WindowSlideOut(uiManager.overWindow, uiManager.GetUpwardWindowPos(), false);
    }

    /// <summary>
    /// 请求游戏管理器撤销最近一次移动。
    /// </summary>
    public void UndoLastMove()
    {
        gameManager.UndoLastMove();
    }
    /// <summary>
    /// 保存并应用背景音乐音量滑块的当前值。
    /// </summary>
    public void SaveBGMVolume()
    {
        soundManager.SaveSoundValue(soundManager.BGM, uiManager.BGMSlider, uiManager.BGMText,
            soundManager.BGMkey);
    }

    /// <summary>
    /// 保存并应用音效音量滑块的当前值。
    /// </summary>
    public void SaveEffctVolume()
    {
        soundManager.SaveSoundValue(soundManager.SoundEffects, uiManager.soundEffectsSlider,
            uiManager.soundEffectsText, soundManager.Effectkey);
    }

    /// <summary>
    /// 在编辑器中停止运行，或在构建版本中退出应用程序。
    /// </summary>
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(0);
#endif

    }
    /// <summary>
    /// 设置棋盘是否可以响应移动操作。
    /// </summary>
    /// <param name="state"><see langword="true"/> 启用棋盘；<see langword="false"/> 禁用棋盘。</param>
    private void SetBoardState(bool state)
    {
        // 状态未变化时无需重复赋值。
        if (gameManager.board.enabled == state) return;
        gameManager.board.enabled = state;
    }
}
