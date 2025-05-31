using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TensorFlowLite;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

public class HangulInference : MonoBehaviour
{
    [SerializeField] private string labelFileName = "labels.txt";
    [SerializeField] private string modelFileName = "model.tflite";
    private float[] inputBuffer;
    private Interpreter interpreter;

    private string[] labels;
    private float[] outputBuffer;

    private void Start()
    {
        StartCoroutine(LoadLabelsAsync());
        StartCoroutine(LoadModelAsync());
    }

    private void OnDestroy()
    {
        interpreter?.Dispose();
    }

    private IEnumerator LoadLabelsAsync()
    {
        var labelPath = Path.Combine(Application.streamingAssetsPath, labelFileName);
        Debug.Log($"加载标签文件: {labelPath}");

        var request = UnityWebRequest.Get(labelPath);
        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            Debug.LogError($"❌ 标签文件加载失败: {request.error}");
            yield break;
        }

        var text = request.downloadHandler.text;
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("❌ 标签文件为空！");
            yield break;
        }

        labels = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Debug.Log($"✅ 标签加载成功，共 {labels.Length} 个标签");
    }

    private IEnumerator LoadModelAsync()
    {
        var modelPath = Path.Combine(Application.streamingAssetsPath, modelFileName);
        Debug.Log($"开始加载模型文件: {modelPath}");

        byte[] modelData = null;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (!File.Exists(modelPath))
        {
            Debug.LogError($"❌ 模型文件未找到: {modelPath}");
            yield break;
        }

        modelData = File.ReadAllBytes(modelPath);
#else
    UnityWebRequest request = UnityWebRequest.Get(modelPath);
    yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
    if (request.result != UnityWebRequest.Result.Success)
#else
    if (request.isNetworkError || request.isHttpError)
#endif
    {
        Debug.LogError($"❌ 模型文件加载失败: {request.error}");
        yield break;
    }
    modelData = request.downloadHandler.data;
#endif

        Debug.Log($"读取模型数据长度: {(modelData != null ? modelData.Length.ToString() : "null")}");

        if (modelData == null || modelData.Length == 0)
        {
            Debug.LogError("❌ 模型数据为空！");
            yield break;
        }

        try
        {
            interpreter?.Dispose();
            var options = new InterpreterOptions { threads = 1 };
            interpreter = new Interpreter(modelData, options);
            interpreter.AllocateTensors();

            var inputInfo = interpreter.GetInputTensorInfo(0);
            inputBuffer = new float[inputInfo.shape[1]];

            var outputInfo = interpreter.GetOutputTensorInfo(0);
            outputBuffer = new float[outputInfo.shape[1]];

            Debug.Log($"✅ 模型加载成功: 输入维度 {string.Join(",", inputInfo.shape)}, 输出维度 {string.Join(",", outputInfo.shape)}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 加载模型失败: {ex.Message}\n{ex.StackTrace}");
        }
    }


    public string Predict(Texture2D image)
    {
        try
        {
            if (image == null)
            {
                Debug.LogError("❌ 传入的图像为 null");
                return "";
            }

            if (interpreter == null)
            {
                Debug.LogError("❌ 模型未加载成功，interpreter 为 null");
                return "";
            }

            if (outputBuffer == null)
            {
                Debug.LogError("❌ outputBuffer 尚未初始化");
                return "";
            }

            // 数据增强，得到多版本图像
            var variants = AugmentImage(image);
            var predictions = new List<string>();

            foreach (var img in variants)
            {
                // 预处理，转为模型输入格式的一维float数组
                var vector = Preprocess(img);
                inputBuffer = vector; // 直接赋值

                // 填充输入tensor并推理
                interpreter.SetInputTensorData(0, inputBuffer);
                interpreter.Invoke();

                // 获取输出结果
                interpreter.GetOutputTensorData(0, outputBuffer);

                // softmax概率计算，选最大概率索引
                var probs = Softmax(outputBuffer);
                var index = ArgMax(probs);
                predictions.Add(labels[index]);
            }

            // 多版本预测结果投票，返回最多的那个label
            var finalPrediction = predictions
                .GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .First().Key;

            Debug.Log($"✅ 最终预测: {finalPrediction}");
            return finalPrediction;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 推理过程中发生异常: {ex.Message}");
            return "";
        }
    }


    // 下面是你的辅助方法，保持不变

    private float[] Preprocess(Texture2D input)
    {
        // 1. 转灰度并resize为64x64（建议用RenderTexture+shader做灰度缩放）
        var gray64 = ResizeAndGrayscale(input, 64, 64);

        // 2. 读取像素并归一化反色
        var pixels = gray64.GetPixels32();
        var vector = new float[pixels.Length];

        for (var i = 0; i < pixels.Length; i++)
        {
            // 灰度值在r/g/b三通道相同
            var gray = pixels[i].r / 255f;
            vector[i] = 1f - gray; // 反色
        }

        return vector;
    }

// 辅助方法：缩放+灰度转换
    private Texture2D ResizeAndGrayscale(Texture2D source, int width, int height)
    {
        // 先缩放到目标尺寸
        var resized = ResizeTexture(source, width, height);

        var pixels = resized.GetPixels32();
        for (var i = 0; i < pixels.Length; i++)
        {
            var gray = (byte)(0.299f * pixels[i].r + 0.587f * pixels[i].g + 0.114f * pixels[i].b);
            pixels[i] = new Color32(gray, gray, gray, pixels[i].a);
        }

        resized.SetPixels32(pixels);
        resized.Apply();
        return resized;
    }


    private Texture2D GaussianBlur(Texture2D source, int radius)
    {
        var rt1 = RenderTexture.GetTemporary(source.width, source.height);
        var rt2 = RenderTexture.GetTemporary(source.width, source.height);

        Graphics.Blit(source, rt1);

        var blurMat = new Material(Shader.Find("Hidden/GaussianBlur"));
        blurMat.SetFloat("_Radius", radius);

        Graphics.Blit(rt1, rt2, blurMat);
        var result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        RenderTexture.active = rt2;
        result.ReadPixels(new Rect(0, 0, rt2.width, rt2.height), 0, 0);
        result.Apply();

        RenderTexture.ReleaseTemporary(rt1);
        RenderTexture.ReleaseTemporary(rt2);
        RenderTexture.active = null;

        return result;
    }

    private Texture2D AddGaussianNoise(Texture2D source, float stdDev)
    {
        var noisy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        var pixels = source.GetPixels32();
        var rnd = new Random();

        for (var i = 0; i < pixels.Length; i++)
        {
            var noise = (float)GaussianRandom(rnd, 0, stdDev) / 255f;
            var gray = pixels[i].r / 255f;

            var noisyGray = Mathf.Clamp01(gray + noise);
            var val = (byte)(noisyGray * 255f);
            pixels[i] = new Color32(val, val, val, 255);
        }

        noisy.SetPixels32(pixels);
        noisy.Apply();
        return noisy;
    }

    private double GaussianRandom(Random rnd, double mean, double stdDev)
    {
        // Box-Muller transform
        var u1 = 1.0 - rnd.NextDouble();
        var u2 = 1.0 - rnd.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                            Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }

    private Texture2D RotateTexture(Texture2D source, float angle)
    {
        var width = source.width;
        var height = source.height;

        var rotated = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var srcPixels = source.GetPixels32();
        var rotatedPixels = new Color32[srcPixels.Length];

        var rad = angle * Mathf.Deg2Rad;
        var cos = Mathf.Cos(rad);
        var sin = Mathf.Sin(rad);

        var cx = width / 2;
        var cy = height / 2;

        var white = new Color32(255, 255, 255, 255);

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var tx = x - cx;
            var ty = y - cy;

            var rx = Mathf.RoundToInt(cos * tx - sin * ty) + cx;
            var ry = Mathf.RoundToInt(sin * tx + cos * ty) + cy;

            if (rx >= 0 && rx < width && ry >= 0 && ry < height)
                rotatedPixels[y * width + x] = srcPixels[ry * width + rx];
            else
                rotatedPixels[y * width + x] = white;
        }

        rotated.SetPixels32(rotatedPixels);
        rotated.Apply();

        return rotated;
    }

    private List<Texture2D> AugmentImage(Texture2D original)
    {
        var variants = new List<Texture2D> { original };

        // 旋转
        float[] angles = { -5f, 5f, 10f };
        foreach (var angle in angles)
            variants.Add(RotateTexture(original, angle));

        // 高斯噪声
        variants.Add(AddGaussianNoise(original, 5f));
        variants.Add(AddGaussianNoise(original, 10f));

        // 高斯模糊
        variants.Add(GaussianBlur(original, 1));

        // 缩放 + 居中白底
        float[] scales = { 0.8f, 1.2f, 1.4f };
        foreach (var scale in scales)
            variants.Add(ScaleAndCenter(original, scale));

        return variants;
    }

    private Texture2D ScaleAndCenter(Texture2D source, float scale)
    {
        var newW = Mathf.RoundToInt(source.width * scale);
        var newH = Mathf.RoundToInt(source.height * scale);

        var scaled = new Texture2D(newW, newH, TextureFormat.RGBA32, false);
        // 直接缩放（用RenderTexture更好）
        scaled = ResizeTexture(source, newW, newH);

        var canvas = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        var bg = Enumerable.Repeat(new Color32(255, 255, 255, 255), canvas.width * canvas.height).ToArray();
        canvas.SetPixels32(bg);

        var offsetX = (canvas.width - newW) / 2;
        var offsetY = (canvas.height - newH) / 2;

        for (var y = 0; y < newH; y++)
        for (var x = 0; x < newW; x++)
            canvas.SetPixel(x + offsetX, y + offsetY, scaled.GetPixel(x, y));

        canvas.Apply();
        return canvas;
    }

    private Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        RenderTexture.active = rt;

        // 使用Graphics.Blit进行缩放
        Graphics.Blit(source, rt);

        var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    private float[] Softmax(float[] logits)
    {
        var max = logits.Max();
        var sum = logits.Select(x => Mathf.Exp(x - max)).Sum();
        return logits.Select(x => Mathf.Exp(x - max) / sum).ToArray();
    }

    private int ArgMax(float[] array)
    {
        var maxIndex = 0;
        var maxValue = array[0];
        for (var i = 1; i < array.Length; i++)
            if (array[i] > maxValue)
            {
                maxValue = array[i];
                maxIndex = i;
            }

        return maxIndex;
    }
}