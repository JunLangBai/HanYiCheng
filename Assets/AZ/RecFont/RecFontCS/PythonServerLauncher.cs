using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using TensorFlowLite;

public class HangulInference : MonoBehaviour
{
    [SerializeField] private string labelFileName = "labels.txt";
    [SerializeField] private string modelFileName = "model.tflite";

    private string[] labels;
    private Interpreter interpreter;
    private float[] inputBuffer;
    private float[] outputBuffer;

    void Start()
    {
        StartCoroutine(LoadLabelsAsync());
        StartCoroutine(LoadModelAsync());
    }

    private IEnumerator LoadLabelsAsync()
    {
        var labelPath = Path.Combine(Application.streamingAssetsPath, labelFileName);
        Debug.Log($"加载标签文件: {labelPath}");

        UnityWebRequest request = UnityWebRequest.Get(labelPath);
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

        string text = request.downloadHandler.text;
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
            var options = new InterpreterOptions() { threads = 1 };
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
            List<string> predictions = new List<string>();

            foreach (var img in variants)
            {
                // 预处理，转为模型输入格式的一维float数组
                var vector = Preprocess(img);
                inputBuffer = vector;  // 直接赋值

                // 填充输入tensor并推理
                interpreter.SetInputTensorData(0, inputBuffer);
                interpreter.Invoke();

                // 获取输出结果
                interpreter.GetOutputTensorData(0, outputBuffer);

                // softmax概率计算，选最大概率索引
                float[] probs = Softmax(outputBuffer);
                int index = ArgMax(probs);
                predictions.Add(labels[index]);
            }

            // 多版本预测结果投票，返回最多的那个label
            string finalPrediction = predictions
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
        Texture2D gray64 = ResizeAndGrayscale(input, 64, 64);

        // 2. 读取像素并归一化反色
        Color32[] pixels = gray64.GetPixels32();
        float[] vector = new float[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            // 灰度值在r/g/b三通道相同
            float gray = pixels[i].r / 255f;
            vector[i] = 1f - gray;  // 反色
        }

        return vector;
    }

// 辅助方法：缩放+灰度转换
    private Texture2D ResizeAndGrayscale(Texture2D source, int width, int height)
    {
        // 先缩放到目标尺寸
        Texture2D resized = ResizeTexture(source, width, height);

        Color32[] pixels = resized.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
        {
            byte gray = (byte)(0.299f * pixels[i].r + 0.587f * pixels[i].g + 0.114f * pixels[i].b);
            pixels[i] = new Color32(gray, gray, gray, pixels[i].a);
        }

        resized.SetPixels32(pixels);
        resized.Apply();
        return resized;
    }


    private Texture2D GaussianBlur(Texture2D source, int radius)
    {
        RenderTexture rt1 = RenderTexture.GetTemporary(source.width, source.height);
        RenderTexture rt2 = RenderTexture.GetTemporary(source.width, source.height);

        Graphics.Blit(source, rt1);

        Material blurMat = new Material(Shader.Find("Hidden/GaussianBlur"));
        blurMat.SetFloat("_Radius", radius);

        Graphics.Blit(rt1, rt2, blurMat);
        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
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
        Texture2D noisy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        Color32[] pixels = source.GetPixels32();
        System.Random rnd = new System.Random();

        for (int i = 0; i < pixels.Length; i++)
        {
            float noise = (float)GaussianRandom(rnd, 0, stdDev) / 255f;
            float gray = pixels[i].r / 255f;

            float noisyGray = Mathf.Clamp01(gray + noise);
            byte val = (byte)(noisyGray * 255f);
            pixels[i] = new Color32(val, val, val, 255);
        }

        noisy.SetPixels32(pixels);
        noisy.Apply();
        return noisy;
    }

    private double GaussianRandom(System.Random rnd, double mean, double stdDev)
    {
        // Box-Muller transform
        double u1 = 1.0 - rnd.NextDouble();
        double u2 = 1.0 - rnd.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                               Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }
    private Texture2D RotateTexture(Texture2D source, float angle)
    {
        int width = source.width;
        int height = source.height;

        Texture2D rotated = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] srcPixels = source.GetPixels32();
        Color32[] rotatedPixels = new Color32[srcPixels.Length];

        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        int cx = width / 2;
        int cy = height / 2;

        Color32 white = new Color32(255, 255, 255, 255);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int tx = x - cx;
                int ty = y - cy;

                int rx = Mathf.RoundToInt(cos * tx - sin * ty) + cx;
                int ry = Mathf.RoundToInt(sin * tx + cos * ty) + cy;

                if (rx >= 0 && rx < width && ry >= 0 && ry < height)
                    rotatedPixels[y * width + x] = srcPixels[ry * width + rx];
                else
                    rotatedPixels[y * width + x] = white;
            }
        }

        rotated.SetPixels32(rotatedPixels);
        rotated.Apply();

        return rotated;
    }
    private List<Texture2D> AugmentImage(Texture2D original)
    {
        List<Texture2D> variants = new List<Texture2D> { original };

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
        int newW = Mathf.RoundToInt(source.width * scale);
        int newH = Mathf.RoundToInt(source.height * scale);

        Texture2D scaled = new Texture2D(newW, newH, TextureFormat.RGBA32, false);
        // 直接缩放（用RenderTexture更好）
        scaled = ResizeTexture(source, newW, newH);

        Texture2D canvas = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        Color32[] bg = Enumerable.Repeat(new Color32(255, 255, 255, 255), canvas.width * canvas.height).ToArray();
        canvas.SetPixels32(bg);

        int offsetX = (canvas.width - newW) / 2;
        int offsetY = (canvas.height - newH) / 2;

        for (int y = 0; y < newH; y++)
        {
            for (int x = 0; x < newW; x++)
            {
                canvas.SetPixel(x + offsetX, y + offsetY, scaled.GetPixel(x, y));
            }
        }

        canvas.Apply();
        return canvas;
    }
    private Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        RenderTexture.active = rt;

        // 使用Graphics.Blit进行缩放
        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }
    private float[] Softmax(float[] logits)
    {
        float max = logits.Max();
        float sum = logits.Select(x => Mathf.Exp(x - max)).Sum();
        return logits.Select(x => Mathf.Exp(x - max) / sum).ToArray();
    }

    private int ArgMax(float[] array)
    {
        int maxIndex = 0;
        float maxValue = array[0];
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] > maxValue)
            {
                maxValue = array[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    void OnDestroy()
    {
        interpreter?.Dispose();
    }
}
