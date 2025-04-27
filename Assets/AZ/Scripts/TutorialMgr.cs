using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialMgr : MonoBehaviour, IPointerDownHandler
{
    [Header("教程")]
    public CanvasGroup[] tutorial; // 假设这里有 4 张图片（索引 0-3）
    public float fadeDuration = 1f; // 渐变持续时间
    private int currentIndex; // 当前图片的索引

    [Header("文本")]
    public string[] tutorialText;
    private int currentStep = -1;
    public TextMeshProUGUI[] dialogText;
    
    private void Start()
    {
        // 初始化图片透明度
        for (var i = 0; i < tutorial.Length; i++)
        {
            tutorial[i].alpha = 0;
            tutorial[i].interactable = false;
            tutorial[i].blocksRaycasts = false;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("233143");
        if (currentIndex < tutorial.Length)
        {
            ShowNextStep();
            ShowNextImage();
        }
    }

    
    // 下一张图片按钮
    public void ShowNextImage()
    {
        // 检查是否可以继续前进
        if (currentIndex >= tutorial.Length)
        {
            Debug.Log("已经到达最后一张图片！");
            return; // 如果已经是最后一张图片，则不再执行
        }

        var nextIndex = currentIndex;
        StartCoroutine(FadeOutIn(currentIndex - 1, nextIndex));
    }

    // 淡出当前图片，淡入下一张图片
    private IEnumerator FadeOutIn(int fromIndex, int toIndex)
    {
        var timer = 0f;

        // 渐渐淡出当前图片并淡入下一张图片
        while (timer < fadeDuration)
        {
            var alpha = timer / fadeDuration;
            if (fromIndex >= 0)
            {
                tutorial[fromIndex].alpha = 1 - alpha; // 渐渐变为透明
            }

            if (toIndex <= tutorial.Length)
            {
                tutorial[toIndex].alpha = alpha; // 渐渐变为可见
            }
            else
            {
                break;
            }
            timer += Time.unscaledDeltaTime; // 使用 Time.unscaledDeltaTime 不受暂停影响
            yield return null;
        }
    }
    
    public void ShowNextStep()
    {
        currentStep++;

        if (currentStep > tutorialText.Length)
        {
            EndTutorial();
            return;
        }

        if (currentIndex < tutorial.Length)
        {
            UpdateDialog();
        }
    }

    private void EndTutorial()
    {
        throw new NotImplementedException();
    }

    void UpdateDialog()
    {
        if (currentStep < 0 || currentStep >= tutorialText.Length || currentStep >= dialogText.Length)
        {
            return;
        }
        dialogText[currentStep].text = tutorialText[currentStep];
    }
}