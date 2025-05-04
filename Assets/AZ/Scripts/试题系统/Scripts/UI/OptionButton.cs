// OptionButton.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI optionText;
    [SerializeField] private Image highlight;

    public int Index { get; private set; }
    private Option optionData;

    public void Initialize(int index, Option data)
    {
        Index = index;
        optionData = data;
        optionText.text = data.text;
        
        GetComponent<Button>().onClick.AddListener(() =>
        {
            // 传递完整选项数据到GameManager
            QuestionManager.Instance.SelectOption(index, data);
            UIManager.Instance.PlayAudio(data.audioClip);
            UIManager.Instance.UpdateSelectionInfo(); // 新增
        });
    }

    public void SetState(OptionState state)
    {
        highlight.color = state switch
        {
            OptionState.Correct => Color.green,
            OptionState.Wrong => Color.red,
            _ => Color.clear
        };
    }
}

public enum OptionState
{
    Normal,
    Correct,
    Wrong
}