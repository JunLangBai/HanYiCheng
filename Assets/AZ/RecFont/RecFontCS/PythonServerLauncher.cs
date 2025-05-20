using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonServerLauncher : MonoBehaviour
{
    private Process serverProcess;

    // 改为只需要 exe 路径
    public string exePath;

    private bool isRunning = false;

    void Start()
    {
        // 修改为你的 .exe 路径（推荐放在 StreamingAssets 或其他路径）
        exePath = Application.dataPath + "/AZ/RecFont/dist/server.exe";

        UnityEngine.Debug.LogError($"EXE 路径: {exePath}");

        if (!isRunning)
        {
            StartPythonServer();
        }
    }

    void StartPythonServer()
    {
        if (!File.Exists(exePath))
        {
            UnityEngine.Debug.LogError($"❌ 找不到可执行文件: {exePath}");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = Path.GetDirectoryName(exePath),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        serverProcess = new Process();
        serverProcess.StartInfo = startInfo;

        try
        {
            serverProcess.Start();
            serverProcess.BeginOutputReadLine();
            serverProcess.BeginErrorReadLine();

            isRunning = true;
            UnityEngine.Debug.Log("✅ EXE 服务已启动！");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("❌ 启动 EXE 服务失败: " + ex.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (serverProcess != null && !serverProcess.HasExited)
        {
            serverProcess.Kill();
            UnityEngine.Debug.Log("🔚 服务已关闭");
        }
    }
}