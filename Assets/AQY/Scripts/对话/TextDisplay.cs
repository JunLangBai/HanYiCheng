using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TextDisplay : MonoBehaviour
{
    [Header("UI References")] public Button dialogueButton;
    public TextMeshProUGUI dialogueText;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    [Header("Dialogue Configuration")] [SerializeField]
    private List<ChatText> dialogueSequence = new();

    [Header("UI Fade")]
    //过度
    public CanvasGroup canvasGroup; // 目标CanvasGroup

    public float fadeDuration = 1f; // 过渡时间
    public float delayBetweenFades = 0.5f; // 淡入和淡出之间的延迟时间

    [SerializeField] private string endSceneName;
    private bool _awaitingChoice;

    private int _currentDialogueIndex = -1;

    //是否进行过转场
    private bool isDoExcessive;

    // 单例实例
    public static TextDisplay Instance { get; private set; }

    private void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeDialogue();
        dialogueButton.onClick.AddListener(ProceedToNextDialogue);
    }

    /// <summary>
    ///     初始化对话系统
    /// </summary>
    public void InitializeDialogue()
    {
        _currentDialogueIndex = -1;
        ProceedToNextDialogue();
    }

    /// <summary>
    ///     推进到下一段对话
    /// </summary>
    public void ProceedToNextDialogue()
    {
        // 清理旧UI元素
        ClearButtonContainer();

        // 终止条件检查
        if (_currentDialogueIndex >= dialogueSequence.Count - 1)
        {
            FinalizeDialogue();
            return;
        }

        _currentDialogueIndex++;
        ProcessCurrentDialogue();
    }

    /// <summary>
    ///     处理当前对话项
    /// </summary>
    private void ProcessCurrentDialogue()
    {
        var current = dialogueSequence[_currentDialogueIndex];

        if (current.onlyText == false)
        {
            dialogueText = PlacementMgr.instance.optionText;
            dialogueText.text = current.content;
        }
        else
        {
            dialogueText = PlacementMgr.instance.optionText;
            dialogueText = PlacementMgr.instance.onlyText;
            dialogueText.text = current.content;
        }


        // 清除旧按钮
        ClearButtonContainer();

        if (current.onlyText)
        {
            GlobalTutorialsManager.instance.canNextText = true;
            // 需要暂停等待继续按钮
            SetupContinueButton();
            PlacementMgr.instance.ShowOnlyText();
            _awaitingChoice = true;
        }
        else
        {
            if (isDoExcessive == false)
            {
                isDoExcessive = true;
                // 启动时执行先淡入再淡出
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                StartCoroutine(FadeInOutSequence());
            }
            else
            {
                GlobalTutorialsManager.instance.canNextText = false;
                // 显示选项按钮
                SetupInteractiveButtons();
                PlacementMgr.instance.ShowOptions();
                _awaitingChoice = false;
            }
        }
    }

    private void SetupContinueButton()
    {
        // 获取当前对话数据
        var currentChat = dialogueSequence[_currentDialogueIndex];

        // 创建单个继续按钮
        var button = Instantiate(buttonPrefab, buttonContainer);

        // 设置按钮文本（使用第一个按钮文本或默认"继续"）
        var buttonText = currentChat.buttonTexts != null && currentChat.buttonTexts.Length > 0
            ? currentChat.buttonTexts[0]
            : "继续";

        button.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;

        // 绑定点击事件
        button.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (!_awaitingChoice)
            {
                GlobalTutorialsManager.instance.canNextText = true;
                ProceedToNextDialogue();
            }
        });
    }

    /// <summary>
    ///     创建交互按钮
    /// </summary>
    /// <summary>
    ///     创建选项按钮组
    /// </summary>
    private void SetupInteractiveButtons()
    {
        var currentChat = dialogueSequence[_currentDialogueIndex];

        if (currentChat.buttonTexts == null || currentChat.buttonTexts.Length == 0)
        {
            Debug.LogWarning("对话项配置错误：需要按钮但未配置buttonTexts");
            return;
        }

        foreach (var btnText in currentChat.buttonTexts)
        {
            var button = Instantiate(buttonPrefab, buttonContainer);
            button.gameObject.AddComponent<GlobalButtonClickListener>();

            // 设置按钮文本
            var textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = btnText;

            // 绑定带参数的点击事件
            var btnComponent = button.GetComponent<Button>();
            btnComponent.onClick.AddListener(() => { HandleButtonClick(btnText); });
            
        }
    }

    // <summary>
    /// 处理按钮点击事件
    /// </summary>
    private void HandleButtonClick(string selectedText)
    {
        Debug.Log($"选择选项：{selectedText}");
        _awaitingChoice = false;
        ProceedToNextDialogue();
    }

    /// <summary>
    ///     处理按钮选择
    /// </summary>
    private void HandleButtonSelection(string selectedOption)
    {
        // 这里可以添加选择后的逻辑处理
        Debug.Log($"Selected option: {selectedOption}");
        GlobalTutorialsManager.instance.ValidateChoice(selectedOption);
    }

    /// <summary>
    ///     清理按钮容器
    /// </summary>
    private void ClearButtonContainer()
    {
        foreach (Transform child in buttonContainer) Destroy(child.gameObject);
    }

    /// <summary>
    ///     结束对话流程
    /// </summary>
    private void FinalizeDialogue()
    {
        Debug.Log("Dialogue sequence completed");
        if (!string.IsNullOrEmpty(endSceneName))
        {
            dialogueText.text = "接下来开始真正的冒险吧！";
            ClearButtonContainer();
            // 创建单个继续按钮
            var button = Instantiate(buttonPrefab, buttonContainer);

            var buttonText = "开始冒险";
            button.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
            var btnComponent = button.GetComponent<Button>();
            btnComponent.onClick.AddListener(SceneLoaded);
        }
        else
        {
            dialogueText.text = "接下来开始真正的冒险吧！";
            ClearButtonContainer();
            // 创建单个继续按钮
            var button = Instantiate(buttonPrefab, buttonContainer);

            var buttonText = "开始冒险";
            button.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
            var btnComponent = button.GetComponent<Button>();
        }
    }

    public void SceneLoaded()
    {
         // 加载 JSON 数据
         GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
         gameData.placementClear = true;
         // 保存修改后的数据
         JsonFileManager.SaveToJson(gameData, "GameData.json");
         SceneManager.LoadScene(endSceneName);
    }

    /// <summary>
    ///     先淡入再淡出的完整序列
    /// </summary>
    private IEnumerator FadeInOutSequence()
    {
        // 第一步：淡入
        yield return StartCoroutine(Fade(0f, 1f));

        // 可选：在淡入和淡出之间插入延迟
        if (delayBetweenFades > 0f) yield return new WaitForSeconds(delayBetweenFades);

        GlobalTutorialsManager.instance.canNextText = false;
        // 显示选项按钮
        SetupInteractiveButtons();
        PlacementMgr.instance.ShowOptions();
        _awaitingChoice = false;

        // 第二步：淡出
        yield return StartCoroutine(Fade(1f, 0f));
        
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    /// <summary>
    ///     通用的透明度过渡协程
    /// </summary>
    /// <param name="start">起始透明度</param>
    /// <param name="end">目标透明度</param>
    /// <returns></returns>
    private IEnumerator Fade(float start, float end)
    {
        var elapsedTime = 0f;
        canvasGroup.alpha = start;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsedTime / fadeDuration);
            yield return null; // 等待下一帧
        }

        // 确保最终值精确
        canvasGroup.alpha = end;

        // 根据透明度设置交互状态
        canvasGroup.interactable = end > 0f;
        canvasGroup.blocksRaycasts = end > 0f;
    }
}