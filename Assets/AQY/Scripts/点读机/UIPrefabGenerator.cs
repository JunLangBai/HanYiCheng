using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using TMPro;

public class UIPrefabGenerator : MonoBehaviour
{
    public enum GenerationMode
    {
        Sequential,
        Random
    }

    [Header("UI Settings")]
    public RectTransform parentPanel;  // UI父容器
    public GameObject uiPrefab;       // UI预制体

    [Header("Resources")]
    public List<Sprite> sprites = new List<Sprite>();
    public List<AudioClip> audioClips = new List<AudioClip>();
    public List<string> texts = new List<string>(); // 新增文本列表

    [Header("Audio")]
    public AudioSource audioPlayer;   // 音频播放器

    [Header("Generation Mode")]
    public GenerationMode mode = GenerationMode.Sequential;

    [Header("Position Settings")]
    public Vector2 spawnPosition = Vector2.zero;

    private int currentIndex;

    private void Start()
    {
        // 使用三个资源列表的最小长度
        int totalToGenerate = Mathf.Min(
            Mathf.Min(sprites.Count, audioClips.Count),
            texts.Count);
        
        for (int i = 0; i < totalToGenerate; i++)
        {
            GenerateUIPrefab();
        }
    }
    public void GenerateUIPrefab()
    {
        if (!ValidateUIComponents()) return;

        int index = GetNextIndex();
        GameObject instance = CreateUIInstance();
        SetupUIImage(instance, index);
        SetupText(instance, index); // 新增文本设置
        SetupButton(instance, index);
    }
    private bool ValidateUIComponents()
    {
        if (uiPrefab == null)
        {
            Debug.LogError("UI Prefab is not assigned!");
            return false;
        }

        if (parentPanel == null)
        {
            Debug.LogError("Parent panel is not assigned!");
            return false;
        }

        if (sprites.Count == 0 || audioClips.Count == 0)
        {
            Debug.LogError("Sprite or AudioClip list is empty!");
            return false;
        }

        if (texts.Count == 0)
        {
            Debug.LogError("Text list is empty!");
            return false;
        }

        return true;
    }

    private int GetNextIndex()
    {
        // 获取三个列表的最小长度作为最大索引
        int maxIndex = Mathf.Min(
            Mathf.Min(sprites.Count, audioClips.Count),
            texts.Count);
        
        if (mode == GenerationMode.Sequential)
        {
            int index = currentIndex % maxIndex;
            currentIndex = (currentIndex + 1) % maxIndex;
            return index;
        }
        
        return Random.Range(0, maxIndex);
    }

    private GameObject CreateUIInstance()
    {
        GameObject instance = Instantiate(uiPrefab, parentPanel);
        RectTransform rt = instance.GetComponent<RectTransform>();
        
        // 设置UI位置和锚点
        rt.anchoredPosition = spawnPosition;
        rt.localScale = Vector3.one;
        return instance;
    }

    private void SetupUIImage(GameObject instance, int index)
    {
        // 方法1：按名称查找子物体
        Transform child = instance.transform.Find("底片"); // 替换为你的子物体名称
        if (child == null)
        {
            Debug.LogError("未找到子物体: ChildWithImage");
            return;
        }

        Image img = child.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = sprites[index];
        }
        else
        {
            Debug.LogError("子物体上未找到Image组件");
        }
    }
    
    private void SetupButton(GameObject instance, int index)
    {
        Button btn = instance.GetComponent<Button>();
        if (btn != null)
        {
            // 动态绑定点击事件
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnButtonClick(index));
        }
        else
        {
            Debug.LogWarning("UI Prefab missing Button component");
        }
    }

    private void SetupText(GameObject instance, int index)
    {
        // 方法1：按名称查找TMP子物体（根据你的预制体结构修改名称）
        Transform textChild = instance.transform.Find("发音");
        if (textChild == null)
        {
            Debug.LogError("未找到TMP子物体: TextTMP");
            return;
        }

        TMP_Text tmpComponent = textChild.GetComponent<TMP_Text>();
        if (tmpComponent != null)
        {
            // 安全检查索引范围
            if (index >= 0 && index < texts.Count)
            {
                tmpComponent.text = texts[index];
            }
            else
            {
                Debug.LogWarning($"文本索引超出范围: {index}");
                tmpComponent.text = "Default Text";
            }
        }
        else
        {
            Debug.LogError("子物体上未找到TMP_Text组件");
        }
    }

    
    private void OnButtonClick(int index)
    {
        // 点击时播放对应音频
        PlayAudio(index);
    
        // 可以添加其他点击反馈逻辑
        Debug.Log($"Button {index} clicked!");
    }
    
    private void PlayAudio(int index)
    {
        if (audioPlayer != null)
        {
            audioPlayer.PlayOneShot(audioClips[index]);
        }
        else
        {
            Debug.LogWarning("AudioPlayer is not assigned");
        }
    }
}