using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonManager : MonoBehaviour
{
    // 静态实例，用于全局访问
    public static JsonManager Instance { get; private set; }
    public GameData gameData;

    // 在Awake方法中初始化单例
    private void Awake()
    {
        // 检查是否已经存在实例
        if (Instance == null)
        {
            // 如果不存在，则将当前对象设为实例
            Instance = this;
            // 确保对象在场景切换时不会被销毁
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 如果已经存在实例，则销毁当前对象以避免重复
            Destroy(gameObject);
        }
        
        // 加载 JSON 数据
        gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");

        // 输出加载的数据
        Debug.Log($"PlacementUI: {gameData.placementClear}, Tutorial: {gameData.tutorialClear}");
    }

}
