using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random; // 需要引入这个来使用Linq

[System.Serializable]
public class AnimationCollection
{
    [Tooltip("这个集合对应的Trigger名称，必须和Animator中的完全一致")]
    public string triggerName;

    [Tooltip("所有属于这个Trigger的动画片段 (Animation Clips)")]
    public List<AnimationClip> clips;
}

public class OverrideAnimationManager : MonoBehaviour
{
    [Header("核心组件")]
    public Animator characterAnimator;
    public float randomTime;

    [Header("动画集合")]
    [Tooltip("在这里定义Trigger和它包含的随机动画列表")]
    public List<AnimationCollection> animationCollections;

    private AnimatorOverrideController overrideController;
    private float timer = 0f;
    void Awake()
    {
        if (characterAnimator == null || characterAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator 或其 Controller 未指定!", this.gameObject);
            return;
        }

        // 1. 基于当前Animator的“基础控制器”创建一个可覆盖的版本
        overrideController = new AnimatorOverrideController(characterAnimator.runtimeAnimatorController);
        
        // 2. 将Animator的控制器替换为我们新建的可覆盖版本
        characterAnimator.runtimeAnimatorController = overrideController;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= randomTime)
        {
            characterAnimator.SetTrigger("RandomWait");
            randomTime = Random.Range(10f, 25f);
            timer = 0f;
        }
    }

    
    
    /// <summary>
    /// 根据Trigger名称，从其对应的集合中随机播放一个动画。
    /// </summary>
    /// <param name="triggerName">你在Animator中定义的Trigger名称</param>
    public void PlayRandomAnimation(string triggerName)
    {
        if (overrideController == null) return;

        // 1. 根据triggerName找到对应的动画集合
        AnimationCollection collection = animationCollections.FirstOrDefault(c => c.triggerName == triggerName);

        if (collection != null && collection.clips.Count > 0)
        {
            // 2. 从集合中随机选择一个动画片段 (AnimationClip)
            int randomIndex = Random.Range(0, collection.clips.Count);
            AnimationClip randomClip = collection.clips[randomIndex];

            // 3. 【核心魔法】在“覆盖控制器”中，将占位符动画替换为我们随机选中的动画
            //    我们约定占位符状态的名称格式为 "Action" + Trigger名 + "_Placeholder"
            string placeholderName = "Action" + triggerName + "_Placeholder";
            overrideController[placeholderName] = randomClip;
            
            Debug.Log($"为Trigger '{triggerName}' 随机选择动画 '{randomClip.name}' 并设置到状态 '{placeholderName}'");

            // 4. 最后，安全地触发Trigger
            characterAnimator.SetTrigger(triggerName);
        }
        else
        {
            Debug.LogWarning($"未找到名为 '{triggerName}' 的动画集合或该集合为空。", this.gameObject);
        }
    }
}