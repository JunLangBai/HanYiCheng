using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialButtonSystem : MonoBehaviour
{
    [Header("UI Components")] [SerializeField]
    private GameObject mainContinueBtn; // 初始的继续按钮

    [SerializeField] private Transform buttonContainer; // 按钮父对象
    [SerializeField] private GameObject buttonPrefab; // 按钮预制体
    [SerializeField] private TextMeshProUGUI dialogText; // 对话框文字

    [Header("Dialog Content")] [SerializeField]
    private  List<DialogStep> tutorialSteps = new();

    private readonly Stack<GameObject> currentButtons = new();

    private int currentStep = -1;

    // 单例实例
    public static TutorialButtonSystem Instance { get; private set; }

    private void Awake()
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

    private void Start()
    {
        InitializeTutorial();
    }

    private void InitializeTutorial()
    {
        mainContinueBtn.SetActive(true);
        mainContinueBtn.GetComponent<Button>().onClick.AddListener(StartFirstStep);
    }

    private void StartFirstStep()
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

    private void UpdateDialog()
    {
        dialogText.text = tutorialSteps[currentStep].description;
    }

    private void CreateStepButtons()
    {
        foreach (var btn in tutorialSteps[currentStep].buttons)
        {
            var newBtn = Instantiate(buttonPrefab, buttonContainer);
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

    private void ClearCurrentButtons()
    {
        while (currentButtons.Count > 0)
        {
            var btn = currentButtons.Pop();
            if (btn != null) Destroy(btn);
        }
    }

    private void EndTutorial()
    {
        dialogText.text = "教学完成！";
        mainContinueBtn.SetActive(false);
    }
}

[Serializable]
public class DialogStep
{
    [TextArea(3, 5)] public string description;

    public List<TutorialButton> buttons = new();
}

[Serializable]
public class TutorialButton
{
    public string buttonText;
    public UnityEvent onClickEvent;
}