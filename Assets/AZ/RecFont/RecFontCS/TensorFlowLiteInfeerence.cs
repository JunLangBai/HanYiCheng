using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PythonScriptRunner : MonoBehaviour
{
    private static readonly HttpClient client = new();
    public string pythonScriptPath = "/AZ/RecFont/server.py";
    public string imagePath = "/AZ/RecFont/temp.jpg";
    public string pythonOutput = "";

    public async Task<string> RunPythonServerInference(string imagePath)
    {
        try
        {
            var form = new MultipartFormDataContent();
            var imgData = File.ReadAllBytes(imagePath);
            form.Add(new ByteArrayContent(imgData), "image", "temp.jpg");

            var response = await client.PostAsync("http://127.0.0.1:5000/predict", form);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var json = JsonUtility.FromJson<LabelResult>("{\"label\":" +
                                                             result.Split(':')[1].Replace('}', ' ').Trim() + "}");
                return json.label;
            }

            // UnityEngine.Debug.LogError("Python服务器返回失败: " + result);
            return "识别失败";
        }
        catch (Exception e)
        {
            // UnityEngine.Debug.LogError("调用Python服务器出错: " + e.Message);
            return "错误";
        }
    }


    public string RunPythonScript()
    {
        var pythonExecutable = "/StreamingAssets/Fonts/python.exe";
        var arguments = $"{pythonScriptPath} {imagePath}";

        var startInfo = new ProcessStartInfo
        {
            FileName = pythonExecutable,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            Debug.Log("即将启动Python脚本...");
            Debug.Log($"命令: {pythonExecutable} {arguments}");

            using (var process = Process.Start(startInfo))
            {
                pythonOutput = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("运行Python脚本时发生异常:\n" + ex.Message);
        }

        return pythonOutput;
    }

    [Serializable]
    public class LabelResult
    {
        public string label;
    }
}