using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.UI;

public class MyButtons : MonoBehaviour
{
    public static MyButtons Instance { get; private set; }

    public Button[] buttons;
    public GameManager gameManager;
    public UIManager uiManager;
    public SoundManager soundManager;
    public AudioClip hoverSound; // 悬停音效
    public AudioClip pressSound; // 点击音效

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

    private void Start()
    {
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
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        gameManager.NewGame();
        uiManager.MindowSlideOut(uiManager.startWindow, uiManager.GetLeftWindowPos(), false);
    }
    /// <summary>
    /// 打开帮助窗口
    /// </summary>
    public void OpenHelpWindow()
    {
        uiManager.MindowSlideIn(uiManager.helpWindow, Vector2.zero);
    }
    /// <summary>
    /// 打开设置窗口
    /// </summary>
    public void OpenSettingsWindow()
    {
        uiManager.MindowSlideIn(uiManager.settingsWindow, Vector2.zero);
        SetBoardState(false);
    }
    /// <summary>
    /// 关闭帮助窗口
    /// </summary>
    public void HelpBack()
    {
        uiManager.MindowSlideOut(uiManager.helpWindow, uiManager.GetRightWindowPos(), false);
        if(uiManager.settingsWindow.IsActive()) return;
        SetBoardState(true);
    }
    /// <summary>
    /// 关闭设置
    /// </summary>
    public void SettingsBack()
    {
        uiManager.MindowSlideOut(uiManager.settingsWindow, uiManager.GetRightWindowPos(), false);
        if (uiManager.helpWindow.IsActive()) return;
        SetBoardState(true);
    }
    /// <summary>
    /// 新游戏
    /// </summary>
    public void NewGame()
    {
        gameManager.NewGame();
        if(uiManager.overWindow.IsActive()) uiManager.MindowSlideOut(uiManager.overWindow, uiManager.GetUpwardWindowPos(),false);
    }
    /// <summary>
    /// 再试一次
    /// </summary>
    public void TryAgain()
    {
        gameManager.NewGame();
        uiManager.MindowSlideOut(uiManager.overWindow, uiManager.GetUpwardWindowPos(), false);
    }
    /// <summary>
    /// 保存音乐音量
    /// </summary>
    public void SaveBGMVolume()
    {
        soundManager.SaveSoundValue(soundManager.BGM, uiManager.BGMSlider, uiManager.BGMText,
            soundManager.BGMkey);
    }

    /// <summary>
    /// 保存音效音量
    /// </summary>
    public void SaveEffctVolume()
    {
        soundManager.SaveSoundValue(soundManager.SoundEffects, uiManager.soundEffectsSlider,
            uiManager.soundEffectsText, soundManager.Effectkey);
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(0);
#endif

    }
    private void SetBoardState(bool state)
    {
        if (gameManager.board.enabled == state) return;
        gameManager.board.enabled = state;
    }
}
