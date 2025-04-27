using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class GameData
{
    public string playerName = "DefaultPlayer";
    public int playerLevel = 1;
    public float health = 100f;
    public bool isPremiumUser;
}

public static class JsonFileManager
{
    // 获取文件路径（区分编辑器和打包环境）
    private static string GetFilePath(string fileName)
    {
#if UNITY_EDITOR
        // 编辑器模式下，保存到项目根目录下的 "SaveData" 文件夹
        var path = Path.Combine(Application.dataPath, "SaveData", fileName);
#else
        // 打包后，保存到持久化数据路径
        string path = Path.Combine(Application.persistentDataPath, fileName);
#endif
        return path;
    }

    // 保存 JSON 文件
    public static void SaveToJson<T>(T data, string fileName) where T : class
    {
        var filePath = GetFilePath(fileName);

        // 确保目录存在
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        // 序列化对象为 JSON 字符串
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);

        // 写入文件
        File.WriteAllText(filePath, json);

        Debug.Log($"Saved JSON to: {filePath}");
    }

    // 加载 JSON 文件
    public static T LoadFromJson<T>(string fileName) where T : class, new()
    {
        var filePath = GetFilePath(fileName);

        // 如果文件不存在，则创建默认文件
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"JSON file not found at: {filePath}. Creating a default one.");

            // 创建默认对象并保存
            var defaultData = new T();
            SaveToJson(defaultData, fileName);

            return defaultData;
        }

        // 读取文件内容
        var json = File.ReadAllText(filePath);

        // 反序列化 JSON 字符串为对象
        var data = JsonConvert.DeserializeObject<T>(json);

        Debug.Log($"Loaded JSON from: {filePath}");
        return data;
    }
}