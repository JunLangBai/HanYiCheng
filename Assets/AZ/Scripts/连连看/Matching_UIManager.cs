using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Matching_UIManager : MonoBehaviour
{
    public static Matching_UIManager Instance;

    [Header("UI 元素")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI resultText;
    public Button nextButton;
    public CanvasGroup summaryPanel;

    [Header("按钮容器")]
    public Transform questionArea;
    public Transform answerArea;

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
        LoadQuestion(MatchingQuestionManager.Instance.GetCurrentQuestion());
        summaryPanel.gameObject.SetActive(false);
    }

    public void LoadQuestion(MatchingQuestion question)
    {
        resultText.text = "";
        nextButton.gameObject.SetActive(false);
        questionText.text = question.questionText;
        correctPairs = question.correctPairs;

        idMap.Clear();
        InitButtons(questionArea, true);
        InitButtons(answerArea, false);

        UpdateProgress();
    }

    private void InitButtons(Transform area, bool isQuestionSide)
    {
        List<string> usedIDs = new();
        foreach (Transform t in area)
        {
            var btn = t.GetComponent<Button>();
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (!btn || !txt) continue;

            btn.image.color = normalColor;
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();

            MatchPair pair = null;

            if (isQuestionSide)
            {
                // 找到一个未使用的 questionID
                pair = correctPairs.Find(p => !usedIDs.Contains(p.questionID));
                if (pair != null)
                {
                    txt.text = pair.questionText;
                    idMap[btn] = pair.questionID;
                    usedIDs.Add(pair.questionID);
                    btn.onClick.AddListener(() => OnQClick(btn));
                }
            }
            else
            {
                pair = correctPairs.Find(p => !usedIDs.Contains(p.answerID));
                if (pair != null)
                {
                    txt.text = pair.answerText;
                    idMap[btn] = pair.answerID;
                    usedIDs.Add(pair.answerID);
                    btn.onClick.AddListener(() => OnAClick(btn));
                }
            }
        }
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

        bool matched = correctPairs.Exists(pair => pair.questionID == qID && pair.answerID == aID);

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

    private void ShowSummary()
    {
        summaryPanel.gameObject.SetActive(true);
    }

    private System.Collections.IEnumerator ResetAfterDelay(Button q, Button a)
    {
        yield return new WaitForSeconds(0.5f);
        q.image.color = normalColor;
        a.image.color = normalColor;
    }

    private bool AllMatched()
    {
        foreach (var btn in idMap.Keys)
        {
            if (btn.interactable) return false;
        }

        return true;
    }

    private void OnNextQuestionClicked()
    {
        MatchingQuestionManager.Instance.MoveToNextQuestion();
        LoadQuestion(MatchingQuestionManager.Instance.GetCurrentQuestion());
    }

    private void UpdateProgress()
    {
        var idx = MatchingQuestionManager.Instance.currentQuestionIndex + 1;
        var total = MatchingQuestionManager.Instance.questionData.questions.Count;
        progressText.text = $"进度: {idx}/{total}";
    }
}
