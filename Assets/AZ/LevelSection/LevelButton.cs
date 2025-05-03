using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelNameText;
    
    public LevelData LevelData { get; set; }
    
    private Button _button;
    
    private Image _image;
    
    public Color ReturnColor { get; set; }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
        ReturnColor = Color.gray;
    }

    public void Setup(LevelData level, bool isUnlocked)
    {
        LevelData = level;
        _levelNameText.SetText(level.LevelID);
        _button.interactable = isUnlocked;

        if (isUnlocked)
        {
            _button.onClick.AddListener(LoadLevel);
            Debug.Log(level.LevelID);
            ReturnColor = Color.white;
            _image.color = ReturnColor;
        }
        else
        {
            ReturnColor = Color.gray;
            _image.color = ReturnColor;
        }
    }

    public void Unlock()
    {
        _button.interactable = true;
        _button.onClick.AddListener(LoadLevel);
        ReturnColor = Color.white;
        _image.color = ReturnColor;
    }

    public void LoadLevel()
    {
        Debug.Log("Loading Level");
        SceneManager.LoadScene(LevelData.Scene.SceneName, LoadSceneMode.Single);
    }
}