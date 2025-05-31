using TMPro;
using UnityEngine;

public class HangulIntorduction : MonoBehaviour
{
    public TextMeshProUGUI intorductionText;
    public string textToDisplay;

    private void Start()
    {
        intorductionText.text = textToDisplay;
    }
}