// OCR_UIManager.cs

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OCR_UIManager : MonoBehaviour
{
    public static OCR_UIManager Instance;

    [Header("UI Elements")] public TextMeshProUGUI questionText;

    public Image questionImage;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI resultText;
    public Button nextButton;
    public GameObject summaryPanel;
    public RectTransform summaryContent;
    public GameObject summaryItemPrefab;
    public TextMeshProUGUI finalScoreText;

    [Header("Drawing Components")] public DrawingBoard drawingBoard;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeUI();
        LoadQuestion(OCRQuestionManager.Instance.GetCurrentQuestion());

        nextButton.onClick.AddListener(OnNextQuestionClicked);
    }

    private void InitializeUI()
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
            drawingBoard.ClearCanvas();
        else
            Debug.LogError("画板组件未赋值！");
    }

    private void UpdateProgress()
    {
        var current = OCRQuestionManager.Instance.currentQuestionIndex + 1;
        var total = OCRQuestionManager.Instance.questionData.questions.Count;
        progressText.text = $"进度: {current}/{total}";
    }


    public void OnRecognitionComplete(bool success, string result)
    {
        if (!success)
        {
            resultText.text = "识别失败，请重试";
            return;
        }

        var currentQuestion = OCRQuestionManager.Instance.GetCurrentQuestion();
        var isCorrect = OCRQuestionManager.Instance.ValidateAnswer(result, currentQuestion.correctAnswer);

        if (isCorrect)
        {
            resultText.text = $"正确！正确答案：{currentQuestion.correctAnswer}";
            OCRQuestionManager.Instance.SubmitOCRResult(result);

            if (OCRQuestionManager.Instance.HasNextQuestion())
                nextButton.gameObject.SetActive(true);
            else
                ShowSummary();
        }
        else
        {
            resultText.text = $"错误，请重试\n识别结果：{result}";
            OCRQuestionManager.Instance.SubmitOCRResult(result);
        }
    }

    private void OnNextQuestionClicked()
    {
        OCRQuestionManager.Instance.MoveToNextQuestion();
        LoadQuestion(OCRQuestionManager.Instance.GetCurrentQuestion());
    }


    private void ShowSummary()
    {
        summaryPanel.SetActive(true);
    }
    
}