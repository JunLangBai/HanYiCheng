using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonServerLauncher : MonoBehaviour
{
    private Process serverProcess;

    public string pythonExecutable => Path.Combine(
        Application.streamingAssetsPath, 
        "Fonts", 
        "python.exe"
    );

    public string serverScriptPath => Path.Combine(
        Application.dataPath, 
        "AZ", 
        "RecFont", 
        "server.py"
    );
    private bool isRunning = false;

    void Start()
    {
        if (!isRunning)
        {
            StartPythonServer();
        }
    }

    void StartPythonServer()
    {

        var startInfo = new ProcessStartInfo
        {
            FileName = pythonExecutable,
            Arguments = $"\"{serverScriptPath}\"",  // 添加引号防止路径带空格出错
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

        serverProcess = new Process();
        serverProcess.StartInfo = startInfo;

        

            serverProcess.Start();
            serverProcess.BeginOutputReadLine();
            serverProcess.BeginErrorReadLine();

            
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
