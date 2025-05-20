using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonServerLauncher : MonoBehaviour
{
    private Process serverProcess;

    public string pythonExecutable;
    public string serverScriptPath;

    private bool isRunning = false;

    void Start()
    {   
        pythonExecutable =  Application.dataPath + @"\StreamingAssets\Fonts\python.exe";
        serverScriptPath = Application.dataPath + @"\AZ\RecFont\server.py";
        if (!isRunning)
        {
            StartPythonServer();
        }
    }

    void StartPythonServer()
    {
        if (!File.Exists(serverScriptPath))
        {
            UnityEngine.Debug.LogError($"❌ 找不到 Python 脚本路径: {serverScriptPath}");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonExecutable,
            Arguments = $"\"{serverScriptPath}\"",  // 注意加引号以防路径中有空格
            WorkingDirectory = Path.GetDirectoryName(serverScriptPath),  // 设置正确目录
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };


        startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

        serverProcess = new Process();
        serverProcess.StartInfo = startInfo;

        try
        {
            serverProcess.Start();
            serverProcess.BeginOutputReadLine();
            serverProcess.BeginErrorReadLine();

            isRunning = true;
            UnityEngine.Debug.Log("✅ Python Flask 服务已启动！");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("❌ 启动 Python 服务失败: " + ex.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (serverProcess != null && !serverProcess.HasExited)
        {
            serverProcess.Kill();
            UnityEngine.Debug.Log("🔚 Python 服务已关闭");
        }
    }
}
