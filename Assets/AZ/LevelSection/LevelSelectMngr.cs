using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;
using System.Linq;

public class LevelSelectMngr : MonoBehaviour
{
   public Transform LevelParent;
   public GameObject LevelButtonPrefab;
   public TextMeshProUGUI AreaHeaderText;
   public TextMeshProUGUI LevelHeaderText;
   public AreaData CurrentArea;
   
   public HashSet<string> UnlockedLevelIDs = new HashSet<string>();
   
   public Camera _camera;
   
   private List<GameObject> _buttonObjects= new List<GameObject>();
   private Dictionary<GameObject, Vector3> _ButtonLocations = new Dictionary<GameObject, Vector3>();
   
   GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
  
   private void Start()
   {
      if (CurrentArea != null)
      {
         LoadAndApplyJson();
         SaveAreaDataToJson();
      }
      else
      {
         Debug.LogError("JSON 文件或数据盒未赋值！");
      }
      AssignAreaText();
      LoadUnlockedLevels();
      CreateLevelButtons();
   }
   
   // 序列化 AreaData 并保存到 JSON 文件
   // 序列化 AreaData 并保存到 JSON 文件
   void SaveAreaDataToJson()
   {
      // 将 AreaData 转换为 GameData
      var gameData = JsonFileManager.AreaDataConverter.ConvertToGameData(CurrentArea);

      // 使用 JsonFileManager 保存 JSON 数据
      JsonFileManager.SaveToJson(gameData, "GameData.json");

      Debug.Log($"Saved AreaData to JSON file: {"GameData.json"}");
   }

   // 加载 JSON 并更新 AreaData
   void LoadAndApplyJson()
   {
      // 使用 JsonFileManager 加载 JSON 数据
      GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");

      if (gameData != null)
      {
         // 将 GameData 应用到 AreaData
         JsonFileManager.AreaDataConverter.ApplyGameDataToAreaData(gameData, CurrentArea);
      }
      else
      {
         Debug.LogError("JSON 数据为空或格式不正确！");
      }
   }

   public void AssignAreaText()
   {
      AreaHeaderText.SetText(CurrentArea.AreaName);
   }

   private void LoadUnlockedLevels()
   {
      foreach (var level in CurrentArea.Levels)
      {
         if (level.ISUnlockedByDefault)
         {
            UnlockedLevelIDs.Add(level.LevelID);
         }
      }
   }

   private void CreateLevelButtons()
   {
      for (int i = 0; i < CurrentArea.Levels.Count; i++)
      {
         GameObject buttonGO = Instantiate(LevelButtonPrefab, LevelParent);
         _buttonObjects.Add(buttonGO);
         
         RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
         
         buttonGO.name = CurrentArea.Levels[i].LevelID;
         CurrentArea.Levels[i].LevelButtonObj = buttonGO;
         
         LevelButton levelButton = buttonGO.GetComponent<LevelButton>();
         levelButton.Setup(CurrentArea.Levels[i], UnlockedLevelIDs.Contains(CurrentArea.Levels[i].LevelID));
      }
   }
}
