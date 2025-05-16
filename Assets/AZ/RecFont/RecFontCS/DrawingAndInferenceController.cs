using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class DrawingAndInferenceController : MonoBehaviour
{
    public DrawingBoard drawingBoard;
    public PythonScriptRunner inferenceScript;
    public Button recognizeButton;
    public TextMeshProUGUI resultText;  // 用于展示识别结果
    public Button clearButton; // 新增：清空按钮引用
    
    void Start()
    {
        inferenceScript = gameObject.GetComponent<PythonScriptRunner>();
        recognizeButton.onClick.AddListener(OnRecognizeButtonClick);
        clearButton.onClick.AddListener(OnClearButtonClick); // 新增：注册清空按钮事件
    }

    void OnRecognizeButtonClick()
    {
        Texture2D drawingTexture = drawingBoard.GetDrawingTexture();

        if (drawingTexture != null)
        {
            string imagePath = Application.dataPath + "/AZ/RecFont/temp.jpg";
            string resultPath = Application.dataPath + "/AZ/RecFont/result.txt";

            // 删除旧的结果文件
            if (File.Exists(resultPath))
            {
                File.Delete(resultPath);
            }

            SaveTextureAsImage(drawingTexture, imagePath);

            // 启动协程等待推理结果生成
            StartCoroutine(RunInferenceAndWaitForResult(imagePath, resultPath));
        }
        else
        {
            Debug.LogError("绘图纹理为空！");
        }
    }

    IEnumerator RunInferenceAndWaitForResult(string imagePath, string resultPath)
    {
        // 启动 Python 推理
        inferenceScript.RunPythonServerInference(imagePath);

        float timeout = 5f; // 最长等待 5 秒
        float timer = 0f;

        // 等待 result.txt 文件生成
        while (!File.Exists(resultPath))
        {
            yield return new WaitForSeconds(0.05f); // 每 50ms 检查一次
            timer += 0.05f;

            if (timer >= timeout)
            {
                resultText.text = "等待超时，未生成结果文件";
                yield break;
            }
        }

        // 读取预测结果
        string resultTextFromFile = File.ReadAllText(resultPath);
        if (resultTextFromFile=="<rare>")
        {
            resultText.text = "当前书写内容不是常用字";
        }
        else
        {
            resultText.text = "书写的结果是：" + resultTextFromFile;
        }
            
    }

// 新增：清空按钮点击事件处理
    void OnClearButtonClick()
    {
        drawingBoard.ClearCanvas();
        resultText.text = "已清空画板";
    }


    void SaveTextureAsImage(Texture2D texture, string path)
    {
        Texture2D resized = new Texture2D(64, 64, TextureFormat.RGB24, false);
        RenderTexture rt = RenderTexture.GetTemporary(64, 64);
        Graphics.Blit(texture, rt);
        RenderTexture.active = rt;
        resized.ReadPixels(new Rect(0, 0, 64, 64), 0, 0);
        resized.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        byte[] bytes = resized.EncodeToJPG();
        File.WriteAllBytes(path, bytes);
    }
}