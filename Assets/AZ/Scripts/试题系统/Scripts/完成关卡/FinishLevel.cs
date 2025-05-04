using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLevel : MonoBehaviour
{
   public void Finish()
   {
      if (UIManager.Instance.GetScore() >= 0.8f)
      {
         LevelSelected();
         LevelSelectMngr.Instance.UnlockNextLevel("1-1");
         SceneManager.LoadScene("LevelSelection");
      }
      else
      {
         SceneManager.LoadScene("LevelSelection");
      }
   }


   //通关后标记为true
    private void LevelSelected()
    {
       GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
    
       if (gameData != null)
       {
          // 找到目标 LevelDataJson 对象
          var targetLevel = gameData.levels.FirstOrDefault(l => l.Scene == SceneManager.GetActiveScene().name);
    
          if (targetLevel != null)
          {
             // 修改 ISUnlockedByDefault 值
             targetLevel.ISUnlockedByDefault = true;
    
             Debug.Log($"Updated LevelID: {targetLevel.Scene}, ISUnlockedByDefault: {targetLevel.ISUnlockedByDefault}");
    
             // 保存修改后的数据回 JSON 文件
             JsonFileManager.SaveToJson(gameData, "GameData.json");
          }
          else
          {
             Debug.LogWarning("Level with ID '关卡数据盒名字' not found!");
          }
       }
    }
}
