using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchingFinishLevel : MonoBehaviour
{
    public void FinishMatchingLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        string currentLevelID = currentSceneName.Replace("Level", ""); // 例如 Level2-1 → 2-1

        // 解锁下一关
        LevelSelectMngr.Instance.UnlockNextLevel(currentLevelID);

        // 强制保存
        LevelSelectMngr.Instance.SaveProgress();

        // 标记当前关卡为完成
        MarkLevelCompleted(currentLevelID);

        // 加载对应区域的关卡选择界面
        Match match = Regex.Match(LevelSelectMngr.Instance.CurrentArea.ToString(), @"\d+");

        if (match.Success)
        {
            string areaNum = match.Value;
            SceneManager.LoadScene("LevelSelection" + areaNum); // 例如：LevelSelection2
        }
    }

    private void MarkLevelCompleted(string levelID)
    {
        GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
        if (gameData == null) return;

        var existing = gameData.levels.FirstOrDefault(l => l.LevelID == levelID);
        if (existing != null)
        {
            existing.ISUnlockedByDefault = true;
        }
        else
        {
            gameData.levels.Add(new LevelDataJson
            {
                LevelID = levelID,
                ISUnlockedByDefault = true,
                Scene = SceneManager.GetActiveScene().path,
                LevelName = $"Matching关卡-{levelID}"
            });
        }

        JsonFileManager.SaveToJson(gameData, "GameData.json");
    }
}