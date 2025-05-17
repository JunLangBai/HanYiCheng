using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HangulIntorduction : MonoBehaviour
{
    public TextMeshProUGUI intorductionText;
    public string textToDisplay;

    private void Start()
    {
        intorductionText.text = textToDisplay;
    }
}
