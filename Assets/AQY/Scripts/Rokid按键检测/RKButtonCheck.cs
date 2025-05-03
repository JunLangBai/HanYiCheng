using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  Rokid.UXR;
public class RKButtonCheck : MonoBehaviour
{
    private RKButtonCheck Instance;
    
    // 在Awake方法中初始化单例
    private void Awake()
    {
        // 检查是否已经存在实例
        if (Instance == null)
        {
            // 如果不存在，则将当前对象设为实例
            Instance = this;
            // 确保对象在场景切换时不会被销毁
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 如果已经存在实例，则销毁当前对象以避免重复
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetButton(KeyCode.Joystick1Button2.ToString()))
        {
            ExitGame();
        }
    }
    
    public void ExitGame()
    {
        // 如果是在编辑器中运行，使用 UnityEditor 的功能退出播放模式
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 在发布的应用中，退出游戏
            Application.Quit();
#endif
    }
}
