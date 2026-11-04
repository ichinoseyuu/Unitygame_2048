using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public TileBoard board; // 游戏的核心玩法面板
    public UIManager uiManager; //UI管理类
    private int score; //当前分数

    /// <summary>
    /// 初始化游戏管理器，并为 Android 设置目标帧率。
    /// </summary>
    private void Awake()
    {
        if (Application.platform == RuntimePlatform.Android) Application.targetFrameRate = 90;//帧率
    }

    /// <summary>
    /// 获取当前游戏分数。
    /// </summary>
    public int Score => score;

    /// <summary>
    /// 检测键盘快捷键，并在按下 Ctrl+Z 时撤销一步。
    /// </summary>
    private void Update()
    {
        if (Input.GetKey(KeyCode.Backspace))
        {
            UndoLastMove();
        }
    }

    /// <summary>
    /// 重置分数和棋盘，并创建新游戏的初始方块。
    /// </summary>
    public void NewGame()
    {
        // 清理旧棋盘并更新界面，再生成两个初始方块。
        SetScore(0);// 还原分数
        uiManager.hiscoreText.text = LoadHiscore().ToString();// 加载最高分数
        board.ClearBoard();//清除面板
        board.CreateTile();//生成一个方块
        board.CreateTile();
        board.enabled = true;//允许面板中的方块移动，代表游戏开始
    }

    /// <summary>
    /// 停止棋盘操作，保存数据并显示游戏结束界面。
    /// </summary>
    public void GameOver()
    {
        board.enabled = false;//关闭方块移动
        PlayerPrefs.Save();
        uiManager.WindowSlideIn(uiManager.overWindow, Vector2.zero);
        SoundManager.Instance.playSound(SoundManager.Instance.faildClip);
    }

    /// <summary>
    /// 撤销棋盘上最近一次有效移动，并关闭游戏结束窗口（若已打开）。
    /// </summary>
    public void UndoLastMove()
    {
        if (!board.Undo()) return;

        board.enabled = true;
        if (uiManager.overWindow.IsActive())
        {
            uiManager.WindowSlideOut(uiManager.overWindow, uiManager.GetUpwardWindowPos(), false);
        }
    }

    /// <summary>
    /// 将指定分数加到当前分数上。
    /// </summary>
    /// <param name="points">要增加的分数。</param>
    public void IncreaseScore(int points)
    {
        SetScore(score + points);
    }

    /// <summary>
    /// 更新当前分数、界面显示及最高分记录。
    /// </summary>
    /// <param name="score">要设置的分数。</param>
    private void SetScore(int score)
    {
        this.score = score;
        uiManager.scoreText.text = score.ToString();

        // 保存分数
        SaveHiscore();
    }

    /// <summary>
    /// 将当前分数恢复为指定值。
    /// </summary>
    /// <param name="value">要恢复的分数。</param>
    public void RestoreScore(int value)
    {
        SetScore(value);
    }

    /// <summary>
    /// 当当前分数超过历史最高分时，更新保存的最高分。
    /// </summary>
    private void SaveHiscore()
    {
        int hiscore = LoadHiscore();

        if (score > hiscore)
        {
            PlayerPrefs.SetInt("hiscore", score);
        }
    }

    /// <summary>
    /// 加载已保存的历史最高分。
    /// </summary>
    /// <returns>已保存的最高分；若没有记录则返回 0。</returns>
    private int LoadHiscore()
    {
        return PlayerPrefs.GetInt("hiscore", 0);
    }


}
