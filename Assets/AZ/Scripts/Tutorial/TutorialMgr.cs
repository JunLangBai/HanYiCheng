using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialMgr : MonoBehaviour
{
    [Header("教程步骤")] 
    public CanvasGroup[] tutorialSteps; // 四个教程步骤
    public string targetSceneName = "MainUI";
    public float fadeDuration = 0.5f;

    [Header("文本内容")] 
    public string[] tutorialTexts;
    public TextMeshProUGUI[] dialogTexts;
    
    private int currentStep = -1; // 当前步骤索引（-1表示未开始）
    private bool isLoadingScene;
    private bool isTransitioning;

    private void Awake()
    {
        
    }

    private void Start()
    {
        // 确保所有步骤初始隐藏
        for (int i = 0; i < tutorialSteps.Length; i++)
        {
            SetStepVisible(i, false, true);
        }
        
        // 绑定按钮事件
        GetComponent<Button>().onClick.AddListener(AdvanceTutorial);
    }

    public void AdvanceTutorial()
    {
        if (isLoadingScene || isTransitioning) return;
        
        PlayButtonSound();
        
        if (currentStep < tutorialSteps.Length - 1)
        {
            ShowNextStep();
        }
        else
        {
            EndTutorial();
        }
    }

    private void ShowNextStep()
    {
        int previousStep = currentStep;
        currentStep++;
        
        // 更新按钮文本（如果是最后一步）
        if (currentStep == tutorialSteps.Length - 1)
        {
            GetComponentInChildren<TextMeshProUGUI>().text = "完成";
        }
        
        StartCoroutine(TransitionSteps(previousStep, currentStep));
        UpdateDialog(currentStep);
    }

    private IEnumerator TransitionSteps(int hideIndex, int showIndex)
    {
        isTransitioning = true;
        float timer = 0f;

        // 准备新步骤
        if (showIndex >= 0)
        {
            SetStepVisible(showIndex, true, false);
            tutorialSteps[showIndex].alpha = 0;
        }

        while (timer < fadeDuration)
        {
            float progress = timer / fadeDuration;

            // 淡出旧步骤
            if (hideIndex >= 0)
            {
                tutorialSteps[hideIndex].alpha = 1 - progress;
            }

            // 淡入新步骤
            if (showIndex >= 0)
            {
                tutorialSteps[showIndex].alpha = progress;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 完成状态设置
        if (hideIndex >= 0)
        {
            SetStepVisible(hideIndex, false, true);
        }
        
        if (showIndex >= 0)
        {
            tutorialSteps[showIndex].alpha = 1;
        }
        
        isTransitioning = false;
    }

    private void SetStepVisible(int index, bool visible, bool immediate)
    {
        if (index < 0 || index >= tutorialSteps.Length) return;
        
        CanvasGroup step = tutorialSteps[index];
        step.blocksRaycasts = visible;
        step.interactable = visible;
        
        if (immediate)
        {
            step.alpha = visible ? 1 : 0;
        }
    }

    private void UpdateDialog(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= tutorialTexts.Length) return;
        
        // 隐藏所有对话框文本
        foreach (var text in dialogTexts)
        {
            if (text != null) text.text = "";
        }
        
        // 显示当前步骤的文本
        if (stepIndex < dialogTexts.Length && dialogTexts[stepIndex] != null)
        {
            dialogTexts[stepIndex].text = tutorialTexts[stepIndex];
        }
    }

    private void PlayButtonSound()
    {
        // 简化的音频播放逻辑
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PlayButtonSound();
        }
    }

    private void EndTutorial()
    {
        isLoadingScene = true;
        
        // 保存进度
        GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
        gameData.tutorialClear = true;
        JsonFileManager.SaveToJson(gameData, "GameData.json");
        
        // 加载场景
        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;

        // 等待加载完成
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        asyncLoad.allowSceneActivation = true;
    }
}