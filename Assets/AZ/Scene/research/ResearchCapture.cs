using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Independent handwriting board for the research scene. The visible board
/// follows the original level's white-paper/black-pen appearance. Saving
/// converts it to the experiment pipeline's black background, bright strokes,
/// 64 x 64 JPEG (RGB pixels with equal R/G/B values).
/// </summary>
[RequireComponent(typeof(RawImage))]
public sealed class ResearchCapture : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler,
    IInitializePotentialDragHandler
{
    [Serializable]
    private sealed class PredictionResponse
    {
        public string label;
    }

    private const int BoardSize = 256;
    private const int ModelSize = 64;

    [Header("Scene UI")]
    public RawImage drawingImage;
    public Button finishButton;
    public Button clearButton;
    public TMP_Text statusText;

    [Header("Sample annotation (optional)")]
    public string participantId = "anonymous";
    public string groundTruthLabel = "";
    public bool clearAfterSave = true;

    [Header("Optional cloud inference (off until deployed)")]
    [Tooltip("Save locally first, then POST the same 64x64 JPEG to serverUrl.")]
    public bool uploadAfterSave = false;
    [Tooltip("Full HTTPS /predict URL of the final model. Leave empty until the server is ready.")]
    public string serverUrl = "";
    [Tooltip("Request timeout in seconds; local saving is unaffected by network errors.")]
    public int timeoutSeconds = 15;

    private Texture2D board;
    private Color32[] pixels;
    private bool drawing;
    private bool hasInk;
    private Vector2Int previous;
    private string outputDirectory;

    private void Awake()
    {
        if (drawingImage == null) drawingImage = GetComponent<RawImage>();
        board = new Texture2D(BoardSize, BoardSize, TextureFormat.RGB24, false);
        board.filterMode = FilterMode.Bilinear;
        board.wrapMode = TextureWrapMode.Clamp;
        pixels = new Color32[BoardSize * BoardSize];
        drawingImage.texture = board;
        drawingImage.color = Color.white;
        drawingImage.raycastTarget = true;
        ClearBoard();

#if UNITY_EDITOR
        outputDirectory = Path.Combine(Application.dataPath, "AZ", "Scene", "research", "Samples");
#else
        // Assets is read-only inside an Android APK. This is the equivalent
        // writable folder on the Rokid/Android device.
        outputDirectory = Path.Combine(Application.persistentDataPath, "research", "Samples");
#endif
        if (finishButton != null) finishButton.onClick.AddListener(SaveSample);
        if (clearButton != null) clearButton.onClick.AddListener(ClearBoard);
        SetStatus("请在框内写字");
    }

    private void OnDestroy()
    {
        if (finishButton != null) finishButton.onClick.RemoveListener(SaveSample);
        if (clearButton != null) clearButton.onClick.RemoveListener(ClearBoard);
        if (board != null) Destroy(board);
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2Int point;
        if (!TryMapPoint(eventData, out point)) return;
        drawing = true;
        previous = point;
        DrawDisk(point.x, point.y, 5);
        ApplyPixels();
        hasInk = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!drawing) return;
        Vector2Int point;
        if (!TryMapPoint(eventData, out point))
        {
            drawing = false;
            return;
        }
        DrawLine(previous, point);
        previous = point;
        ApplyPixels();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        drawing = false;
    }

    private bool TryMapPoint(PointerEventData eventData, out Vector2Int point)
    {
        point = default(Vector2Int);
        if (board == null || drawingImage == null) return false;
        Vector2 local;
        RectTransform rectTransform = drawingImage.rectTransform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, eventData.position, eventData.pressEventCamera, out local))
            return false;
        Rect rect = rectTransform.rect;
        if (!rect.Contains(local) || rect.width <= 0f || rect.height <= 0f)
            return false;
        int x = Mathf.Clamp(Mathf.FloorToInt((local.x - rect.xMin) / rect.width * BoardSize), 0, BoardSize - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt((local.y - rect.yMin) / rect.height * BoardSize), 0, BoardSize - 1);
        point = new Vector2Int(x, y);
        return true;
    }

    private void DrawDisk(int centerX, int centerY, int radius)
    {
        for (int y = Mathf.Max(0, centerY - radius); y <= Mathf.Min(BoardSize - 1, centerY + radius); y++)
        for (int x = Mathf.Max(0, centerX - radius); x <= Mathf.Min(BoardSize - 1, centerX + radius); x++)
        {
            int dx = x - centerX;
            int dy = y - centerY;
            if (dx * dx + dy * dy <= radius * radius)
                pixels[y * BoardSize + x] = new Color32(0, 0, 0, 255);
        }
    }

    private void DrawLine(Vector2Int from, Vector2Int to)
    {
        int x = from.x, y = from.y;
        int dx = Mathf.Abs(to.x - x), dy = Mathf.Abs(to.y - y);
        int sx = x < to.x ? 1 : -1, sy = y < to.y ? 1 : -1;
        int error = dx - dy;
        while (true)
        {
            DrawDisk(x, y, 5);
            if (x == to.x && y == to.y) break;
            int twice = 2 * error;
            if (twice > -dy) { error -= dy; x += sx; }
            if (twice < dx) { error += dx; y += sy; }
        }
    }

    private void ApplyPixels()
    {
        board.SetPixels32(pixels);
        board.Apply(false, false);
    }

    public void ClearBoard()
    {
        if (pixels == null) return;
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(255, 255, 255, 255);
        ApplyPixels();
        hasInk = false;
        drawing = false;
        SetStatus("画板已清空");
    }

    private byte[] EncodeModelJpeg()
    {
        Texture2D result = new Texture2D(ModelSize, ModelSize, TextureFormat.RGB24, false);
        Color32[] modelPixels = new Color32[ModelSize * ModelSize];
        const int scale = BoardSize / ModelSize;
        for (int y = 0; y < ModelSize; y++)
        for (int x = 0; x < ModelSize; x++)
        {
            int sum = 0;
            for (int sy = 0; sy < scale; sy++)
            for (int sx = 0; sx < scale; sx++)
                sum += pixels[(y * scale + sy) * BoardSize + x * scale + sx].r;
            byte ink = (byte)(255 - Mathf.RoundToInt(sum / (float)(scale * scale)));
            modelPixels[y * ModelSize + x] = new Color32(ink, ink, ink, 255);
        }
        result.SetPixels32(modelPixels);
        result.Apply(false, false);
        byte[] jpeg = result.EncodeToJPG(100);
        Destroy(result);
        return jpeg;
    }

    public void SaveSample()
    {
        if (!hasInk)
        {
            SetStatus("请先写字");
            return;
        }
        try
        {
            Directory.CreateDirectory(outputDirectory);
            string stamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmssfff'Z'", CultureInfo.InvariantCulture);
            string name = "hangul_" + stamp + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".jpg";
            string path = Path.Combine(outputDirectory, name);
            byte[] jpeg = EncodeModelJpeg();
            File.WriteAllBytes(path, jpeg);

            string manifest = Path.Combine(outputDirectory, "samples.csv");
            if (!File.Exists(manifest))
                File.AppendAllText(manifest, "filename,label,participant_id,utc_time,width,height,format\n", new UTF8Encoding(false));
            File.AppendAllText(manifest,
                Csv(name) + "," + Csv(groundTruthLabel.Trim()) + "," + Csv(participantId.Trim()) + "," +
                Csv(DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)) + ",64,64,jpeg\n",
                new UTF8Encoding(false));

            Debug.Log("Research image saved: " + path);
            SetStatus("已保存");
            if (clearAfterSave) ClearBoardAfterSave();
            if (uploadAfterSave)
            {
                Uri endpoint;
                if (!TryGetServerEndpoint(out endpoint))
                {
                    Debug.LogWarning("Research upload enabled, but serverUrl is not an absolute HTTP(S) URL: " + serverUrl);
                    SetStatus("服务器地址无效");
                }
                else
                {
                    SetStatus("上传中");
                    StartCoroutine(Upload(endpoint.AbsoluteUri, jpeg, name));
                }
            }
        }
        catch (Exception error)
        {
            Debug.LogError("Research image save failed: " + error);
            SetStatus("保存失败");
        }
    }

    private void ClearBoardAfterSave()
    {
        ClearBoard();
        SetStatus("已保存");
    }

    private bool TryGetServerEndpoint(out Uri endpoint)
    {
        endpoint = null;
        if (string.IsNullOrWhiteSpace(serverUrl)) return false;
        if (!Uri.TryCreate(serverUrl.Trim(), UriKind.Absolute, out endpoint)) return false;
        return endpoint.Scheme == Uri.UriSchemeHttps || endpoint.Scheme == Uri.UriSchemeHttp;
    }

    private IEnumerator Upload(string endpoint, byte[] jpeg, string name)
    {
        List<IMultipartFormSection> form = new List<IMultipartFormSection>
        {
            new MultipartFormFileSection("image", jpeg, name, "image/jpeg")
        };
        using (UnityWebRequest request = UnityWebRequest.Post(endpoint, form))
        {
            request.timeout = Mathf.Max(1, timeoutSeconds);
            float start = Time.realtimeSinceStartup;
            yield return request.SendWebRequest();
            float elapsedMs = (Time.realtimeSinceStartup - start) * 1000f;
            string response = request.downloadHandler != null ? request.downloadHandler.text : "";
            string error = request.result == UnityWebRequest.Result.Success ? "" : request.error;
            string report = Path.Combine(outputDirectory, "server_responses.csv");
            try
            {
                if (!File.Exists(report))
                    File.AppendAllText(report, "filename,elapsed_ms,http_code,response,error\n", new UTF8Encoding(false));
                File.AppendAllText(report,
                    Csv(name) + "," + elapsedMs.ToString("F1", CultureInfo.InvariantCulture) + "," +
                    request.responseCode.ToString(CultureInfo.InvariantCulture) + "," + Csv(response) + "," + Csv(error) + "\n",
                    new UTF8Encoding(false));
            }
            catch (Exception writeError)
            {
                Debug.LogError("Could not save server response: " + writeError);
            }
            Debug.Log("Research server result for " + name + ": " + response + " / " + error);
            if (!string.IsNullOrEmpty(error)) SetStatus("上传失败");
            else
            {
                PredictionResponse prediction = null;
                try { prediction = JsonUtility.FromJson<PredictionResponse>(response); }
                catch (ArgumentException) { /* Other server response formats remain in CSV. */ }
                SetStatus(prediction != null && !string.IsNullOrEmpty(prediction.label)
                    ? "预测: " + prediction.label : "服务器已返回");
            }
        }
    }

    private static string Csv(string value)
    {
        return "\"" + (value ?? "").Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " ") + "\"";
    }

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}
