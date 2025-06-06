using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PronunciationMatchingGame : MonoBehaviour
{
    [Header("UI 引用")]
    public Transform pronunciationQ;  // 上方的按钮容器
    public Transform pronunciationA;  // 下方的按钮容器
    public Button completeButton;      // 完成按钮
    
    [Header("颜色设置")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.blue;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public float resetDelay = 0.5f;   // 错误配对的延迟重置时间

    // 正确答案列表（在Inspector中设置）
    public List<MatchPair> correctPairs = new List<MatchPair>();
    
    [System.Serializable]
    public class MatchPair
    {
        public string questionID;  // 匹配上方的ID
        public string answerID;    // 匹配下方的ID
    }

    private Dictionary<Button, string> buttonIDs = new Dictionary<Button, string>();
    private Button selectedQButton; // 当前选中的上方按钮
    private Button selectedAButton; // 当前选中的下方按钮
    
    void Start()
    {
        // 初始化按钮点击事件
        InitializeButtons();
        
        // 完成按钮事件
        if (completeButton != null)
        {
            completeButton.onClick.AddListener(CheckAllMatches);
        }
        
        // 重置所有按钮颜色
        ResetAllButtons();
    }

    void InitializeButtons()
    {
        // 添加上方按钮的事件
        foreach (Transform child in pronunciationQ)
        {
            Button btn = child.GetComponent<Button>();
            if (btn != null)
            {
                // 使用按钮名字作为ID（需在编辑器中设置）
                buttonIDs[btn] = btn.name;
                btn.onClick.AddListener(() => OnQButtonClick(btn));
            }
        }
        
        // 添加下方按钮的事件
        foreach (Transform child in pronunciationA)
        {
            Button btn = child.GetComponent<Button>();
            if (btn != null)
            {
                // 使用按钮名字作为ID（需在编辑器中设置）
                buttonIDs[btn] = btn.name;
                btn.onClick.AddListener(() => OnAButtonClick(btn));
            }
        }
    }

    void OnQButtonClick(Button clickedButton)
    {
        // 如果点击了已选中的按钮，则取消选择
        if (selectedQButton == clickedButton)
        {
            SetButtonColor(clickedButton, normalColor);
            selectedQButton = null;
            return;
        }
        
        // 取消之前选中的按钮
        if (selectedQButton != null)
        {
            SetButtonColor(selectedQButton, normalColor);
        }
        
        // 选中新按钮
        SetButtonColor(clickedButton, selectedColor);
        selectedQButton = clickedButton;
        
        // 如果有配对的A按钮，检查匹配
        if (selectedAButton != null)
        {
            CheckPairMatch();
        }
    }

    void OnAButtonClick(Button clickedButton)
    {
        // 如果点击了已选中的按钮，则取消选择
        if (selectedAButton == clickedButton)
        {
            SetButtonColor(clickedButton, normalColor);
            selectedAButton = null;
            return;
        }
        
        // 取消之前选中的按钮
        if (selectedAButton != null)
        {
            SetButtonColor(selectedAButton, normalColor);
        }
        
        // 选中新按钮
        SetButtonColor(clickedButton, selectedColor);
        selectedAButton = clickedButton;
        
        // 如果有配对的Q按钮，检查匹配
        if (selectedQButton != null)
        {
            CheckPairMatch();
        }
    }

    void CheckPairMatch()
    {
        if (selectedQButton == null || selectedAButton == null) return;
        
        string qID = buttonIDs[selectedQButton];
        string aID = buttonIDs[selectedAButton];
        
        bool isMatch = false;
        
        // 检查是否是正确配对
        foreach (var pair in correctPairs)
        {
            if (pair.questionID == qID && pair.answerID == aID)
            {
                isMatch = true;
                break;
            }
        }
        
        if (isMatch)
        {
            // 匹配正确
            SetButtonColor(selectedQButton, correctColor);
            SetButtonColor(selectedAButton, correctColor);
            
            // 禁用已匹配的按钮
            selectedQButton.interactable = false;
            selectedAButton.interactable = false;
            
            // 清除选择状态
            selectedQButton = null;
            selectedAButton = null;
            
            // 检查是否全部完成
            CheckAllMatches();
        }
        else
        {
            // 匹配错误
            SetButtonColor(selectedQButton, wrongColor);
            SetButtonColor(selectedAButton, wrongColor);
            
            // 保存当前选择用于延迟重置
            Button tempQ = selectedQButton;
            Button tempA = selectedAButton;
            
            // 清除选择状态
            selectedQButton = null;
            selectedAButton = null;
            
            // 延迟重置按钮颜色
            StartCoroutine(ResetButtonAfterDelay(tempQ, tempA));
        }
    }

    System.Collections.IEnumerator ResetButtonAfterDelay(Button qBtn, Button aBtn)
    {
        yield return new WaitForSeconds(resetDelay);
        
        if (qBtn != null)
        {
            SetButtonColor(qBtn, normalColor);
        }
        if (aBtn != null)
        {
            SetButtonColor(aBtn, normalColor);
        }
    }

    void CheckAllMatches()
    {
        bool allMatched = true;
        
        // 检查是否所有按钮都已被匹配
        foreach (var pair in correctPairs)
        {
            bool pairMatched = false;
            
            // 找到对应的按钮
            foreach (var btn in buttonIDs)
            {
                if (btn.Value == pair.questionID && !btn.Key.interactable)
                {
                    pairMatched = true;
                    break;
                }
            }
            
            if (!pairMatched)
            {
                allMatched = false;
                break;
            }
        }
        
        // 如果全部完成
        if (allMatched)
        {
            Debug.Log("所有配对已完成！");
            // 这里可以触发下一题或完成逻辑
        }
    }

    void SetButtonColor(Button button, Color color)
    {
        // 确保按钮有Image组件
        if (button != null && button.image != null)
        {
            button.image.color = color;
        }
    }

    void ResetAllButtons()
    {
        foreach (var btn in buttonIDs)
        {
            SetButtonColor(btn.Key, normalColor);
        }
    }
}