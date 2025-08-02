using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BounceFadeInMoveSimultaneous : MonoBehaviour
{
    [Header("起点和目标")]
    public RectTransform startRect;
    public RectTransform targetRect;

    [Header("组件引用")]
    public RectTransform imageRect;
    public CanvasGroup canvasGroup;

    [Header("动画设置")]
    public float fadeInDuration = 0.5f;
    public float moveDuration = 1f;
    public bool disableAfter = false;

    void Start()
    {
        // 设置目标帧率为60FPS（AR眼镜推荐60-90FPS）
        Application.targetFrameRate = 120;
    
        // 确保关闭垂直同步
        QualitySettings.vSyncCount = 1;
        
        // 初始化位置和透明度
        imageRect.anchoredPosition = startRect.anchoredPosition;
        canvasGroup.alpha = 0f;

        // 同时启动淡入与移动动画
        canvasGroup.DOFade(1f, fadeInDuration);
        imageRect.DOAnchorPos(targetRect.anchoredPosition, moveDuration)
            .SetEase(Ease.OutBounce)
            .OnComplete(() => {
                if (disableAfter)
                    gameObject.SetActive(false);
            });
    }
}