using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanalExit : MonoBehaviour
{
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
