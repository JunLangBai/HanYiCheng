using DG.Tweening;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [Header("震动参数")]
    [Tooltip("震动持续时间(秒)")] 
    public float duration = 0.3f;
    
    [Tooltip("最大震动强度(像素)")]
    public float strength = 0.02f;
    
    [Tooltip("震动频率(越高越密集)")]
    [Range(1, 50)] public int vibrato = 5;
    
    [Tooltip("方向随机性(0-180)")]
    [Range(0, 180)] public float randomness = 180f;
    
    [Tooltip("启用整数像素对齐")]
    public bool snapping = false;
    
    [Header("复位控制")]
    [Tooltip("启用精确复位")] 
    public bool preciseReset = true;
    
    [Tooltip("复位平滑时间")] 
    [Range(0, 0.5f)] public float resetSmoothness = 0.1f;

    private RectTransform targetRect;
    private Vector2 originalPos;
    private Tween shakeTween;

    void Awake()
    {
        targetRect = GetComponent<RectTransform>();
        originalPos = targetRect.anchoredPosition;
    }

    /// <summary>
    /// 触发屏幕震动
    /// </summary>
    public void ShakeScreen()
    {
        // 确保只有一个震动在进行
        if (shakeTween != null && shakeTween.IsActive())
        {
            shakeTween.Kill();
            if (preciseReset) ResetPosition();
        }

        // 开始新震动
        shakeTween = targetRect.DOShakeAnchorPos(
            duration: duration,
            strength: strength,
            vibrato: vibrato,
            randomness: randomness,
            snapping: snapping
        )
        .SetEase(Ease.OutQuad)
        .OnComplete(() => {
            if (preciseReset) 
            {
                // 平滑复位到原始位置
                targetRect.DOAnchorPos(originalPos, resetSmoothness)
                    .SetEase(Ease.OutCubic);
            }
        })
        .OnKill(() => {
            if (preciseReset) ResetPosition();
        });
    }

    /// <summary>
    /// 立即停止震动并复位
    /// </summary>
    public void StopShake()
    {
        if (shakeTween != null && shakeTween.IsActive())
        {
            shakeTween.Kill();
        }
        ResetPosition();
    }

    /// <summary>
    /// 复位到原始位置
    /// </summary>
    private void ResetPosition()
    {
        targetRect.anchoredPosition = originalPos;
    }

    void OnDisable()
    {
        // 对象禁用时停止震动
        if (shakeTween != null && shakeTween.IsActive())
        {
            shakeTween.Kill();
        }
        ResetPosition();
    }
}