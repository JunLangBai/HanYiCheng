using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectMngr : MonoBehaviour
{
    public static LevelSelectMngr Instance;

    [Header("References")] public Transform LevelParent;

    public GameObject LevelButtonPrefab;
    public TextMeshProUGUI AreaHeaderText;
    public AreaData CurrentArea;

    private readonly List<GameObject> _buttonObjects = new();

    [Header("Progress")] public HashSet<string> UnlockedLevelIDs = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeLevelSystem();
    }

    private void InitializeLevelSystem()
    {
        LoadProgress();
        CleanOldButtons();
        CreateLevelButtons();
        UpdateAreaHeader();
    }

    private void LoadProgress()
    {
        UnlockedLevelIDs.Clear();

        // 1. 加载默认解锁的关卡
        foreach (var level in CurrentArea.Levels.Where(l => l.ISUnlockedByDefault))
        {
            UnlockedLevelIDs.Add(level.LevelID);
            Debug.Log($"默认解锁: {level.LevelID}");
        }

        // 2. 加载存档数据（复用 ISUnlockedByDefault 字段）
        var savedData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
        if (savedData != null)
            foreach (var savedLevel in savedData.levels)
                // 关键修改：只要存档中标记为 true 就解锁
                if (savedLevel.ISUnlockedByDefault && !UnlockedLevelIDs.Contains(savedLevel.LevelID))
                {
                    UnlockedLevelIDs.Add(savedLevel.LevelID);
                    Debug.Log($"存档解锁: {savedLevel.LevelID}");
                }

        // 3. 保底机制
        if (UnlockedLevelIDs.Count == 0 && CurrentArea.Levels.Count > 0)
        {
            UnlockedLevelIDs.Add(CurrentArea.Levels[0].LevelID);
            Debug.Log($"保底解锁: {CurrentArea.Levels[0].LevelID}");
        }
    }

    private void CleanOldButtons()
    {
        foreach (var button in _buttonObjects)
            if (button != null)
                Destroy(button);
        _buttonObjects.Clear();
    }

    private void CreateLevelButtons()
    {
        foreach (var levelData in CurrentArea.Levels)
        {
            var buttonObj = Instantiate(LevelButtonPrefab, LevelParent);
            _buttonObjects.Add(buttonObj);

            var levelButton = buttonObj.GetComponent<LevelButton>();
            var isUnlocked = UnlockedLevelIDs.Contains(levelData.LevelID);

            levelButton.Setup(levelData, isUnlocked);

            // 设置按钮名称用于调试
            buttonObj.name = $"Button_{levelData.LevelID}";
        }
    }

    public void UnlockNextLevel(string completedLevelID)
    {
        var currentIndex = CurrentArea.Levels.FindIndex(l => l.LevelID == completedLevelID);
        if (currentIndex == -1 || currentIndex + 1 >= CurrentArea.Levels.Count) return;

        var nextLevel = CurrentArea.Levels[currentIndex + 1];

        // 更新内存中的解锁状态
        if (!UnlockedLevelIDs.Contains(nextLevel.LevelID))
        {
            UnlockedLevelIDs.Add(nextLevel.LevelID);
            UpdateButtonState(nextLevel);
        }

        // 直接调用保存
        SaveProgress();
    }

    private void UpdateButtonState(LevelData levelData)
    {
        var buttonObj = _buttonObjects.FirstOrDefault(b =>
            b != null &&
            b.name.EndsWith(levelData.LevelID));

        if (buttonObj != null) buttonObj.GetComponent<LevelButton>().Unlock();
    }

    public void SaveProgress()
    {
        // 加载现有存档或创建新存档
        var gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
        if (gameData == null)
            gameData = new GameData
            {
                placementClear = false, // 默认值
                tutorialClear = false,
                volume = 1.0f,
                username = "",
                profilePictureIndex = 0
            };

        // 更新当前区域关卡状态
        foreach (var level in CurrentArea.Levels)
        {
            var isUnlocked = UnlockedLevelIDs.Contains(level.LevelID);
            var targetLevel = gameData.levels.FirstOrDefault(l => l.LevelID == level.LevelID);

            if (targetLevel != null)
                targetLevel.ISUnlockedByDefault = isUnlocked;
            else
                gameData.levels.Add(new LevelDataJson(level)
                {
                    ISUnlockedByDefault = isUnlocked
                });
        }

        // 保留其他配置字段
        JsonFileManager.SaveToJson(gameData, "GameData.json");

        Debug.Log("进度保存完成，保留字段状态: " +
                  $"placementClear={gameData.placementClear}, " +
                  $"tutorialClear={gameData.tutorialClear}");
    }

    private void UpdateAreaHeader()
    {
        if (AreaHeaderText != null) AreaHeaderText.text = CurrentArea.AreaName;
    }

    public void ReturnToMainUI()
    {
        SceneManager.LoadScene("MainUI");
    }
}