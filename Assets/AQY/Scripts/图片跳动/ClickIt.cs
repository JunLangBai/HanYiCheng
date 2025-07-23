using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ClickIt : MonoBehaviour
{
    public float interval = 5f; // 触发间隔（秒）
    public float fadeDuration = 1f; // 渐变持续时间（秒）
    
    private CanvasGroup canvasGroup;
    private bool isAnimating = false;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0; // 初始状态透明
        
        // 不再使用Invoke，改用协程控制完整流程
        StartCoroutine(AnimationCycle());
    }

    // 完整的动画循环协程
    IEnumerator AnimationCycle()
    {
        while (true)
        {
            // 等待间隔时间（减去动画时间）
            yield return new WaitForSeconds(interval - (fadeDuration * 2 + 1.5f));
            
            // 开始新动画周期
            isAnimating = true;
            
            // 淡入效果
            yield return Fade(1); // 淡入到完全不透明
            
            // 连续戳动三次（带间隔）
            for (int i = 0; i < 3; i++)
            {
                PokeWithDOTween();
                yield return new WaitForSeconds(0.5f); // 每次戳动间隔
            }
            
            // 淡出效果
            yield return Fade(0); // 淡出到完全透明
            
            isAnimating = false;
        }
    }

    // 通用的淡入/淡出协程
    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    public void PokeWithDOTween()
    {
        transform.DOComplete();
        transform.DOPunchPosition(
            punch: Vector3.down * 30f,
            duration: 0.5f,
            vibrato: 5,
            elasticity: 0.5f
        );
    }
}
