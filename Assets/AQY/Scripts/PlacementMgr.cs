using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlacementMgr : MonoBehaviour
{
    public static PlacementMgr instance;
    public CanvasGroup onlyTextCanvas;
    public CanvasGroup optionsCanvas;

    public TextMeshProUGUI onlyText;
    public TextMeshProUGUI optionText; 

    private void Awake()
    {
        // 单例初始化
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    
    private void Start()
    {
        if (GlobalTutorialsManager.instance.canNextText)
        {
            onlyTextCanvas.alpha = 1;
            onlyTextCanvas.blocksRaycasts = true;
            
            optionsCanvas.alpha = 0;
            optionsCanvas.blocksRaycasts = false;
        }
        else if(!GlobalTutorialsManager.instance.canNextText)
        {
            onlyTextCanvas.alpha = 0;
            onlyTextCanvas.blocksRaycasts = false;
            
            optionsCanvas.alpha = 1;
            optionsCanvas.blocksRaycasts = true;
        }
    }

    private void Update()
    {
        if (GlobalTutorialsManager.instance.canNextText)
        {
            onlyTextCanvas.alpha = 1;
            onlyTextCanvas.blocksRaycasts = true;
            
            optionsCanvas.alpha = 0;
            optionsCanvas.blocksRaycasts = false;
        }
        else if(!GlobalTutorialsManager.instance.canNextText)
        {
            onlyTextCanvas.alpha = 0;
            onlyTextCanvas.blocksRaycasts = false;
            
            optionsCanvas.alpha = 1;
            optionsCanvas.blocksRaycasts = true;
        }
       
    }

    public void ShowOnlyText()
    {
        TextDisplay.Instance.dialogueText = onlyText;
    }

    public void ShowOptions()
    {
        TextDisplay.Instance.dialogueText = optionText;
    }
    
}
