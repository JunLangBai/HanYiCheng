using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainUIManager : MonoBehaviour
{
    public TextMeshProUGUI nowLevels;
    public TextMeshProUGUI attendance;
    public TextMeshProUGUI nowTime;
    
    public Button _button;
    
    GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");

    private void Start()
    {
        _button.onClick.AddListener(() => SceneManager.LoadScene("TotalLevel"));
        nowLevels.text = "当前解锁的关卡为：" + GetCurrentLevelToPlay(gameData).LevelID;
        
        //执行登录检查
        CheckDailyLogin(ref gameData);

        // 保存更新后的数据
        SaveGameData(gameData);

        attendance.text = "连续学习天数：" + gameData.LoginStreak + "天";
        
        // // 使用协程实现每秒更新（更高效）
        // StartCoroutine(UpdateTime());
    }
    
    void SaveGameData(GameData data)
    {
        JsonFileManager.SaveToJson(data, "GameData.json");
    }

    void CheckDailyLogin(ref GameData gameData)
    {
        // 定义中国时区（Windows系统用"China Standard Time"，Linux/macOS用"Asia/Shanghai"）
        TimeZoneInfo chinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        DateTime utcTime = DateTime.UtcNow;
        DateTime currentUTC = TimeZoneInfo.ConvertTimeFromUtc(utcTime, chinaTimeZone);
        
        // 首次登录初始化
        if (gameData.LastLoginUTC == default)
        {
            Debug.Log("首次登录初始化");
            gameData.LoginStreak = 1;
            gameData.LastLoginUTC = currentUTC;
            return;
        }
        // 计算时间差（只比较日期部分）
        DateTime lastDate = gameData.LastLoginUTC.Date;
        DateTime currentDate = currentUTC.Date;
        TimeSpan interval = currentDate - lastDate;

        // 处理不同情况
        if (interval.TotalDays == 1) // 连续登录
        {
            gameData.LoginStreak++;
            Debug.Log($"连续登录天数更新：{gameData.LoginStreak}");
        }
        else if (interval.TotalDays > 1 || interval.TotalDays < 0) // 中断或时间异常
        {
            gameData.LoginStreak = 1;
            Debug.Log("登录间隔异常，重置计数器");
        }
        else // 同一天
        {
            Debug.Log("今日已记录登录");
            return; // 不更新存储时间
        }

        // 更新最后登录时间
        gameData.LastLoginUTC = currentUTC;
    }


    public static LevelDataJson GetCurrentLevelToPlay(GameData gameData)
    {
        // 检查是否完成了摸底和教程
        if (!gameData.placementClear || !gameData.tutorialClear)
        {
            return null;
        }

        // 遍历关卡列表，查找第一个未解锁的关卡
        for (int i = 0; i < gameData.levels.Count; i++)
        {
            if (!gameData.levels[i].ISUnlockedByDefault)
            {
                if (i == 0)
                {
                    // 第一个关卡未解锁，配置错误
                    return null;
                }
                else
                {
                    // 返回前一个已解锁的关卡
                    return gameData.levels[i - 1];
                }
            }
        }

        // 所有关卡都已解锁，返回最后一个
        return gameData.levels.LastOrDefault();
    }
    
    IEnumerator UpdateTime()
    {
        // 定义中国时区（Windows系统用"China Standard Time"，Linux/macOS用"Asia/Shanghai"）
        TimeZoneInfo chinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");

        while (true)
        {
            DateTime utcTime = DateTime.UtcNow;
            DateTime chinaTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, chinaTimeZone);
            nowTime.text = chinaTime.ToString("f"); // 格式化输出
            yield return new WaitForSeconds(1);
        }
    }
}
