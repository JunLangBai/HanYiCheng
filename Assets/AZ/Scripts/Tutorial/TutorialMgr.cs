using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialMgr : MonoBehaviour
{
    [Header("教程步骤")] 
    public CanvasGroup[] tutorialSteps;
    public string targetSceneName = "MainUI";
    public float fadeDuration = 0.5f;

    [Header("文本内容")] 
    public string[] tutorialTexts;
    public TextMeshProUGUI[] dialogTexts;

    [Header("音频设置")]
    public AudioSource audioSource;              // 播放音频用的 AudioSource（拖入主摄像机或空物体）
    public AudioClip startAudioClip;             // 场景刚进入时播放
    public AudioClip[] stepAudioClips;           // 每个步骤的音频（顺序要和步骤一致）

    private int currentStep = -1;
    private bool isLoadingScene;
    private bool isTransitioning;

    private void Start()
    {
        // 隐藏所有步骤
        for (int i = 0; i < tutorialSteps.Length; i++)
        {
            SetStepVisible(i, false, true);
        }

        // 播放入场音频
        if (startAudioClip != null && audioSource != null)
        {
            audioSource.clip = startAudioClip;
            audioSource.Play();
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

        // 修改按钮文字（最后一步）
        if (currentStep == tutorialSteps.Length - 1)
        {
            GetComponentInChildren<TextMeshProUGUI>().text = "完成";
        }

        StartCoroutine(TransitionSteps(previousStep, currentStep));
        UpdateDialog(currentStep);
        PlayStepAudio(currentStep);
    }

    private IEnumerator TransitionSteps(int hideIndex, int showIndex)
    {
        isTransitioning = true;
        float timer = 0f;

        if (showIndex >= 0)
        {
            SetStepVisible(showIndex, true, false);
            tutorialSteps[showIndex].alpha = 0;
        }

        while (timer < fadeDuration)
        {
            float progress = timer / fadeDuration;

            if (hideIndex >= 0)
                tutorialSteps[hideIndex].alpha = 1 - progress;

            if (showIndex >= 0)
                tutorialSteps[showIndex].alpha = progress;

            timer += Time.deltaTime;
            yield return null;
        }

        if (hideIndex >= 0)
            SetStepVisible(hideIndex, false, true);

        if (showIndex >= 0)
            tutorialSteps[showIndex].alpha = 1;

        isTransitioning = false;
    }

    private void SetStepVisible(int index, bool visible, bool immediate)
    {
        if (index < 0 || index >= tutorialSteps.Length) return;

        CanvasGroup step = tutorialSteps[index];
        step.blocksRaycasts = visible;
        step.interactable = visible;

        if (immediate)
            step.alpha = visible ? 1 : 0;
    }

    private void UpdateDialog(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= tutorialTexts.Length) return;

        foreach (var text in dialogTexts)
        {
            if (text != null) text.text = "";
        }

        if (stepIndex < dialogTexts.Length && dialogTexts[stepIndex] != null)
        {
            dialogTexts[stepIndex].text = tutorialTexts[stepIndex];
        }
    }

    private void PlayStepAudio(int index)
    {
        if (audioSource != null && index >= 0 && index < stepAudioClips.Length && stepAudioClips[index] != null)
        {
            audioSource.clip = stepAudioClips[index];
            audioSource.Play();
        }
    }

    private void PlayButtonSound()
    {
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PlayButtonSound();
        }
    }

    private void EndTutorial()
    {
        isLoadingScene = true;

        GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
        gameData.tutorialClear = true;
        JsonFileManager.SaveToJson(gameData, "GameData.json");

        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
    }
}
