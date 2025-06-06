using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Matching_UIManager : MonoBehaviour
{
    public static Matching_UIManager Instance;

    [Header("UI 元素")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI resultText;
    public Button nextButton;
    public CanvasGroup summaryPanel;

    [Header("按钮容器 & 预制体")]
    public Transform questionArea;    // 上排容器
    public Transform answerArea;      // 下排容器
    public Button buttonPrefab;       // 挂上你的 Button 预制体

    [Header("颜色设置")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.cyan;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    private Button selectedQ, selectedA;
    private Dictionary<Button, string> idMap = new();
    private List<MatchPair> correctPairs;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        nextButton.onClick.AddListener(OnNextQuestionClicked);
        summaryPanel.gameObject.SetActive(false);
        LoadQuestion(MatchingQuestionManager.Instance.GetCurrentQuestion());
    }

    public void LoadQuestion(MatchingQuestion question)
    {
        // 重置 UI
        resultText.text = "";
        nextButton.gameObject.SetActive(false);
        questionText.text = question.questionText;
        correctPairs = question.correctPairs;

        // 清空旧按钮
        foreach (Transform t in questionArea) Destroy(t.gameObject);
        foreach (Transform t in answerArea) Destroy(t.gameObject);
        idMap.Clear();
        selectedQ = selectedA = null;

        // 动态生成按钮并随机打乱显示顺序
        // 1. 收集所有 questionText 和 answerText
        var questionList = new List<(string id, string text)>();
        var answerList   = new List<(string id, string text)>();
        foreach (var p in correctPairs)
        {
            questionList.Add((p.questionID, p.questionText));
            answerList.Add((p.answerID, p.answerText));
        }
        Shuffle(questionList);
        Shuffle(answerList);

        // 2. 在容器中实例化
        foreach (var (id, text) in questionList)
        {
            var btn = Instantiate(buttonPrefab, questionArea);
            btn.image.color = normalColor;
            btn.interactable = true;
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            label.text = text;
            idMap[btn] = id;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnQClick(btn));
        }
        foreach (var (id, text) in answerList)
        {
            var btn = Instantiate(buttonPrefab, answerArea);
            btn.image.color = normalColor;
            btn.interactable = true;
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            label.text = text;
            idMap[btn] = id;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnAClick(btn));
        }

        UpdateProgress();
    }

    private void OnQClick(Button btn)
    {
        if (selectedQ == btn)
        {
            btn.image.color = normalColor;
            selectedQ = null;
        }
        else
        {
            if (selectedQ) selectedQ.image.color = normalColor;
            selectedQ = btn;
            btn.image.color = selectedColor;
        }
        if (selectedA) TryMatch();
    }

    private void OnAClick(Button btn)
    {
        if (selectedA == btn)
        {
            btn.image.color = normalColor;
            selectedA = null;
        }
        else
        {
            if (selectedA) selectedA.image.color = normalColor;
            selectedA = btn;
            btn.image.color = selectedColor;
        }
        if (selectedQ) TryMatch();
    }

    private void TryMatch()
    {
        string qID = idMap[selectedQ];
        string aID = idMap[selectedA];
        bool matched = correctPairs.Exists(p => p.questionID == qID && p.answerID == aID);

        if (matched)
        {
            selectedQ.image.color = correctColor;
            selectedA.image.color = correctColor;
            selectedQ.interactable = false;
            selectedA.interactable = false;
            selectedQ = selectedA = null;

            if (AllMatched())
            {
                resultText.text = "全部匹配成功！";
                if (MatchingQuestionManager.Instance.HasNextQuestion())
                    nextButton.gameObject.SetActive(true);
                else
                    ShowSummary();
            }
        }
        else
        {
            selectedQ.image.color = wrongColor;
            selectedA.image.color = wrongColor;
            StartCoroutine(ResetAfterDelay(selectedQ, selectedA));
            selectedQ = selectedA = null;
        }
    }

    private System.Collections.IEnumerator ResetAfterDelay(Button q, Button a)
    {
        yield return new WaitForSeconds(0.5f);
        q.image.color = normalColor;
        a.image.color = normalColor;
    }

    private bool AllMatched()
    {
        foreach (var kv in idMap)
            if (kv.Key.interactable)
                return false;
        return true;
    }

    private void OnNextQuestionClicked()
    {
        MatchingQuestionManager.Instance.MoveToNextQuestion();
        LoadQuestion(MatchingQuestionManager.Instance.GetCurrentQuestion());
    }

    private void ShowSummary()
    {
        summaryPanel.gameObject.SetActive(true);
    }

    private void UpdateProgress()
    {
        int idx = MatchingQuestionManager.Instance.currentQuestionIndex + 1;
        int total = MatchingQuestionManager.Instance.questionData.questions.Count;
        progressText.text = $"进度: {idx}/{total}";
    }

    // Fisher–Yates shuffle
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
