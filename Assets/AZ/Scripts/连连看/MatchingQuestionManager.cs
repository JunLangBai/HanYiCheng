using System.Collections.Generic;
using UnityEngine;

public class MatchingQuestionManager : MonoBehaviour
{
    public static MatchingQuestionManager Instance;
    public MatchingQuestionData questionData;
    public int currentQuestionIndex = 0;

    private readonly List<MatchingQuestion> wrongQuestions = new();

    private void Awake()
    {
        Instance = this;
    }

    public MatchingQuestion GetCurrentQuestion()
    {
        if (questionData == null || questionData.questions.Count == 0)
        {
            Debug.LogError("未配置匹配题目数据！");
            return null;
        }

        return questionData.questions[currentQuestionIndex];
    }

    public void SubmitMatchResult(bool correct)
    {
        var current = GetCurrentQuestion();
        if (!correct && current != null)
        {
            wrongQuestions.Add(current);
        }
    }

    public void MoveToNextQuestion()
    {
        if (currentQuestionIndex < questionData.questions.Count - 1)
            currentQuestionIndex++;
    }

    public bool HasNextQuestion()
    {
        return currentQuestionIndex < questionData.questions.Count - 1;
    }

    public List<MatchingQuestion> GetWrongQuestions() => wrongQuestions;
}