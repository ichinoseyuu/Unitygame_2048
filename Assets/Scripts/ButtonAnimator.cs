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

    void Start()
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

    private void AnimateScale(float scale, TweenCallback onComplete = null)
    {
        button.transform.DOScale(originalScale * scale, duration).SetEase(Ease.OutBack).OnComplete(onComplete);
    }

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

    private void OnPointerEnter()
    {
        if (Application.platform == RuntimePlatform.Android) return;
        AnimateScale(hoverScale);
        PlaySound(MyButtons.Instance.hoverSound);
    }

    private void OnPointerExit()
    {
        AnimateScale(1f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
