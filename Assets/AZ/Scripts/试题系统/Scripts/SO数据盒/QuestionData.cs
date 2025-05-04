// QuestionData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewQuestionData", menuName = "Question Data")]
public class QuestionData : ScriptableObject
{
    public List<Question> questions;
}

[System.Serializable]
public class Question
{
    public string questionText;
    public Sprite image;
    public AudioClip questionAudio;
    public List<Option> options;
    public int correctAnswerIndex;
}

[System.Serializable]
public class Option
{
    public string text;
    public AudioClip audioClip;
}