using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Diagnostics;
using System.Text;

public class PythonScriptRunner : MonoBehaviour
{
    public string pythonScriptPath = @"D:\GitHub\HanYiCheng\Assets\AZ\RecFont\server.py";
    public string imagePath = @"D:\GitHub\HanYiCheng\Assets\AZ\RecFont\temp.jpg";
    public string pythonOutput = "";
    
    
    private static readonly HttpClient client = new HttpClient();

    public async Task<string> RunPythonServerInference(string imagePath)
    {
        try
        {
            MultipartFormDataContent form = new MultipartFormDataContent();
            byte[] imgData = File.ReadAllBytes(imagePath);
            form.Add(new ByteArrayContent(imgData), "image", "temp.jpg");

            HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:5000/predict", form);
            string result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var json = JsonUtility.FromJson<LabelResult>("{\"label\":" + result.Split(':')[1].Replace('}', ' ').Trim() + "}");
                return json.label;
            }
            else
            {
                // UnityEngine.Debug.LogError("Python服务器返回失败: " + result);
                return "识别失败";
            }
        }
        catch (Exception e)
        {
            // UnityEngine.Debug.LogError("调用Python服务器出错: " + e.Message);
            return "错误";
        }
    }

    [Serializable]
    public class LabelResult
    {
        public string label;
    }
    
    
    public string RunPythonScript()
    {
        string pythonExecutable = @"D:\\ANACONDA\\python.exe";
        string arguments = $"{pythonScriptPath} {imagePath}";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonExecutable,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        try
        {
            UnityEngine.Debug.Log("即将启动Python脚本...");
            UnityEngine.Debug.Log($"命令: {pythonExecutable} {arguments}");

            using (Process process = Process.Start(startInfo))
            {
                pythonOutput = process.StandardOutput.ReadToEnd();  
                process.WaitForExit();
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("运行Python脚本时发生异常:\n" + ex.Message);
        }

        return pythonOutput;
    }
}