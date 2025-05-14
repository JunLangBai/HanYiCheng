using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RandomTextDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Button clickButton;
    public TextMeshProUGUI displayText;
    public CanvasGroup textGroup;
    public float displayDuration;

    [Header("Text Settings")]
    public List<string> originalTexts = new List<string>();

    private List<string> availableTexts = new List<string>();

    private Coroutine hideCoroutine;
    private void Start()
    {
        // 初始化按钮点击事件
        clickButton.onClick.AddListener(OnClickButton);
        
        // 初始化可用文本列表
        ResetAvailableTexts();
        displayText.text = "";
    }

    private void OnClickButton()
    {
        // 当可用文本为空时重置列表
        if (availableTexts.Count == 0)
        {
            ResetAvailableTexts();
        }
        
        // 停止之前的隐藏协程
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // 随机选择文本
        int randomIndex = Random.Range(0, availableTexts.Count);
        string selectedText = availableTexts[randomIndex];
        
        // 显示文本
        textGroup.alpha = 1f;
        displayText.text = selectedText;
        
        // 移除已选文本
        availableTexts.RemoveAt(randomIndex);
        
        // 启动新的隐藏协程
        hideCoroutine = StartCoroutine(HideTextAfterDelay());
    }

    private void ResetAvailableTexts()
    {
        // 创建新列表（避免引用问题）
        availableTexts = new List<string>(originalTexts);
        
        // 如果需要更随机的排序，可以添加洗牌算法
        ShuffleList(availableTexts);
    }

    // 可选：洗牌算法让随机更彻底
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    private IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        textGroup.alpha = 0f; // 清空文本内容
    }
}