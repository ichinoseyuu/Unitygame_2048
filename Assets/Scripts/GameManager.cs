using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public TileBoard board; // 游戏的核心玩法面板
    public UIManager uiManager; //UI管理类
    private int score; //当前分数

    private void Awake()
    {
        if (Application.platform == RuntimePlatform.Android) Application.targetFrameRate = 90;//帧率
    }


    /// <summary>
    /// 新游戏
    /// </summary>
    public void NewGame()
    {
        SetScore(0);// 还原分数
        uiManager.hiscoreText.text = LoadHiscore().ToString();// 加载最高分数
        board.ClearBoard();//清除面板
        board.CreateTile();//生成一个方块
        board.CreateTile();
        board.enabled = true;//允许面板中的方块移动，代表游戏开始
    }
    
    /// <summary>
    /// 游戏结束，显示面板
    /// </summary>
    public void GameOver()
    {
        board.enabled = false;//关闭方块移动
        PlayerPrefs.Save();
        uiManager.MindowSlideIn(uiManager.overWindow, Vector2.zero);
        SoundManager.Instance.playSound(SoundManager.Instance.faildClip);
    }

    /// <summary>
    /// 增加分数
    /// </summary>
    /// <param name="points"></param>
    public void IncreaseScore(int points)
    {
        SetScore(score + points);
    }
    
      /// <summary>
     /// 设置分数
     /// </summary>
     /// <param name="score"></param>
    private void SetScore(int score)
    {
        this.score = score;
        uiManager.scoreText.text = score.ToString();

        // 保存分数
        SaveHiscore();
    }

    /// <summary>
    /// 如果已经超过历史分数，那么就将当前分数设置为最高分数
    /// </summary>
    private void SaveHiscore()
    {
        int hiscore = LoadHiscore();

        if (score > hiscore) {
            PlayerPrefs.SetInt("hiscore", score);
        }
    }
    
    /// <summary>
    /// 加载最高分数
    /// </summary>
    /// <returns></returns>
    private int LoadHiscore()
    {
        return PlayerPrefs.GetInt("hiscore", 0);
    }

    
}
