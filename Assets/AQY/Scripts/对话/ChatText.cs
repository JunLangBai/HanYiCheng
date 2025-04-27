// ChatText.cs
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "对话系统/对话文本")]
public class ChatText : ScriptableObject
{
    [TextArea(3, 10)]
    public string content; // 对话内容

    [FormerlySerializedAs("stopText")] public bool onlyText;
    public string[] buttonTexts;
}