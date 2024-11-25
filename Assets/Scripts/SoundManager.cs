using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public string BGMkey = "BGMVolume";
    public string Effectkey = "SoundEffectVolume";
    public UIManager uiManager;
    public AudioClip BGMClip;
    public AudioClip mergeClip;
    public AudioClip faildClip;
    public AudioSource BGM;
    public AudioSource SoundEffects;

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

    public void playSound(AudioClip audioClip)
    {
        if (SoundEffects.isPlaying) return; 
        SoundEffects.PlayOneShot(audioClip);
    }

    /// <summary>
    /// 保存并改变slider的值
    /// </summary>
    /// <param name="audioSource">目标组件</param>
    /// <param name="slider">需要保存的slider</param>
    /// <param name="text">显示的文本</param>
    /// <param name="name">保存的key</param>
    public void SaveSoundValue(AudioSource audioSource,Slider slider, TextMeshProUGUI text, string name)
    {
        audioSource.volume = slider.value;
        text.text = (slider.value * 100).ToString("#0") + "%";
        PlayerPrefs.SetFloat(name, slider.value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 加载并改变slider的值
    /// </summary>
    /// <param name="audioSource">目标组件</param>
    /// <param name="slider">需要加载的slider</param>
    /// <param name="text">显示的文本</param>
    /// <param name="name">加载的key</param>
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
