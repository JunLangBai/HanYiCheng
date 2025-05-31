// QuestionData.cs

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuestionData", menuName = "Question Data")]
public class QuestionData : ScriptableObject
{
    public List<Question> questions;
}

[Serializable]
public class Question
{
    public string questionText;
    public Sprite image;
    public AudioClip questionAudio;
    public List<Option> options;
    public int correctAnswerIndex;
}

[Serializable]
public class Option
{
    public string text;
    public AudioClip audioClip;
}