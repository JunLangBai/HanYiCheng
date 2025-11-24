using UnityEngine;
using DG.Tweening; // 引入DOTween命名空间
using UnityEngine.UI; // 如果你使用了标准的UI组件

public class BubblePopEffect : MonoBehaviour
{
    // 拖拽你的气泡UI对象（例如一个Panel或Image）到这个字段
    private RectTransform bubbleRectTransform;
    
    // 气泡最终的缩放目标（通常是Vector3.one，即(1, 1, 1)）
    private readonly Vector3 targetScale = Vector3.one;

    // 动画持续时间
    private float duration = 0.3f;

    // 关键：使用的缓动类型，"OutBack"或"OutElastic"通常能提供很好的“弹出”感
    public Ease popEase = Ease.OutBack; 
    
    // 建议在Awake或Start中调用一次，确保气泡初始是缩小的
    void Awake()
    {
        Transform childTrans = transform.Find("chatBubble");
        if (childTrans != null)
        {
            bubbleRectTransform = childTrans.GetComponent<RectTransform>();
        }

        // 确保一开始气泡是不可见的（完全缩小）
        if (bubbleRectTransform != null)
        {
            bubbleRectTransform.localScale = Vector3.zero;
        }
    }

    /// <summary>
    /// 播放气泡弹出动画
    /// </summary>
    public void PlayPopAnimation()
    {
        // 确保没有其他动画在播放，并设置初始状态
        bubbleRectTransform.localScale = Vector3.zero;
        
        // 使用DOTween进行缩放动画
        bubbleRectTransform.DOScale(targetScale, duration)
            .SetEase(popEase) // 设置缓动类型，产生弹性效果
            .OnComplete(() =>
            {
                // 动画完成后可以执行一些操作，例如打印日志
                Debug.Log("气泡弹出动画完成！");
            });
    }
}