using System.Collections;
using System.Collections.Generic;
using System.Linq;  // 用于打乱顺序
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

    [Header("按钮预制体")]
    public Button questionButtonPrefab;
    public Button answerButtonPrefab;

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
        resultText.text = "";
        nextButton.gameObject.SetActive(false);
        questionText.text = question.questionText;
        correctPairs = question.correctPairs;

        // 清理旧按钮
        ClearButtons(questionArea);
        ClearButtons(answerArea);

        idMap.Clear();

        CreateButtons(questionArea, correctPairs, true);
        CreateButtons(answerArea, correctPairs, false);

        UpdateProgress();
    }

    private void ClearButtons(Transform area)
    {
        for (int i = area.childCount - 1; i >= 0; i--)
        {
            Destroy(area.GetChild(i).gameObject);
        }
    }

    private void CreateButtons(Transform area, List<MatchPair> pairs, bool isQuestionSide)
    {
        // 先复制一份列表并随机打乱顺序
        List<MatchPair> listToUse = pairs.OrderBy(x => Random.value).ToList();

        foreach (var pair in listToUse)
        {
            Button btn = Instantiate(isQuestionSide ? questionButtonPrefab : answerButtonPrefab, area);
            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();

            btn.image.color = normalColor;
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();

            if (isQuestionSide)
            {
                txt.text = pair.questionText;
                idMap[btn] = pair.questionID;
                btn.onClick.AddListener(() => OnQClick(btn));
            }
            else
            {
                txt.text = pair.answerText;
                idMap[btn] = pair.answerID;
                btn.onClick.AddListener(() => OnAClick(btn));
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

    private IEnumerator ResetAfterDelay(Button q, Button a)
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
        int idx = MatchingQuestionManager.Instance.currentQuestionIndex + 1;
        int total = MatchingQuestionManager.Instance.questionData.questions.Count;
        progressText.text = $"进度: {idx}/{total}";
    }
}
