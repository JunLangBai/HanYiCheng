// OCR_UIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OCR_UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questionText;
    public Image questionImage;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI resultText;
    public Button nextButton;
    public GameObject summaryPanel;
    public RectTransform summaryContent;
    public GameObject summaryItemPrefab;
    public TextMeshProUGUI finalScoreText;
    
    [Header("Drawing Components")]
    public DrawingBoard drawingBoard;
    
    public static OCR_UIManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeUI();
        LoadQuestion(OCRQuestionManager.Instance.GetCurrentQuestion());
        
        nextButton.onClick.AddListener(OnNextQuestionClicked);
    }

    void InitializeUI()
    {
        nextButton.gameObject.SetActive(false);
        summaryPanel.SetActive(false);
        UpdateProgress();
    }

    public void LoadQuestion(OCRQuestion question)
    {
        if (question == null) return;

        ClearDrawingBoard();
        resultText.text = "";
        nextButton.gameObject.SetActive(false);
        
        // 设置题目内容
        questionText.text = question.questionText;
        questionImage.sprite = question.referenceImage;

        // 更新进度
        UpdateProgress();
    }

    private void ClearDrawingBoard()
    {
 if (drawingBoard != null)
            {
                drawingBoard.ClearCanvas();
            }
            else
            {
                Debug.LogError("画板组件未赋值！");
            }
        
    }

    void UpdateProgress()
    {
        int current = OCRQuestionManager.Instance.currentQuestionIndex + 1;
        int total = OCRQuestionManager.Instance.questionData.questions.Count;
        progressText.text = $"进度: {current}/{total}";
    }
    

    public void OnRecognitionComplete(bool success, string result)
    {
        if (!success)
        {
            resultText.text = "识别失败，请重试";
            return;
        }

        OCRQuestion currentQuestion = OCRQuestionManager.Instance.GetCurrentQuestion();
        bool isCorrect = OCRQuestionManager.Instance.ValidateAnswer(result, currentQuestion.correctAnswer);

        if (isCorrect)
        {
            resultText.text = $"正确！正确答案：{currentQuestion.correctAnswer}";
            OCRQuestionManager.Instance.SubmitOCRResult(result);
            
            if (OCRQuestionManager.Instance.HasNextQuestion())
            {
                nextButton.gameObject.SetActive(true);
            }
            else
            {
                ShowSummary();
            }
        }
        else
        {
            resultText.text = $"错误，请重试\n识别结果：{result}";
            OCRQuestionManager.Instance.SubmitOCRResult(result);
        }
    }

    void OnNextQuestionClicked()
    {
        OCRQuestionManager.Instance.MoveToNextQuestion();
        LoadQuestion(OCRQuestionManager.Instance.GetCurrentQuestion());
    }
    

    void ShowSummary()
    {
        summaryPanel.SetActive(true);
        foreach (var question in OCRQuestionManager.Instance.GetWrongQuestions())
        {
            var item = Instantiate(summaryItemPrefab, summaryContent);
            item.GetComponent<TextMeshProUGUI>().text = 
                $"题目: {question.questionText}\n" +
                $"正确答案: {question.correctAnswer}\n" +
                $"你的答案: {OCRQuestionManager.Instance.GetLastOCRResult()}";
        }

        CalculateFinalScore();
    }

    void CalculateFinalScore()
    {
        int total = OCRQuestionManager.Instance.questionData.questions.Count;
        int correct = total - OCRQuestionManager.Instance.GetWrongQuestions().Count;
        float score = (float)correct / total;

        string grade = score switch
        {
            >= 0.9f => "A+",
            >= 0.8f => "A",
            >= 0.7f => "B",
            >= 0.6f => "C",
            _ => "D"
        };

        finalScoreText.text = $"最终成绩: {grade} ({correct}/{total})";
    }
}
