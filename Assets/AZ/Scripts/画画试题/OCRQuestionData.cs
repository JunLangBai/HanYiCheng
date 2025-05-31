// OCRQuestionData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewOCRQuestionData", menuName = "OCR Question Data")]
public class OCRQuestionData : ScriptableObject
{
    public List<OCRQuestion> questions;
}

[System.Serializable]
public class OCRQuestion
{
    public string questionText;
    public Sprite referenceImage;
    public string correctAnswer; // OCR期望匹配的正确文本
}