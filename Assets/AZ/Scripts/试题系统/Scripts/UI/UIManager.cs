// UIManager.cs
// 负责管理所有UI元素的交互和显示逻辑
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    // ========== UI元素引用配置 ==========
    [Header("UI Elements")]
    public TextMeshProUGUI questionText;          // 题目文本显示组件
    public TextMeshProUGUI nowChose;   //当前选项
    public Image imagePanel;           //显示图片
    public Transform optionsContainer; // 选项按钮的父级容器
    public GameObject optionPrefab;    // 选项按钮预制体
    public Button actionButton;        // 操作按钮（提交/下一题）
    public TextMeshProUGUI actionButtonText;      // 操作按钮的文本组件
    public GameObject summaryPanel;    // 错题总结面板
    public Transform summaryContent;   // 错题列表的父级容器
    public GameObject summaryItemPrefab; // 错题项预制体
    public TextMeshProUGUI scoreText;  //评分（躺赢狗）

    // ========== 音频相关 ==========
    [Header("Audio")]
    public AudioSource audioSource;    // 通用音频播放组件

    // ========== 运行时变量 ==========
    private List<OptionButton> currentOptions = new List<OptionButton>(); // 当前显示的选项按钮
    private bool isShowingResult;      // 是否处于显示答案结果的状态

    private float score;
    
    public static UIManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // 初始化按钮监听
        actionButton.onClick.AddListener(OnActionButtonClicked);
        nowChose.text = "当前选项:";
        actionButtonText.text = "完成";
        // 加载第一题
        LoadQuestion(QuestionManager.Instance.GetCurrentQuestion());
    }

    /// <summary>
    /// 加载并显示题目
    /// </summary>
    /// <param name="question">要加载的题目数据</param>
    void LoadQuestion(Question question)
    {
        Debug.Log("LoadQuestion"+question);
        if (question == null)
        {
            Debug.LogError("尝试加载空题目！");
            ShowSummary();
            return;
        }
        
        if (question.questionText != null)
        {
            // 设置题目文本
            questionText.text = question.questionText;
        }
        else
        {
            questionText.text = "";
        }

        if (question.image != null)
        {
            imagePanel.sprite = question.image;
        }
        else
        {
            Debug.Log("null image");
        }

        // 清理旧选项
        ClearOptions();
        
        // 动态生成新选项
        for (int i = 0; i < question.options.Count; i++)
        {
            // 实例化选项按钮
            var option = Instantiate(optionPrefab, optionsContainer).GetComponent<OptionButton>();
            // 初始化选项数据
            option.Initialize(i, question.options[i]);
            currentOptions.Add(option);
        }

        // 播放题目音频（如果有）
        PlayAudio(question.questionAudio);
    }

    /// <summary>
    /// 清理所有选项按钮
    /// </summary>
    void ClearOptions()
    {
        foreach (var option in currentOptions)
        {
            Destroy(option.gameObject);
        }
        currentOptions.Clear();
        nowChose.text = "当前选择:";
    }

    /// <summary>
    /// 显示答案结果（正确/错误高亮）
    /// </summary>
    /// <param name="correctIndex">正确答案的索引</param>
    public void ShowAnswerResult(int correctIndex)
    {
        foreach (var option in currentOptions)
        {
            // 设置选项状态颜色：
            // - 正确答案显示绿色
            // - 用户选择的错误答案显示红色
            // - 其他选项保持默认
            option.SetState(option.Index == correctIndex ? 
                OptionState.Correct : 
                (option.Index == QuestionManager.Instance.selectedAnswerIndex ? 
                    OptionState.Wrong : 
                    OptionState.Normal));
        }
    }

    /// <summary>
    /// 显示错题总结面板
    /// </summary>
    public void ShowSummary()
    {
        summaryPanel.SetActive(true);
        // 遍历所有错题生成总结项
        foreach (var question in QuestionManager.Instance.GetWrongQuestions())
        {
            var item = Instantiate(summaryItemPrefab, summaryContent);
            // 设置文本格式：题目 + 正确答案
            item.GetComponent<TextMeshProUGUI>().text = $"问题: {question.questionText}\n正确答案: {question.options[question.correctAnswerIndex].text}";
        }

        // 正确代码
        var assessment = GetScore();
        if (assessment >= 0.8f)
        {
            scoreText.text = "A";
        }
        else if (assessment >= 0.6f)
        {
            scoreText.text = "B";
        }
        else if (assessment >= 0.4f)
        {
            scoreText.text = "C";
        }
        else
        {
            scoreText.text = "D";
        }
        
    }
    
    /// <summary>
    /// 操作按钮点击事件处理
    /// </summary>
    void OnActionButtonClicked()
    {
        if (isShowingResult)
        {
            // 进入下一题或显示总结
            if (QuestionManager.Instance.IsLastQuestion())
            {
                ShowSummary();
            }
            else
            {
                QuestionManager.Instance.MoveToNextQuestion();
                LoadQuestion(QuestionManager.Instance.GetCurrentQuestion());
            }
        }
        else
        {
            /// 在提交前打印选中内容
            Debug.Log(GetSelectedOptionContent());
        
            if (QuestionManager.Instance.selectedAnswerIndex == -1)
            {
                Debug.Log("请先选择答案！");
                return;
            }
        
            QuestionManager.Instance.SubmitAnswer();
            ShowAnswerResult(QuestionManager.Instance.GetCurrentQuestion().correctAnswerIndex);
        }
        
        // 切换按钮状态
        isShowingResult = !isShowingResult;
        // 更新按钮文字：提交 -> 下一题 -> 总结
        actionButtonText.text = isShowingResult ? "下一题" : "完成";
    }

    /// <summary>
    /// 通用音频播放方法
    /// </summary>
    /// <param name="clip">要播放的音频片段</param>
    public void PlayAudio(AudioClip clip)
    {
        if (clip == null) return;
        
        // 停止当前音频并播放新音频
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
    
    public string GetSelectedOptionContent()
    {
        var option = QuestionManager.Instance.GetSelectedOption();
        if (option != null)
        {
            return $"选中内容：{option.text}";
        }
        return "未选择任何选项";
    }
    
    // 新增更新方法
    public void UpdateSelectionInfo()
    {
        var option = QuestionManager.Instance.GetSelectedOption();
        nowChose.text = option != null ? 
            $"当前选择：{option.text}" : 
            "请选择答案";
    }

    public float GetScore()
    {
        int totalQuestions = QuestionManager.Instance.questionData.questions.Count;
        int correctCount = totalQuestions - QuestionManager.Instance.GetWrongQuestions().Count;
        score = (float)correctCount / totalQuestions;
        return score;
    }
}