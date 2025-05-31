// OCRQuestionManager.cs

using System.Collections.Generic;
using UnityEngine;

public class OCRQuestionManager : MonoBehaviour
{
    public static OCRQuestionManager Instance;
    public OCRQuestionData questionData;

    [Header("不要动")] public int currentQuestionIndex;

    private string lastOCRResult;
    private readonly List<OCRQuestion> wrongQuestions = new();

    private void Awake()
    {
        Instance = this;
        if (questionData == null)
        {
            Debug.LogError("未分配OCRQuestionData资源！");
            return;
        }

        if (questionData.questions == null || questionData.questions.Count == 0)
        {
            Debug.LogError("题目列表为空！");
            return;
        }

        currentQuestionIndex = 0;
    }

    public OCRQuestion GetCurrentQuestion()
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

    public void SubmitOCRResult(string result)
    {
        lastOCRResult = result;
        var current = GetCurrentQuestion();

        if (ValidateAnswer(result, current.correctAnswer))
        {
            MoveToNextQuestion();
            return;
        }

        wrongQuestions.Add(current);
    }

    public bool ValidateAnswer(string ocrResult, string correctAnswer)
    {
        // 标准化比较（去除空格、转小写）
        return ocrResult?.Replace(" ", "").ToLower() == correctAnswer?.Replace(" ", "").ToLower();
    }

    public void MoveToNextQuestion()
    {
        if (currentQuestionIndex < questionData.questions.Count - 1) currentQuestionIndex++;
    }

    public bool IsLastQuestion()
    {
        return currentQuestionIndex >= questionData.questions.Count - 1;
    }

    public List<OCRQuestion> GetWrongQuestions()
    {
        return wrongQuestions;
    }

    public bool HasNextQuestion()
    {
        return currentQuestionIndex < questionData.questions.Count - 1;
    }

    public string GetLastOCRResult()
    {
        return lastOCRResult;
    }
}