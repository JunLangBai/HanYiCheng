// UISettingsController.cs
using UnityEngine;
using UnityEngine.UI;

public class FlowWindows : MonoBehaviour
{
    [Header("绑定截图中的元素")]
    [SerializeField] private CanvasGroup settingsPanel;
    [SerializeField] private Button gearButton;  // 对应Hierarchy中的Image (4)

    [Range(0.1f, 2f)]
    public float fadeSpeed = 0.5f;

    private bool isVisible;
    private float targetAlpha;

    void Start()
    {
        // 初始状态配置
        settingsPanel.alpha = 0;
        settingsPanel.interactable = false;
        settingsPanel.blocksRaycasts = false;

        // 绑定齿轮按钮事件
        gearButton.onClick.AddListener(TogglePanel);
    }

    void Update()
    {
        // 平滑渐变实现
        if (Mathf.Abs(settingsPanel.alpha - targetAlpha) > 0.001f)
        {
            settingsPanel.alpha = Mathf.Lerp(
                settingsPanel.alpha, 
                targetAlpha, 
                Time.unscaledDeltaTime / fadeSpeed
            );
        }
    }

    private void TogglePanel()
    {
        isVisible = !isVisible;
        targetAlpha = isVisible ? 1 : 0;
        
        // 立即更新交互状态
        settingsPanel.interactable = isVisible;
        settingsPanel.blocksRaycasts = isVisible;

        // 适配截图中的Animator组件
        if (TryGetComponent<Animator>(out var anim))
        {
            anim.enabled = !isVisible; // 显示面板时暂停原有动画
        }
    }
}