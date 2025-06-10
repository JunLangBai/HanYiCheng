using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueEditor;
using UnityEngine.UI;
public class MockTalkMgr : MonoBehaviour
{
    public NPCConversation conversation;
    public NPCConversation[] conversationList;
    public Button[] buttonList;
    public GameObject orange;
    public CanvasGroup canvasGroup;
    
    
    void Start()
    {
        CVControl(true);
        
        // 确保数组长度匹配
        if (buttonList.Length != conversationList.Length)
        {
            Debug.LogError("按钮数量和对话资源数量不匹配！");
            return;
        }

        // 为每个按钮绑定点击事件
        for (int i = 0; i < buttonList.Length; i++)
        {
            // 必须创建局部变量避免闭包问题
            int index = i;
            buttonList[index].onClick.AddListener(() => OnButtonClick(index));
        }
        
        
    }

    void CVControl(bool b)
    {
       canvasGroup.alpha =  b ? 1 : 0;
       canvasGroup.interactable = b;
       canvasGroup.blocksRaycasts = b;
       orange.SetActive(!b);
    }

    // 按钮点击事件处理
    private void OnButtonClick(int index)
    {
        // 检查索引有效性
        if (index >= 0 && index < conversationList.Length)
        {
            conversation = conversationList[index];
            Debug.Log($"已设置对话: {conversation.name} (索引: {index})");
            
            // 这里可以添加触发对话开始的代码
            CVControl(false);
            
            ConversationManager.Instance.StartConversation(conversation);
        }
        else
        {
            Debug.LogError($"无效的索引: {index}");
        }
    }
}
