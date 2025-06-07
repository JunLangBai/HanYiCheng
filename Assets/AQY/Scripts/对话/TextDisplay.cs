using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TextDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Button dialogueButton;
    public TextMeshProUGUI dialogueText;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    [Header("Dialogue Configuration")]
    [SerializeField] private List<ChatText> dialogueSequence = new();

    [Header("UI Fade")]
    public CanvasGroup canvasGroup;
    public float fadeDuration = 1f;
    public float delayBetweenFades = 0.5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip defaultClickClip;

    [SerializeField] private string endSceneName;

    private bool _awaitingChoice;
    private int _currentDialogueIndex = -1;
    private bool isDoExcessive;

    public static TextDisplay Instance { get; private set; }

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
        InitializeDialogue();
        dialogueButton.onClick.AddListener(ProceedToNextDialogue);
    }

    public void InitializeDialogue()
    {
        _currentDialogueIndex = -1;
        ProceedToNextDialogue();
    }

    public void ProceedToNextDialogue()
    {
        ClearButtonContainer();

        if (_currentDialogueIndex >= dialogueSequence.Count - 1)
        {
            FinalizeDialogue();
            return;
        }

        _currentDialogueIndex++;
        ProcessCurrentDialogue();
    }

    private void ProcessCurrentDialogue()
    {
        var current = dialogueSequence[_currentDialogueIndex];

        if (current.onlyText == false)
        {
            dialogueText = PlacementMgr.instance.optionText;
            dialogueText.text = current.content;
        }
        else
        {
            dialogueText = PlacementMgr.instance.onlyText;
            dialogueText.text = current.content;
        }

        ClearButtonContainer();

        if (current.onlyText)
        {
            GlobalTutorialsManager.instance.canNextText = true;
            SetupContinueButton();
            PlacementMgr.instance.ShowOnlyText();
            _awaitingChoice = true;
        }
        else
        {
            if (isDoExcessive == false)
            {
                isDoExcessive = true;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                StartCoroutine(FadeInOutSequence());
            }
            else
            {
                GlobalTutorialsManager.instance.canNextText = false;
                SetupInteractiveButtons();
                PlacementMgr.instance.ShowOptions();
                _awaitingChoice = false;
            }
        }
    }

    private void SetupContinueButton()
    {
        var currentChat = dialogueSequence[_currentDialogueIndex];

        var button = Instantiate(buttonPrefab, buttonContainer);

        var buttonText = currentChat.buttonTexts != null && currentChat.buttonTexts.Length > 0
            ? currentChat.buttonTexts[0]
            : "继续";

        button.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;

        button.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (!_awaitingChoice)
            {
                PlayAudio(currentChat.buttonAudioClip ?? defaultClickClip);
                GlobalTutorialsManager.instance.canNextText = true;
                ProceedToNextDialogue();
            }
        });
    }

    private void SetupInteractiveButtons()
    {
        var currentChat = dialogueSequence[_currentDialogueIndex];

        if (currentChat.buttonTexts == null || currentChat.buttonTexts.Length == 0)
        {
            Debug.LogWarning("对话项配置错误：需要按钮但未配置 buttonTexts");
            return;
        }

        foreach (var btnText in currentChat.buttonTexts)
        {
            var button = Instantiate(buttonPrefab, buttonContainer);
            button.gameObject.AddComponent<GlobalButtonClickListener>();

            var textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = btnText;

            var btnComponent = button.GetComponent<Button>();
            btnComponent.onClick.AddListener(() =>
            {
                PlayAudio(currentChat.buttonAudioClip ?? defaultClickClip);
                HandleButtonClick(btnText);
            });
        }
    }

    private void HandleButtonClick(string selectedText)
    {
        Debug.Log($"选择选项：{selectedText}");
        _awaitingChoice = false;
        ProceedToNextDialogue();
    }

    private void ClearButtonContainer()
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void FinalizeDialogue()
    {
        Debug.Log("Dialogue sequence completed");

        dialogueText.text = "接下来开始真正的冒险吧！";
        ClearButtonContainer();

        var button = Instantiate(buttonPrefab, buttonContainer);
        button.GetComponentInChildren<TextMeshProUGUI>().text = "开始冒险";
        var btnComponent = button.GetComponent<Button>();
        btnComponent.onClick.AddListener(SceneLoaded);
    }

    public void SceneLoaded()
    {
        var gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
        gameData.placementClear = true;
        JsonFileManager.SaveToJson(gameData, "GameData.json");
        
        SceneManager.LoadScene(endSceneName);
    }

    private IEnumerator FadeInOutSequence()
    {

        yield return StartCoroutine(Fade(0f, 1f));

        if (delayBetweenFades > 0f)
            yield return new WaitForSeconds(delayBetweenFades);

        GlobalTutorialsManager.instance.canNextText = false;
        SetupInteractiveButtons();
        PlacementMgr.instance.ShowOptions();
        _awaitingChoice = false;

        yield return StartCoroutine(Fade(1f, 0f));

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private IEnumerator Fade(float start, float end)
    {
        var elapsedTime = 0f;
        canvasGroup.alpha = start;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = end;
        canvasGroup.interactable = end > 0f;
        canvasGroup.blocksRaycasts = end > 0f;
    }

    private void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
