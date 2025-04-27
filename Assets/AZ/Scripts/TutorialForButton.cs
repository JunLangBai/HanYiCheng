using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialButtonSystem : MonoBehaviour
{
    // 单例实例
    public static TutorialButtonSystem Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private GameObject mainContinueBtn; // 初始的继续按钮
    [SerializeField] private Transform buttonContainer;  // 按钮父对象
    [SerializeField] private GameObject buttonPrefab;    // 按钮预制体
    [SerializeField] private TextMeshProUGUI dialogText; // 对话框文字

    [Header("Dialog Content")]
    [SerializeField] private List<DialogStep> tutorialSteps = new List<DialogStep>();

    private int currentStep = -1;
    private readonly Stack<GameObject> currentButtons = new Stack<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeTutorial();
    }

    void InitializeTutorial()
    {
        mainContinueBtn.SetActive(true);
        mainContinueBtn.GetComponent<Button>().onClick.AddListener(StartFirstStep);
    }

    void StartFirstStep()
    {
        mainContinueBtn.SetActive(false);
        ShowNextStep();
    }

    public void ShowNextStep()
    {
        ClearCurrentButtons();
        currentStep++;

        if (currentStep >= tutorialSteps.Count)
        {
            EndTutorial();
            return;
        }

        UpdateDialog();
        CreateStepButtons();
    }

    void UpdateDialog()
    {
        dialogText.text = tutorialSteps[currentStep].description;
    }

    void CreateStepButtons()
    {
        foreach (var btn in tutorialSteps[currentStep].buttons)
        {
            GameObject newBtn = Instantiate(buttonPrefab, buttonContainer);
            newBtn.GetComponentInChildren<TextMeshProUGUI>().text = btn.buttonText;
            
            // 添加事件监听
            newBtn.GetComponent<Button>().onClick.AddListener(() => 
            {
                btn.onClickEvent?.Invoke();
                ShowNextStep();
            });

            currentButtons.Push(newBtn);
        }
    }

    void ClearCurrentButtons()
    {
        while (currentButtons.Count > 0)
        {
            var btn = currentButtons.Pop();
            if(btn != null) Destroy(btn);
        }
    }

    void EndTutorial()
    {
        dialogText.text = "教学完成！";
        mainContinueBtn.SetActive(false);
    }
}

[System.Serializable]
public class DialogStep
{
    [TextArea(3,5)]
    public string description;
    public List<TutorialButton> buttons = new List<TutorialButton>();
}

[System.Serializable]
public class TutorialButton
{
    public string buttonText;
    public UnityEngine.Events.UnityEvent onClickEvent;
}