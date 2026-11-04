using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    /// <summary>获取当前场景中的音效管理器单例。</summary>
    public static SoundManager Instance { get; private set; }
    public string BGMkey = "BGMVolume";
    public string Effectkey = "SoundEffectVolume";
    public UIManager uiManager;
    public AudioClip BGMClip;
    public AudioClip mergeClip;
    public AudioClip faildClip;
    public AudioSource BGM;
    public AudioSource SoundEffects;

    /// <summary>
    /// 建立音效管理器单例，并销毁重复实例。
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
    /// 初始化音量偏好设置，应用已保存音量并启动背景音乐。
    /// </summary>
    private void Start()
    {
        // 首次运行时为各类音量写入默认值。
        if (!PlayerPrefs.HasKey(BGMkey))
        {
            PlayerPrefs.SetFloat(BGMkey, 0.8f);
        }
        LoadSoundValue(BGM, uiManager.BGMSlider, uiManager.BGMText, BGMkey);
        if (!PlayerPrefs.HasKey(Effectkey))
        {
            PlayerPrefs.SetFloat(Effectkey, 0.8f);
        }
        LoadSoundValue(SoundEffects, uiManager.soundEffectsSlider, uiManager.soundEffectsText, Effectkey);
        BGM.clip = BGMClip;
        BGM.Play();
    }

    /// <summary>
    /// 在音效通道空闲时播放指定音效。
    /// </summary>
    /// <param name="audioClip">要播放的音频片段。</param>
    public void playSound(AudioClip audioClip)
    {
        if (SoundEffects.isPlaying) return; 
        SoundEffects.PlayOneShot(audioClip);
    }

    /// <summary>
    /// 将滑块音量应用到音频源和界面，并保存到本地偏好设置。
    /// </summary>
    /// <param name="audioSource">要设置音量的音频源。</param>
    /// <param name="slider">提供音量值的滑块。</param>
    /// <param name="text">显示音量百分比的文本组件。</param>
    /// <param name="name">用于保存音量值的偏好设置键。</param>
    public void SaveSoundValue(AudioSource audioSource,Slider slider, TextMeshProUGUI text, string name)
    {
        audioSource.volume = slider.value;
        text.text = (slider.value * 100).ToString("#0") + "%";
        PlayerPrefs.SetFloat(name, slider.value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 从本地偏好设置读取音量，并同步到音频源、滑块和文本。
    /// </summary>
    /// <param name="audioSource">要应用音量的音频源。</param>
    /// <param name="slider">用于显示音量值的滑块。</param>
    /// <param name="text">显示音量百分比的文本组件。</param>
    /// <param name="name">用于读取音量值的偏好设置键。</param>
    public void LoadSoundValue(AudioSource audioSource, Slider slider, TextMeshProUGUI text, string name)
    {
        if (PlayerPrefs.HasKey(name))
        {
            slider.value = PlayerPrefs.GetFloat(name);
            audioSource.volume = slider.value;
            text.text = (slider.value * 100).ToString("#0") + "%";
        }
    }

}
