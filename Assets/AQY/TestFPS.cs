using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestFPS : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // 拖拽UI中的Text组件到Inspector
    public float updateInterval = 0.5f; // 更新频率（秒）
    
    private float _accum = 0;
    private int _frames = 0;
    private float _timeLeft;

    void Start()
    {
        _timeLeft = updateInterval;
    }

    void Update()
    {
        _timeLeft -= Time.deltaTime;
        _accum += Time.timeScale / Time.deltaTime;
        _frames++;

        if (_timeLeft <= 0)
        {
            float fps = _accum / _frames;
            fpsText.text = $"FPS: {fps:N1}"; // 显示1位小数

            // 可选：颜色变化
            fpsText.color = (fps > 50) ? Color.green : 
                (fps > 30) ? Color.yellow : Color.red;

            _timeLeft = updateInterval;
            _accum = 0;
            _frames = 0;
        }
    }
}