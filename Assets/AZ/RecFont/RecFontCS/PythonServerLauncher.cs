using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonServerLauncher : MonoBehaviour
{
    private Process serverProcess;

    public string pythonExecutable = @"D:\ANACONDA\python.exe";
    public string serverScriptPath = Path.GetFullPath("server.py");

    void Start()
    {
        StartPythonServer();
    }

    void StartPythonServer()
    {
        if (!File.Exists(serverScriptPath))
        {
            UnityEngine.Debug.LogError("找不到 server.py 脚本！");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonExecutable,
            Arguments = serverScriptPath,
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
            UnityEngine.Debug.Log("Python 服务器已启动。");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("启动 Python 服务器失败: " + ex.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (serverProcess != null && !serverProcess.HasExited)
        {
            serverProcess.Kill();
            UnityEngine.Debug.Log("Python 服务器已关闭。");
        }
    }
}
