using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundInitializer : MonoBehaviour
{
    private void Start()
    {
        // 查找场景中所有按钮（包括未激活的）
        var allButtons = FindObjectsOfType<Button>(true);

        foreach (var btn in allButtons)
            // 检查是否已存在 GlobalButtonClickListener 组件
            if (!btn.GetComponent<GlobalButtonClickListener>())
                // 添加组件到按钮所在的游戏对象
                btn.gameObject.AddComponent<GlobalButtonClickListener>();

        // 可选：销毁自身，避免重复运行
        Destroy(this);
    }
}