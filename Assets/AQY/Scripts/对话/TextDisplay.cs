using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TextDisplay : MonoBehaviour
{
    // 单例实例
    public static TextDisplay Instance { get; private set; }

    [Header("UI References")]
    public Button dialogueButton;
    public TextMeshProUGUI dialogueText;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    [Header("Dialogue Configuration")]
    [SerializeField] private List<ChatText> dialogueSequence = new List<ChatText>();
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
        
        // 根据交互需求处理UI
        if (current.stopText)
        {
            PlacementMgr.instance.ShowOnlyText();
            _awaitingChoice = true;
            SetupInteractiveButtons(current.buttonTexts);
        }
        else
        {
            dialogueText = PlacementMgr.instance.optionText;
            SetupContinueButton();
            _awaitingChoice = false;
        }
        
        // 更新对话文本
        dialogueText.text = current.content;

    }

    /// <summary>
    /// 设置继续按钮
    /// </summary>
    private void SetupContinueButton()
    {
        GlobalTutorialsManager.instance.canNextText = dialogueSequence[_currentDialogueIndex].stopText;
        GameObject button = Instantiate(buttonPrefab, buttonContainer);
        button.GetComponentInChildren<TextMeshProUGUI>().text = dialogueSequence[_currentDialogueIndex].content;
        button.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (!_awaitingChoice) ProceedToNextDialogue();
        });
    }

    /// <summary>
    /// 创建交互按钮
    /// </summary>
    private void SetupInteractiveButtons(string[] options)
    {
        if (options == null || options.Length == 0) return;

        foreach (string option in options)
        {
            Debug.Log($"正在创建选项按钮：{option}");
            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = option;
            
            Button buttonComponent = buttonObj.GetComponent<Button>();
            buttonComponent.onClick.AddListener(() =>
            {
                HandleButtonSelection(option);
                _awaitingChoice = false;
                ProceedToNextDialogue();
            });
        }
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