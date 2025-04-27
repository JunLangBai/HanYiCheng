using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TextDisplay : MonoBehaviour
{
    // 单例实例
    public static TextDisplay Instance { get; private set; }

    [Header("UI References")] public Button dialogueButton;
    public TextMeshProUGUI dialogueText;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    [Header("Dialogue Configuration")] [SerializeField]
    private List<ChatText> dialogueSequence = new List<ChatText>();

    [SerializeField] private string endSceneName;

    private int _currentDialogueIndex = -1;
    private bool _awaitingChoice;

    void Awake()
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

    void Start()
    {
        InitializeDialogue();
        dialogueButton.onClick.AddListener(ProceedToNextDialogue);
    }

    /// <summary>
    /// 初始化对话系统
    /// </summary>
    public void InitializeDialogue()
    {
        _currentDialogueIndex = -1;
        ProceedToNextDialogue();
    }

    /// <summary>
    /// 推进到下一段对话
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
    /// 处理当前对话项
    /// </summary>
    private void ProcessCurrentDialogue()
    {
        ChatText current = dialogueSequence[_currentDialogueIndex];
        
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
            GlobalTutorialsManager.instance.canNextText = false;
            // 显示选项按钮
            SetupInteractiveButtons();
            PlacementMgr.instance.ShowOptions();
            _awaitingChoice = false;
        }
    }

    private void SetupContinueButton()
    {
        // 获取当前对话数据
        ChatText currentChat = dialogueSequence[_currentDialogueIndex];

        // 创建单个继续按钮
        GameObject button = Instantiate(buttonPrefab, buttonContainer);

        // 设置按钮文本（使用第一个按钮文本或默认"继续"）
        string buttonText = currentChat.buttonTexts != null && currentChat.buttonTexts.Length > 0
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
    /// 创建交互按钮
    /// </summary>
    /// <summary>
    /// 创建选项按钮组
    /// </summary>
    private void SetupInteractiveButtons()
    {
        ChatText currentChat = dialogueSequence[_currentDialogueIndex];

        if (currentChat.buttonTexts == null || currentChat.buttonTexts.Length == 0)
        {
            Debug.LogWarning("对话项配置错误：需要按钮但未配置buttonTexts");
            return;
        }

        foreach (string btnText in currentChat.buttonTexts)
        {
            GameObject button = Instantiate(buttonPrefab, buttonContainer);

            // 设置按钮文本
            TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = btnText;

            // 绑定带参数的点击事件
            Button btnComponent = button.GetComponent<Button>();
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
        /// 处理按钮选择
        /// </summary>
        private void HandleButtonSelection(string selectedOption)
        {
            // 这里可以添加选择后的逻辑处理
            Debug.Log($"Selected option: {selectedOption}");
            GlobalTutorialsManager.instance.ValidateChoice(selectedOption);
        }

        /// <summary>
        /// 清理按钮容器
        /// </summary>
        private void ClearButtonContainer()
        {
            foreach (Transform child in buttonContainer)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// 结束对话流程
        /// </summary>
        private void FinalizeDialogue()
        {
            Debug.Log("Dialogue sequence completed");
            if (!string.IsNullOrEmpty(endSceneName))
            {
                SceneManager.LoadScene(endSceneName);
            }
            else
            {
                dialogueText.text = "Conversation Ended";
                ClearButtonContainer();
            }
        }
    }