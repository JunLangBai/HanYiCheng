using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TutorialMgr : MonoBehaviour, IPointerDownHandler
{
    [Header("教程")]
    public CanvasGroup[] tutorial; // 假设这里有 4 张图片（索引 0-3）
    public string targetSceneName = "MainUI";
    public float fadeDuration = 1f; // 渐变持续时间
    private int currentIndex = -1; // 当前图片的索引，初始化为 -1 表示尚未开始

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
        Debug.Log("点击触发");
        AudioManager.Instance.PlayButtonSound();
        if (currentIndex < tutorial.Length - 1) // 确保还有下一张图片可以显示
        {
            ShowNextStep();
            ShowNextImage();
        }
        else
        {
            Debug.Log("已经到达最后一张图片！");
            EndTutorial();
        }
    }

    // 下一张图片按钮
    public void ShowNextImage()
    {
        // 更新当前索引
        currentIndex++;

        // 检查是否可以继续前进
        if (currentIndex >= tutorial.Length)
        {
            Debug.Log("已经到达最后一张图片！");
            currentIndex--; // 回退索引以避免越界
            return;
        }

        var fromIndex = currentIndex - 1; // 当前显示的图片索引
        var toIndex = currentIndex;       // 下一张图片索引

        StartCoroutine(FadeOutIn(fromIndex, toIndex));
    }

    // 淡出当前图片，淡入下一张图片
    private IEnumerator FadeOutIn(int fromIndex, int toIndex)
    {
        var timer = 0f;

        // 渐渐淡出当前图片并淡入下一张图片
        while (timer < fadeDuration)
        {
            var alpha = timer / fadeDuration;

            // 淡出上一张图片
            if (fromIndex >= 0 && fromIndex < tutorial.Length)
            {
                tutorial[fromIndex].alpha = 1 - alpha; // 渐渐变为透明
            }

            // 淡入下一张图片
            if (toIndex >= 0 && toIndex < tutorial.Length)
            {
                tutorial[toIndex].alpha = alpha; // 渐渐变为可见
            }

            timer += Time.unscaledDeltaTime; // 使用 Time.unscaledDeltaTime 不受暂停影响
            yield return null;
        }

        // 确保最终状态正确
        if (fromIndex >= 0 && fromIndex < tutorial.Length)
        {
            tutorial[fromIndex].alpha = 0;
        }

        if (toIndex >= 0 && toIndex < tutorial.Length)
        {
            tutorial[toIndex].alpha = 1;
        }
    }

    public void ShowNextStep()
    {
        currentStep++;

        if (currentStep >= tutorialText.Length)
        {
            EndTutorial();
            return;
        }

        UpdateDialog();
    }

    private void EndTutorial()
    {
        Debug.Log("教程结束");
        GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
        gameData.tutorialClear = true;
        // 保存修改后的数据
        JsonFileManager.SaveToJson(gameData, "GameData.json");
        // 可以在这里添加结束逻辑，例如隐藏所有 UI 或跳转到主菜单
        StartCoroutine(LoadSceneDirectly());
    }

    IEnumerator LoadSceneDirectly()
    {
        // 直接异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;

        // 等待加载进度完成（保留最后的激活权限）
        while(!asyncLoad.isDone)
        {
            if(asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
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
