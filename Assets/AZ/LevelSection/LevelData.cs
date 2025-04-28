using UnityEngine;

[CreateAssetMenu(menuName = "Level Data/Levels", fileName = "New Level")]

public class LevelData : ScriptableObject
{
    [Header("Level Stats")]
    public string LevelID;
    [Tooltip("For starting Levels")] public bool ISUnlockedByDefault;
    public SceneField Scene;
    
    [Header("Level Display Information")]
    public string LevelName;
    
    public GameObject LevelButtonObj { get; set; }
    
}
