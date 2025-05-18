using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Random = UnityEngine.Random;

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

    [Header("Audio")]
    public AudioSource audioPlayer;   // 音频播放器

    [Header("Generation Mode")]
    public GenerationMode mode = GenerationMode.Sequential;

    [Header("Position Settings")]
    public Vector2 spawnPosition = Vector2.zero;

    private int currentIndex;

    private void Start()
    {
        int totalToGenerate = Mathf.Min(sprites.Count, audioClips.Count);
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
        SetupButton(instance, index); // 新增这行
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

        return true;
    }

    private int GetNextIndex()
    {
        int maxIndex = Mathf.Min(sprites.Count, audioClips.Count);
        
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