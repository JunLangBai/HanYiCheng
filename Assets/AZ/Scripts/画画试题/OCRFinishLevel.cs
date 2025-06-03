using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OCRFinishLevel : MonoBehaviour
{
    public void FinishOCRLevel()
    {
        var currentSceneName = SceneManager.GetActiveScene().name;
        var currentLevelID = currentSceneName.Replace("Level", ""); // 例如场景名 "Level1-1" → ID "1-1"
        
        // 解锁下一关
        LevelSelectMngr.Instance.UnlockNextLevel(currentLevelID);

        // 强制保存进度
        LevelSelectMngr.Instance.SaveProgress();
        
        // 标记当前关卡为完成
        MarkLevelCompleted(currentLevelID);

        // 匹配连续的数字部分
        Match match = Regex.Match(LevelSelectMngr.Instance.CurrentArea.ToString(), @"\d+");
        
        if (match.Success)
        {
            string numberPart = match.Value;  // 获取数字字符串 "2"
            int number = int.Parse(numberPart); // 转换为整数
            SceneManager.LoadScene("LevelSelection" + number);
        }
    }

    private void MarkLevelCompleted(string levelID)
    {
        var gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
        if (gameData == null) return;

        var targetLevel = gameData.levels.FirstOrDefault(l => l.LevelID == levelID);
        if (targetLevel != null)
        {
            // 更新现有关卡数据
            targetLevel.ISUnlockedByDefault = true;
        }
        else
        {
            // 添加新关卡数据
            gameData.levels.Add(new LevelDataJson
            {
                LevelID = levelID,
                ISUnlockedByDefault = true,
                Scene = SceneManager.GetActiveScene().path, // 使用场景路径
                LevelName = $"OCR关卡-{levelID}" // 根据实际需要设置
            });
        }

        JsonFileManager.SaveToJson(gameData, "GameData.json");
    }
}