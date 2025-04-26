using UnityEngine;

public class GlobalTutorialsManager : MonoBehaviour
{
    public static GlobalTutorialsManager instance;
    public bool canNextText;

    private void Awake()
    {
        // 单例初始化
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void CanNextText()
    {
        canNextText = true;
    }
    
    /// <summary>
    /// 验证玩家选择
    /// </summary>
    public void ValidateChoice(string choice)
    {
        // 在此处添加游戏逻辑验证
        Debug.Log($"Validating choice: {choice}");
    }
}