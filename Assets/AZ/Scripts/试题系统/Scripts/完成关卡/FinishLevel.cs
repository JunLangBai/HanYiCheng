using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class FinishLevel : MonoBehaviour
{
    public void Finish()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        string currentLevelID = currentSceneName.Replace("Level", ""); // 例如场景名 "Level1-1" → ID "1-1"

        if (UIManager.Instance.GetScore() >= 0.8f)
        {
            Debug.Log($"当前正确率: {UIManager.Instance.GetScore() * 100}%"); // 输出实际正确率

            // 标记当前关卡为完成
            MarkLevelCompleted(currentLevelID);

            // 解锁下一关
            LevelSelectMngr.Instance.UnlockNextLevel(currentLevelID);

        }
        // 强制保存进度
        LevelSelectMngr.Instance.SaveProgress();

        SceneManager.LoadScene("LevelSelection1");
    }

    private void MarkLevelCompleted(string levelID)
    {
        GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
        if (gameData == null) return;

        var targetLevel = gameData.levels.FirstOrDefault(l => l.LevelID == levelID);
        if (targetLevel != null)
        {
            targetLevel.ISUnlockedByDefault = true;
        }
        else
        {
            // 使用无参构造函数 + 对象初始化器
            gameData.levels.Add(new LevelDataJson
            {
                LevelID = levelID,
                ISUnlockedByDefault = true,
                Scene = SceneManager.GetActiveScene().path, // 使用场景路径
                LevelName = "动态解锁的关卡" // 根据实际需要设置
            });
        }

        JsonFileManager.SaveToJson(gameData, "GameData.json");
    }
}