using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DrawingAndInferenceController : MonoBehaviour
{
    public DrawingBoard drawingBoard;
    public HangulInference hangulInference; // 替代原 PythonScriptRunner
    public Button recognizeButton;
    public TextMeshProUGUI resultText;
    public Button clearButton;

    void Start()
    {
        recognizeButton.onClick.AddListener(OnRecognizeButtonClick);
        clearButton.onClick.AddListener(OnClearButtonClick);
    }

    void OnRecognizeButtonClick()
    {
        Texture2D drawingTexture = drawingBoard.GetDrawingTexture();
        if (drawingTexture == null)
        {
            Debug.LogError("❌ 获取绘图图像失败：image 为 null");
            return;
        }
        
        if (drawingTexture != null)
        {
            string result = hangulInference.Predict(drawingTexture);

            if (result == "<rare>")
            {
                resultText.text = "当前书写内容不是常用字";
            }
            else
            {
                resultText.text = "书写的结果是：" + result;
            }
        }
        else
        {
            Debug.LogError("绘图纹理为空！");
        }
    }

    void OnClearButtonClick()
    {
        drawingBoard.ClearCanvas();
        resultText.text = "已清空画板";
    }
}