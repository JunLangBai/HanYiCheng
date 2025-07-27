using Rokid.UXR.Native;
using UnityEngine;
using UnityEngine.UI;

public class GlassBrightnessController : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Slider brightnessSlider;    // 亮度控制滑动条

    [Header("Brightness Settings")]
    [SerializeField, Range(10, 100)] private int minBrightness = 10;
    [SerializeField, Range(10, 100)] private int maxBrightness = 100;

    private int currentBrightness;

    private readonly GameData gameData = JsonFileManager.LoadFromJson<GameData>("GameData.json");
    
     private bool isSettingBrightness; // 防止设置过程中的递归调用
     // 在滑动条值变化回调中添加节流
     private float lastSetTime;

    void Start()
    {
        InitializeBrightnessControl();
    }

    // 初始化亮度控制系统
    private void InitializeBrightnessControl()
    {
        // 获取当前亮度并初始化UI
        currentBrightness = GetCurrentBrightness();
        gameData.light = currentBrightness;
        JsonFileManager.SaveToJson(gameData, "GameData.json");
        SetupSlider();
        
        // 设置监听事件
        brightnessSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    // 获取设备当前亮度
    private int GetCurrentBrightness()
    {
        try
        {
            // 调用Rokid原生接口
            return NativeInterface.NativeAPI.GetGlassBrightness();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"获取亮度失败: {e.Message}");
            return 60; // 默认安全值
        }
    }

    // 配置滑动条参数
    private void SetupSlider()
    {
        brightnessSlider.minValue = minBrightness;
        brightnessSlider.maxValue = maxBrightness;
        brightnessSlider.value = currentBrightness;
        brightnessSlider.wholeNumbers = true; // 整数步进
    }

    // 滑动条值变化回调 - 实时设置亮度
    private void OnSliderChanged(float value)
    {
        if (Time.time - lastSetTime < 0.1f) return; // 100ms节流
    
        currentBrightness = Mathf.RoundToInt(value);
        ApplyBrightness();
        lastSetTime = Time.time;
        gameData.light = currentBrightness;
        JsonFileManager.SaveToJson(gameData, "GameData.json");
    }
    

    // 应用亮度设置到设备
    private void ApplyBrightness()
    {
        isSettingBrightness = true;
        
        try
        {
            // 调用Rokid原生接口
            NativeInterface.NativeAPI.SetGlassBrightness(currentBrightness);
            Debug.Log($"亮度已实时设置为: {currentBrightness}%");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"设置亮度失败: {e.Message}");
        }
        finally
        {
            isSettingBrightness = false;
        }
    }

    // 设备断开时清理
    void OnDestroy()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveListener(OnSliderChanged);
        }
    }
}