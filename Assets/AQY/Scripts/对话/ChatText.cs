// ChatText.cs
using UnityEngine;

[CreateAssetMenu(menuName = "对话系统/对话文本")]
public class ChatText : ScriptableObject
{
    [TextArea(3, 10)]
    public string content; // 对话内容

    public bool stopText;
    public string[] buttonTexts;
}