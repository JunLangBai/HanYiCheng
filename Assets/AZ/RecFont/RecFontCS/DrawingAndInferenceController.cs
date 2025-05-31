using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrawingAndInferenceController : MonoBehaviour
{
    public DrawingBoard drawingBoard;
    public HangulInference hangulInference; // 替代原 PythonScriptRunner
    public Button recognizeButton;
    public TextMeshProUGUI resultText;
    public Button clearButton;

    private void Start()
    {
        recognizeButton.onClick.AddListener(OnRecognizeButtonClick);
        clearButton.onClick.AddListener(OnClearButtonClick);
    }

    private void OnRecognizeButtonClick()
    {
        var drawingTexture = drawingBoard.GetDrawingTexture();
        if (drawingTexture == null)
        {
            Debug.LogError("❌ 获取绘图图像失败：image 为 null");
            return;
        }

        if (drawingTexture != null)
        {
            var result = hangulInference.Predict(drawingTexture);

            if (result == "<rare>")
                resultText.text = "当前书写内容不是常用字";
            else
                resultText.text = "书写的结果是：" + result;
        }
        else
        {
            Debug.LogError("绘图纹理为空！");
        }
    }

    private void OnClearButtonClick()
    {
        drawingBoard.ClearCanvas();
        resultText.text = "已清空画板";
    }
}