using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Unity.VisualScripting;

public class ButtonAnimator : MonoBehaviour
{
    public Button button; // 目标按钮
    public float hoverScale = 1.2f; // Hover 时的缩放比例
    public float pressScale = 0.9f; // Press 时的缩放比例
    public float duration = 0.2f; // 动画时长

    private AudioSource audioSource; //AudioSource 组件
    private Vector3 originalScale;

    /// <summary>
    /// 初始化按钮音效、缩放基准，并注册鼠标和点击事件。
    /// </summary>
    private void Start()
    {
        //if (audioSource == null)
        //{
        //    GameObject audioObject = new GameObject("UIAudioSource");
        //    audioSource = audioObject.AddComponent<AudioSource>();
        //    //DontDestroyOnLoad(audioObject); // 保持在场景切换中
        //}
        audioSource = SoundManager.Instance.SoundEffects;
        originalScale = button.transform.localScale;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry hoverEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        hoverEntry.callback.AddListener((eventData) => OnPointerEnter());
        trigger.triggers.Add(hoverEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        exitEntry.callback.AddListener((eventData) => OnPointerExit());
        trigger.triggers.Add(exitEntry);

        button.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// 将按钮动画缩放到指定倍率，并在动画结束时执行回调。
    /// </summary>
    /// <param name="scale">相对于按钮初始尺寸的缩放倍率。</param>
    /// <param name="onComplete">动画结束后执行的回调；可为空。</param>
    private void AnimateScale(float scale, TweenCallback onComplete = null)
    {
        button.transform.DOScale(originalScale * scale, duration).SetEase(Ease.OutBack).OnComplete(onComplete);
    }

    /// <summary>
    /// 响应按钮点击，播放按下音效，并在 Android 上播放按压动画。
    /// </summary>
    private void OnClick()
    {
        PlaySound(MyButtons.Instance.pressSound);
        if (Application.platform == RuntimePlatform.Android)
        {
            AnimateScale(pressScale, () =>
            {
                AnimateScale(1f);
            });
        }  
    }

    /// <summary>
    /// 响应鼠标进入按钮区域，播放悬停动画和音效。
    /// </summary>
    private void OnPointerEnter()
    {
        if (Application.platform == RuntimePlatform.Android) return;
        AnimateScale(hoverScale);
        PlaySound(MyButtons.Instance.hoverSound);
    }

    /// <summary>
    /// 响应鼠标离开按钮区域，并将按钮缩放恢复到初始尺寸。
    /// </summary>
    private void OnPointerExit()
    {
        AnimateScale(1f);
    }

    /// <summary>
    /// 使用按钮音效源播放指定音效。
    /// </summary>
    /// <param name="clip">要播放的音频片段；为空时不播放。</param>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
