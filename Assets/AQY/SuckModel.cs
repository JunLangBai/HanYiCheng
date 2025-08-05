using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SuckModel : MonoBehaviour
{
    public float amplitude = 0.2f; // 晃动幅度
    public float duration = 0.5f;  // 单程运动时间

    void Start()
    {
        // 保存初始Y坐标
        float startY = transform.position.y;
        
        // 创建上下晃动的Tween
        transform.DOMoveY(startY + amplitude, duration)
            .SetLoops(-1, LoopType.Yoyo) // 无限循环并往返运动
            .SetEase(Ease.InOutSine);    // 平滑缓动效果
    }
}
