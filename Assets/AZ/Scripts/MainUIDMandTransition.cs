using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DMandTransitionM  : MonoBehaviour
{
    [Header("UI组件绑定")]
    public TextMeshProUGUI chatText;  // 对应Chat/welcome文本组件
    public Button continueButton;    // 继续按钮组件

    [Header("对话配置")]
    public string[] dialogueLines = {
        "欢迎来到神秘的新世界！",
        "我是你的向导韩易成",
        "在这个世界你会学到韩语",
        "准备好了吗？",
        "让我们开始冒险吧！"
    };

    [Header("场景设置")]
    [Tooltip("在Build Settings中确认场景顺序")]
    public int targetSceneIndex = 1; // 要转场的场景序号

    private int currentLine = 0;

    void Start()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        UpdateUI();
    }

    void OnContinueClicked()
    {
        if (currentLine < dialogueLines.Length - 1)
        {
            currentLine++;
            UpdateUI();
        }
        else
        {
            // 触发场景转场
            SceneManager.LoadScene(targetSceneIndex);
        }
    }

    void UpdateUI()
    {
        // 更新对话文本
        if (chatText != null && currentLine < dialogueLines.Length)
        {
            chatText.text = dialogueLines[currentLine];
        }

        // 更新按钮文字（根据截图中的"继续"按钮适配）
        TextMeshProUGUI btnText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = IsLastLine ? "开始学习！" : "继续";
        }
    }

    bool IsLastLine => currentLine >= dialogueLines.Length - 1;

    // 场景安全验证（编辑器模式下）
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!SceneExists(targetSceneIndex))
        {
            Debug.LogError($"场景索引{targetSceneIndex}未添加到Build Settings！");
        }
    }

    bool SceneExists(int index)
    {
        return index >= 0 && index < UnityEditor.EditorBuildSettings.scenes.Length;
    }
#endif
}
