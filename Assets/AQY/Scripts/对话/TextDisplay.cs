using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TextDisplay : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI dialogueText;      // 显示对话的UI文本
    public Button nextButton;      // 下一步按钮

    [Header("对话数据")]
    public List<ChatText> dialogueList = new List<ChatText>(); // 对话列表
    
    [Header("结束对话后跳转的场景")]
    public string targetScene;
    
    [Header("按钮显示")]
    public Transform buttonParent; // 用于放置按钮的父对象（在Inspector中指定）
    public GameObject buttonPrefab; // 按钮预制体（需要带Button和TextMeshProUGUI组件）

    private int currentIndex = -1; // 当前对话索引

    void Start()
    {
        ShowNextDialogue();
        // 绑定按钮点击事件
        nextButton.onClick.AddListener(ShowNextDialogue);
    }

    public void StartDialogue()
    {
        currentIndex = -1;
        dialogueText.gameObject.SetActive(true);
        ShowNextDialogue();
    }

    public void ShowNextDialogue()
    {
        if (GlobalTutorialsManager.instance.canNextText)
        {
            currentIndex++;
            if(currentIndex < dialogueList.Count)
            {
                // 更新对话内容
                dialogueText.text = dialogueList[currentIndex].content;
                GlobalTutorialsManager.instance.canNextText = dialogueList[currentIndex].stopText;
            }
            else
            {
                // 对话结束
                EndDialogue();
            }

        }

        else if (!GlobalTutorialsManager.instance.canNextText)
        {
            // 清除所有旧按钮
            foreach (Transform child in buttonParent)
            {
                Destroy(child.gameObject);
            }

            // 确保当前对话有效
            if (currentIndex >= 0 && currentIndex < dialogueList.Count)
            {
                ChatText currentChat = dialogueList[currentIndex];
            
                // 创建新按钮
                if (currentChat.buttonTexts != null)
                {
                    foreach (string btnText in currentChat.buttonTexts)
                    {
                        GameObject newButton = Instantiate(buttonPrefab, buttonParent);
                        TextMeshProUGUI textComponent = newButton.GetComponentInChildren<TextMeshProUGUI>();
                        if (textComponent != null)
                        {
                            textComponent.text = btnText;
                        }
                    
                        Button buttonComponent = newButton.GetComponent<Button>();
                        buttonComponent.onClick.AddListener(() =>
                        {
                            GlobalTutorialsManager.instance.CanNextText();
                        });
                    }
                }
            }
        }
    }

    void EndDialogue()
    {
        if (!string.IsNullOrEmpty(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }

    }
}
