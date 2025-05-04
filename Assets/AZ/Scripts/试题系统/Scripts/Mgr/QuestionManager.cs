// GameManager.cs
using UnityEngine;
using System.Collections.Generic;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager Instance;
    public QuestionData questionData;
    
    private int currentQuestionIndex;
    private List<Question> wrongQuestions = new List<Question>();
    public int selectedAnswerIndex = -3;
    // 新增字段存储完整选项数据
    private Option selectedOptionData;


    void Awake()
    {
        Instance = this;
        if (questionData == null)
        {
            Debug.LogError("未分配QuestionData资源！");
            return;
        }
    
        if (questionData.questions == null || questionData.questions.Count == 0)
        {
            Debug.LogError("题目列表为空！");
            return;
        }
        currentQuestionIndex = 0;
    }
    
    public Question GetCurrentQuestion()
    {
        if (questionData == null || questionData.questions == null)
        {
            Debug.LogError("未配置题目数据或题目列表为空！");
            return null;
        }
    
        if (currentQuestionIndex < 0 || currentQuestionIndex >= questionData.questions.Count)
        {
            Debug.LogError($"无效的题目索引：{currentQuestionIndex}，总题数：{questionData.questions.Count}");
            return null;
        }
    
        return questionData.questions[currentQuestionIndex];
    }

    public void SubmitAnswer()
    {
        var current = GetCurrentQuestion();
        if (selectedAnswerIndex != current.correctAnswerIndex)
        {
            wrongQuestions.Add(current);
        }
    }

    public void MoveToNextQuestion()
    {
        currentQuestionIndex++;
        selectedAnswerIndex = -1;
    }

    public bool IsLastQuestion()
    {
        return currentQuestionIndex >= questionData.questions.Count - 1;
    }

    public List<Question> GetWrongQuestions()
    {
        return wrongQuestions;
    }
    
    public Option GetSelectedOption()
    {
        return selectedOptionData;
    }

    // 修改选项选择方法
    public void SelectOption(int index, Option data)
    {
        selectedAnswerIndex = index;
        selectedOptionData = data;
    }
}