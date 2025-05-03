using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PersonalCard : MonoBehaviour
{
    
    [Header("头像设置")]
    public Sprite[] avatarSprites; // 所有头像的Sprite数组（在Inspector中赋值）
    public Image currentAvatarImage; // 当前显示的Image组件
    public Image changeImage;
    private int index;

    [Header("名字设置")]
    public TMP_Text userNameText; // 显示名字的TMP组件
    
    [Header("修改")]
    public CanvasGroup editPanel;
    public CanvasGroup nomalPanel;
    
    
    
    // 加载 JSON 数据
    GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");

    private void Start()
    {
        // 初始化：加载保存的数据（如果有）
        LoadUserData();
    }

    // 选择头像的方法（绑定到每个头像按钮的OnClick事件）
    public void SelectAvatar()
    {
        if (index >= 0 && index < avatarSprites.Length)
        {
            index++;
            if (index >= avatarSprites.Length)
            {
                index = 0;
            }
            currentAvatarImage.sprite = avatarSprites[index];
            changeImage.sprite = avatarSprites[index];
            gameData.profilePictureIndex = index;
            JsonFileManager.SaveToJson(gameData, "GameData.json");// 保存选择
        }
    }

    // 打开名字输入框（绑定到"编辑按钮"的OnClick事件）
    public void OpenEditor()
    {
        editPanel.alpha = 1; // 显示输入框面板
        editPanel.interactable = true;
        editPanel.blocksRaycasts = true;
        
        nomalPanel.alpha = 0;
        nomalPanel.interactable = false;
        nomalPanel.blocksRaycasts = false;
    }

    // 提交新名字（绑定到InputField的OnEndEdit事件或确认按钮）
    public void CloseEditor()
    {
        editPanel.alpha = 0; // 隐藏输入框
        editPanel.interactable = false;
        editPanel.blocksRaycasts = false;
        
        nomalPanel.alpha = 1;
        nomalPanel.interactable = true;
        nomalPanel.blocksRaycasts = true;
    }

    // 加载保存的数据
    private void LoadUserData()
    {
        //加载图片下标
        currentAvatarImage.sprite = avatarSprites[gameData.profilePictureIndex];
        changeImage.sprite = avatarSprites[gameData.profilePictureIndex];
        if (!string.IsNullOrWhiteSpace(gameData.username) )
        {
            // 加载名字
            userNameText.text = gameData.username;
        }
        else
        {
            userNameText.text = "HYCer";
        }
    }
}
