using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMatchingQuestionData", menuName = "Matching Question Data")]
public class MatchingQuestionData : ScriptableObject
{
    public List<MatchingQuestion> questions;
}

[Serializable]
public class MatchingQuestion
{
    public string questionText;
    public List<MatchPair> correctPairs;
}

[Serializable]
public class MatchPair
{
    public string questionID;
    public string questionText;
    public string answerID;
    public string answerText;
}
