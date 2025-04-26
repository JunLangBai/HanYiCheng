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
}