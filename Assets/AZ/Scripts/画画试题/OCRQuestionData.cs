// OCRQuestionData.cs

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOCRQuestionData", menuName = "OCR Question Data")]
public class OCRQuestionData : ScriptableObject
{
    public List<OCRQuestion> questions;
}

[Serializable]
public class OCRQuestion
{
    public string questionText;
    public Sprite referenceImage;
    public AudioClip questionAudio;
    public string correctAnswer; // OCR期望匹配的正确文本
}