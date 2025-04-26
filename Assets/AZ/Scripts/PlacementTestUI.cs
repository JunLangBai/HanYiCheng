using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlacementTestUI : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI chatText;  // 对应Chat下的welcome文本
    public Button continueButton;    // 继续按钮

    [Header("对话内容")]
    public string[] dialogueLines = {
        "哦，对了！冒险前来几个快问快答吧！",
        
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
