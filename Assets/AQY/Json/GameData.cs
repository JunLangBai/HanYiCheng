using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public DateTime LastLoginUTC; // 使用UTC时间存储
    public int LoginStreak;       // 连续登录天数
    //是否经过摸底
    public bool placementClear = false;
    //是否经过教程
    public bool tutorialClear = false;
    //关卡数据
    public List<LevelDataJson> levels = new List<LevelDataJson>();
    //音量大小
    public float volume = 1;
    //用户名
    public string username;
    //头像下标
    public int  profilePictureIndex;
}

[System.Serializable]
public class LevelDataJson
{
    public string LevelID;
    public bool ISUnlockedByDefault;
    public string Scene;
    public string LevelName;

    // 无参构造函数（用于 JSON 反序列化）
    public LevelDataJson() 
    {
        // 空构造函数
    }

    // 带参构造函数（原功能保留）
    public LevelDataJson(LevelData level)
    {
        if (level == null) return;
        LevelID = level.LevelID;
        ISUnlockedByDefault = level.ISUnlockedByDefault;
        Scene = level.Scene?.SceneName ?? string.Empty;
        LevelName = level.LevelName ?? string.Empty;
    }
}
[System.Serializable]
public class AreaDataJson
{
    public string AreaName;
    public List<LevelDataJson> Levels;

    // 构造函数：从 AreaData 转换为 AreaDataJson
    public AreaDataJson(AreaData area)
    {
        AreaName = area.AreaName;
        Levels = area.Levels.Select(l => new LevelDataJson(l)).ToList();
    }
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
    
    public static class AreaDataConverter
    {
        // 将 AreaData 转换为 GameData
        public static GameData ConvertToGameData(AreaData areaData)
        {
            var gameData = new GameData
            {
                placementClear = false, // 可以根据需要设置
                tutorialClear = false,  // 可以根据需要设置
                levels = areaData.Levels.Select(level => new LevelDataJson(level)).ToList()
            };

            return gameData;
        }

        // 将 GameData 转换为 AreaData
        public static void ApplyGameDataToAreaData(GameData gameData, AreaData areaData)
        {
            if (gameData == null || areaData == null) return;

            foreach (var jsonLevel in gameData.levels)
            {
                var level = areaData.Levels.FirstOrDefault(l => l.LevelID == jsonLevel.LevelID);
                if (level != null)
                {
                    level.ISUnlockedByDefault = jsonLevel.ISUnlockedByDefault;
                    Debug.Log($"Updated LevelID: {jsonLevel.LevelID}, ISUnlockedByDefault: {jsonLevel.ISUnlockedByDefault}");
                }
                else
                {
                    Debug.LogWarning($"Level with ID {jsonLevel.LevelID} not found in AreaData!");
                }
            }
        }
    }

}