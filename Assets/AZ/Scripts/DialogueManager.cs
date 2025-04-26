using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI chatText;  // 对应Chat下的welcome文本
    public Button continueButton;    // 继续按钮

    [Header("对话内容")]
    public string[] dialogueLines = {
        "欢迎来到神秘的新世界！",
        "我是你的向导韩易成",
        "在这个世界你会学到韩语",
        "准备好了吗？",
        "让我们开始冒险吧！"
    };

    private int currentLine = 0;

    void Start()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        UpdateDialogueText();
    }

    void OnContinueClicked()
    {
        currentLine = (currentLine + 1) % dialogueLines.Length;
        UpdateDialogueText();
    }

    void UpdateDialogueText()
    {
        if(chatText != null && currentLine < dialogueLines.Length)
        {
            chatText.text = dialogueLines[currentLine];
        }
    }
}