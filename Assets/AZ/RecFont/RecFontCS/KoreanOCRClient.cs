using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class KoreanOCRClient : MonoBehaviour
{
    
    [Header("画板组件")]
    public DrawingBoard drawingBoard;
    [Header("UI 组件")]
    public RawImage targetRawImage; // 要识别的图片
    public Button recognizeButton;  // 识别按钮
    public TextMeshProUGUI resultText;         // 结果显示
    public Button clearButton; // 新增清除按钮
    
        
    [Header("服务器配置")]
    public string serverURL = "http://110.40.170.159:6666/predict";
    public int timeoutSeconds = 15;
    
    [Header("测试图片")]
    public Texture2D testTexture; // 可选：用于测试的图片
    
    private bool isRecognizing = false; // 识别状态标志
    private Coroutine currentRecognitionCoroutine; // 当前识别协程
    
    void Start()
    {
        // 绑定清除按钮事件
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearDrawingBoard);
        }
        
        // 绑定按钮事件
        if (recognizeButton != null)
        {
            recognizeButton.onClick.AddListener(StartRecognition);
        }
        
        // 初始化状态
        UpdateResult("就绪");
        
        // 如果有测试图片，加载到RawImage
        if (testTexture != null && targetRawImage != null)
        {
            targetRawImage.texture = testTexture;
        }
        
    }
    // 新增清除方法
    void ClearDrawingBoard()
    {
        if (drawingBoard != null)
        {
            drawingBoard.ClearCanvas();
            UpdateResult("画板已清空");
        }
        else
        {
            Debug.LogError("画板组件未赋值！");
        }
    }
    public void StartRecognition()
    {
        if (targetRawImage == null || targetRawImage.texture == null)
        {
            UpdateResult("错误：未找到图片");
            return;
        }
        
        // 如果正在识别，停止当前识别
        if (isRecognizing && currentRecognitionCoroutine != null)
        {
            StopCoroutine(currentRecognitionCoroutine);
            isRecognizing = false;
            UpdateResult("识别已取消");
            return;
        }
        
        currentRecognitionCoroutine = StartCoroutine(RecognizeImage());
    }
    
    IEnumerator RecognizeImage()
    {
        isRecognizing = true;
        UpdateResult("正在识别...");
        
        // 获取纹理
        Texture2D texture = GetTextureFromRawImage();
        if (texture == null)
        {
            UpdateResult("错误：无法获取纹理");
            FinishRecognition();
            yield break;
        }
        
        // 将纹理转换为PNG字节数组
        byte[] imageData = texture.EncodeToPNG();
        
        // 创建表单数据
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection("image", imageData, "image.png", "image/png"));
        
        // 发送请求
        using (UnityWebRequest request = UnityWebRequest.Post(serverURL, formData))
        {
            // 设置超时时间
            request.timeout = timeoutSeconds;
            
            // 添加请求开始时间
            float startTime = Time.time;
            
            yield return request.SendWebRequest();
            
            // 计算请求耗时
            float requestTime = Time.time - startTime;
            Debug.Log($"请求耗时: {requestTime:F2}秒");
            
            // 检查是否仍在识别状态（防止被取消）
            if (!isRecognizing)
            {
                yield break;
            }
            
            // 处理响应
            bool success = ProcessResponse(request);    
            
            // 无论成功失败，都结束识别状态
            FinishRecognition();
        }
    }
    
    bool ProcessResponse(UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                // 解析JSON响应
                string responseText = request.downloadHandler.text;
                Debug.Log($"服务器响应: {responseText}");
                
                // 检查响应是否为空
                if (string.IsNullOrEmpty(responseText))
                {
                    UpdateResult("识别失败：服务器返回空响应");
                    return false;
                }
                
                OCRResponse response = JsonUtility.FromJson<OCRResponse>(responseText);
                
                if (!string.IsNullOrEmpty(response.label))
                {
                    UpdateResult($"识别结果是: {response.label}");
                    Debug.Log($"OCR识别结果: {response.label}");
                    return true; // 成功
                }
                else if (!string.IsNullOrEmpty(response.error))
                {
                    UpdateResult($"识别失败: {response.error}");
                    Debug.LogError($"OCR错误: {response.error}");
                    return false;
                }
                else
                {
                    UpdateResult("识别失败：服务器返回格式错误");
                    Debug.LogError("服务器返回格式错误");
                    return false;
                }
            }
            catch (Exception e)
            {
                UpdateResult($"识别失败: JSON解析错误 - {e.Message}");
                Debug.LogError($"JSON解析错误: {e.Message}");
                Debug.LogError($"服务器响应: {request.downloadHandler.text}");
                return false;
            }
        }
        else
        {
            UpdateResult($"识别失败: 网络错误 - {request.error}");
            Debug.LogError($"网络请求失败: {request.error}");
            Debug.LogError($"响应码: {request.responseCode}");
            return false;
        }
    }
    
    void FinishRecognition()
    {
        isRecognizing = false;
        currentRecognitionCoroutine = null;
        Debug.Log("识别流程结束");
    }
    
    Texture2D GetTextureFromRawImage()
    {
        if (targetRawImage == null || targetRawImage.texture == null)
            return null;
        
        // 如果已经是Texture2D，直接返回
        if (targetRawImage.texture is Texture2D)
        {
            return targetRawImage.texture as Texture2D;
        }
        
        // 如果是RenderTexture，需要转换
        if (targetRawImage.texture is RenderTexture)
        {
            return ConvertRenderTextureToTexture2D(targetRawImage.texture as RenderTexture);
        }
        
        Debug.LogError("不支持的纹理类型");
        return null;
    }
    
    Texture2D ConvertRenderTextureToTexture2D(RenderTexture renderTexture)
    {
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        
        Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();
        
        RenderTexture.active = currentActiveRT;
        return texture2D;
    }
    
    void UpdateResult(string result)
    {
        if (resultText != null)
        {
            resultText.text = result;
        }
        Debug.Log($"OCR: {result}");
    }
    
    // 公共方法：从外部调用识别
    public void RecognizeTexture(Texture2D texture)
    {
        if (targetRawImage != null)
        {
            targetRawImage.texture = texture;
        }
        StartRecognition();
    }
    
    // 公共方法：设置服务器地址
    public void SetServerURL(string url)
    {
        serverURL = url;
    }
    
    // 公共方法：检查是否正在识别
    public bool IsRecognizing()
    {
        return isRecognizing;
    }
    
    // 公共方法：取消当前识别
    public void CancelRecognition()
    {
        if (isRecognizing && currentRecognitionCoroutine != null)
        {
            StopCoroutine(currentRecognitionCoroutine);
            FinishRecognition();
            UpdateResult("识别已取消");
        }
    }
    
    void OnDestroy()
    {
        // 清理协程
        if (currentRecognitionCoroutine != null)
        {
            StopCoroutine(currentRecognitionCoroutine);
        }
    }
    
    
}

// JSON响应数据结构
[System.Serializable]
public class OCRResponse
{
    public string label;
    public string error;
}