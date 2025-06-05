using UnityEngine;
using UnityEngine.UI;

public class MatchingGame_Horizontal : MonoBehaviour
{
    [System.Serializable]
    public class MatchPair
    {
        public string korean;   // 韩文
        public string pinyin;   // 拼音
    }

    [Header("配对设置")]
    public MatchPair[] correctPairs;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.blue;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public float resetDelay = 0.8f;
    public GameObject rowPrefab; // 水平行预制体

    [Header("UI引用")]
    public Transform rowsContainer; // 行容器
    public Button checkButton;

    private int selectedRowIndex = -1;
    private Button selectedKoreanBtn;
    private Button selectedPinyinBtn;

    void Start()
    {
        InitializeHorizontalRows();
        checkButton.onClick.AddListener(CheckResults);
    }

    void InitializeHorizontalRows()
    {
        // 清除已有内容
        foreach (Transform child in rowsContainer)
        {
            Destroy(child.gameObject);
        }

        // 创建水平行
        for (int i = 0; i < correctPairs.Length; i++)
        {
            // 创建一行
            GameObject row = Instantiate(rowPrefab, rowsContainer);
            row.name = $"Row_{i}";
            
            // 获取按钮引用
            Button koreanBtn = row.transform.Find("KoreanButton").GetComponent<Button>();
            Button pinyinBtn = row.transform.Find("PinyinButton").GetComponent<Button>();
            
            // 设置文本
            koreanBtn.GetComponentInChildren<Text>().text = correctPairs[i].korean;
            pinyinBtn.GetComponentInChildren<Text>().text = correctPairs[i].pinyin;
            
            // 添加点击事件
            int index = i; // 闭包变量
            koreanBtn.onClick.AddListener(() => OnKoreanButtonClick(index, koreanBtn));
            pinyinBtn.onClick.AddListener(() => OnPinyinButtonClick(index, pinyinBtn));
            
            // 重置颜色
            ResetButtonColor(koreanBtn);
            ResetButtonColor(pinyinBtn);
        }
    }

    void OnKoreanButtonClick(int rowIndex, Button btn)
    {
        // 取消之前的选择
        if (selectedKoreanBtn != null)
        {
            ResetButtonColor(selectedKoreanBtn);
        }
        
        // 选择当前韩文按钮
        SetButtonColor(btn, selectedColor);
        selectedRowIndex = rowIndex;
        selectedKoreanBtn = btn;
        
        // 如果有拼音选择，立即检查
        if (selectedPinyinBtn != null)
        {
            CheckPair();
        }
    }

    void OnPinyinButtonClick(int rowIndex, Button btn)
    {
        // 取消之前的选择
        if (selectedPinyinBtn != null)
        {
            ResetButtonColor(selectedPinyinBtn);
        }
        
        // 选择当前拼音按钮
        SetButtonColor(btn, selectedColor);
        selectedRowIndex = rowIndex;
        selectedPinyinBtn = btn;
        
        // 如果有韩文选择，立即检查
        if (selectedKoreanBtn != null)
        {
            CheckPair();
        }
    }

    void CheckPair()
    {
        if (selectedRowIndex == -1 || selectedKoreanBtn == null || selectedPinyinBtn == null)
            return;
        
        int currentIndex = selectedRowIndex;
        
        if (selectedKoreanBtn.GetComponentInChildren<Text>().text == correctPairs[currentIndex].korean &&
            selectedPinyinBtn.GetComponentInChildren<Text>().text == correctPairs[currentIndex].pinyin)
        {
            // 配对正确
            SetButtonColor(selectedKoreanBtn, correctColor);
            SetButtonColor(selectedPinyinBtn, correctColor);
            
            // 禁用正确配对的按钮
            selectedKoreanBtn.interactable = false;
            selectedPinyinBtn.interactable = false;
            
            // 重置选择状态
            ClearSelection();
        }
        else
        {
            // 配对错误
            SetButtonColor(selectedKoreanBtn, wrongColor);
            SetButtonColor(selectedPinyinBtn, wrongColor);
            
            // 保存引用用于重置
            Button tempKorean = selectedKoreanBtn;
            Button tempPinyin = selectedPinyinBtn;
            
            // 重置选择状态
            ClearSelection();
            
            // 延迟后重置颜色
            StartCoroutine(ResetWrongPairAfterDelay(tempKorean, tempPinyin));
        }
    }

    void ClearSelection()
    {
        selectedRowIndex = -1;
        selectedKoreanBtn = null;
        selectedPinyinBtn = null;
    }

    System.Collections.IEnumerator ResetWrongPairAfterDelay(Button koreanBtn, Button pinyinBtn)
    {
        yield return new WaitForSeconds(resetDelay);
        
        ResetButtonColor(koreanBtn);
        ResetButtonColor(pinyinBtn);
    }

    void CheckResults()
    {
        // 检查所有配对是否完成
        int completedPairs = 0;
        foreach (Transform row in rowsContainer)
        {
            Button koreanBtn = row.Find("KoreanButton").GetComponent<Button>();
            Button pinyinBtn = row.Find("PinyinButton").GetComponent<Button>();
            
            if (!koreanBtn.interactable && !pinyinBtn.interactable)
            {
                completedPairs++;
            }
        }
        
        Debug.Log($"完成配对: {completedPairs}/{correctPairs.Length}");
    }

    // 辅助函数
    void SetButtonColor(Button btn, Color color) => btn.image.color = color;
    void ResetButtonColor(Button btn) => btn.image.color = normalColor;
}