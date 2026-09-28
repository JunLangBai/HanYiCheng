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
        public int class_id = -1;
        public string labels_sha256;
    }

    [Serializable]
    private sealed class ServerConfiguration
    {
        public string server_url;
        public string api_token;
        public bool upload_after_save;
        public bool allow_http_for_testing;
        public int timeout_seconds = 30;
    }

    private const int BoardSize = 256;
    private const int ModelSize = 64;

    [Header("Scene UI")]
    public RawImage drawingImage;
    public Button finishButton;
    public Button clearButton;
    public TMP_Text statusText;

    [Header("Guided collection (training vocabulary)")]
    public bool guidedCollection = true;
    [Tooltip("Exact 2350-label training table; never use the old 2351-entry table with <rare>.")]
    public TextAsset labelFile;
    public TMP_FontAsset promptFont;
    [Tooltip("Leave empty for all 2350 classes. Or enter characters, e.g. 가 나 다 한 글. IDs remain the model IDs.")]
    [TextArea(1, 4)] public string collectionCharacters = "";
    [Tooltip("Optional first character, e.g. 한. It must belong to the collection list. No need to enter its numeric ID.")]
    public string startCharacter = "";
    [Tooltip("Off: keep collecting the same character. On: next character after a successful save and clear.")]
    public bool advanceAfterSave = false;

    [Header("Sample annotation")]
    [Tooltip("Optional initial anonymous ID. Normally use the in-scene New Participant button; anonymous cannot save.")]
    public string participantId = "anonymous";
    public string sessionId = "S01";
    [Tooltip("Legacy free-writing target, used only when guidedCollection is off. A prompt is not a verified ground truth.")]
    public string groundTruthLabel = "";
    public bool clearAfterSave = true;

    [Header("Optional cloud inference (off until deployed)")]
    [Tooltip("Save locally first, then POST the same 64x64 JPEG to serverUrl.")]
    public bool uploadAfterSave = false;
    [Tooltip("Full HTTPS /predict URL of the final model. Leave empty until the server is ready.")]
    public string serverUrl = "";
    [Tooltip("Request timeout in seconds; local saving is unaffected by network errors.")]
    public int timeoutSeconds = 15;
    [Tooltip("Temporary synthetic-data testing only. Prefer HTTPS; never disable certificate verification.")]
    public bool allowHttpForTesting = false;

    private string apiToken = ""; // Loaded from persistent storage, never serialized into a scene/APK.
    private bool uploadInProgress;

    private Texture2D board;
    private Color32[] pixels;
    private bool drawing;
    private bool hasInk;
    private Vector2Int previous;
    private string outputDirectory;
    private string[] labels;
    private int[] collectionIds;
    private int targetPosition;
    private bool vocabularyReady;
    private TMP_Text targetCharacterText;
    private TMP_Text targetInfoText;
    private Button previousTargetButton;
    private Button nextTargetButton;
    private int initialTargetPosition;
    private ResearchParticipantRegistry participants;
    private string activeParticipantId = "";
    private string activeSessionId = "S01";
    private TMP_Text participantInfoText;
    private Button newParticipantButton;
    private Button resumeParticipantButton;
    private GameObject participantDialog;
    private TMP_Text participantDialogText;
    private string pendingParticipantId;
    private bool participantDialogOpen;

    public int CurrentTargetId { get { return vocabularyReady && guidedCollection ? collectionIds[targetPosition] : -1; } }
    public string CurrentTargetCharacter { get { return CurrentTargetId >= 0 ? labels[CurrentTargetId] : ""; } }
    public string ActiveParticipantId { get { return activeParticipantId; } }
    public int CurrentParticipantSampleCount { get { return participants != null ? participants.CountFor(activeParticipantId) : 0; } }
    public int SampledParticipantCount { get { return participants != null ? participants.SampledParticipantCount : 0; } }
    public string OutputDirectory { get { return outputDirectory; } }

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

        try
        {
#if UNITY_EDITOR
            outputDirectory = ResearchStoragePaths.EditorSamplesDirectory(this);
#else
            // Android APK assets are read-only. Preserve the existing device
            // directory so an updated APK can continue using its old records.
            outputDirectory = ResearchStoragePaths.PlayerSamplesDirectory(Application.persistentDataPath);
#endif
            Debug.Log("Research samples directory: " + outputDirectory, this);
        }
        catch (Exception error)
        {
            if (finishButton != null) finishButton.interactable = false;
            SetStatus("无法定位采集目录，请检查控制台");
            Debug.LogError("Research storage configuration error: " + error.Message, this);
            enabled = false;
            return;
        }
        if (finishButton != null) finishButton.onClick.AddListener(SaveSample);
        if (clearButton != null) clearButton.onClick.AddListener(ClearBoard);
        InitializeTargets();
        CreateParticipantUI();
        InitializeParticipants();
        LoadServerConfiguration();
    }

    public static string ServerConfigurationPath
    {
        get { return Path.Combine(Application.persistentDataPath, "research", "research_server.json"); }
    }

    [ContextMenu("Reload server configuration")]
    public void LoadServerConfiguration()
    {
        if (uploadInProgress) return;
        apiToken = "";
        // A missing/invalid private config must not silently enable anonymous uploads.
        uploadAfterSave = false;
        Debug.Log("Research server configuration path: " + ServerConfigurationPath);
        if (!File.Exists(ServerConfigurationPath)) return;
        try
        {
            ServerConfiguration config = JsonUtility.FromJson<ServerConfiguration>(File.ReadAllText(ServerConfigurationPath, Encoding.UTF8));
            if (config == null || !ValidApiToken(config.api_token)) throw new ArgumentException("Invalid server configuration.");
            serverUrl = (config.server_url ?? "").Trim();
            apiToken = config.api_token;
            allowHttpForTesting = config.allow_http_for_testing;
            timeoutSeconds = Mathf.Clamp(config.timeout_seconds, 5, 120);
            Uri endpoint;
            if (!TryGetServerEndpoint(out endpoint)) throw new ArgumentException("Invalid URL or HTTP is not explicitly allowed.");
            uploadAfterSave = config.upload_after_save;
            if (uploadAfterSave && endpoint.Scheme == Uri.UriSchemeHttp)
                Debug.LogWarning("Research HTTP test mode exposes image/token in transit. Use dummy data and an IP allowlist; upgrade to HTTPS before participant collection.");
        }
        catch (Exception)
        {
            apiToken = "";
            uploadAfterSave = false;
            Debug.LogError("Research cloud configuration invalid. Local saving remains available. Check the private research_server.json file; do not post its token to logs/chat.");
        }
    }

    private static bool ValidApiToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length < 32 || token.StartsWith("REPLACE", StringComparison.Ordinal)) return false;
        foreach (char ch in token) if (ch < 33 || ch > 126) return false;
        return true;
    }

    private void OnDestroy()
    {
        if (finishButton != null) finishButton.onClick.RemoveListener(SaveSample);
        if (clearButton != null) clearButton.onClick.RemoveListener(ClearBoard);
        if (previousTargetButton != null) previousTargetButton.onClick.RemoveListener(PreviousTarget);
        if (nextTargetButton != null) nextTargetButton.onClick.RemoveListener(NextTarget);
        if (newParticipantButton != null) newParticipantButton.onClick.RemoveListener(BeginNewParticipant);
        if (resumeParticipantButton != null) resumeParticipantButton.onClick.RemoveListener(ResumeLastParticipant);
        if (board != null) ReleaseTexture(board);
    }

    private void InitializeTargets()
    {
        try
        {
            labels = ResearchLabelCatalog.Parse(labelFile != null ? labelFile.text : null);
            collectionIds = ResearchLabelCatalog.BuildPlan(labels, collectionCharacters);
            targetPosition = 0;
            if (guidedCollection && !string.IsNullOrWhiteSpace(startCharacter))
            {
                int id = Array.IndexOf(labels, startCharacter.Trim().Normalize(NormalizationForm.FormC));
                targetPosition = Array.IndexOf(collectionIds, id);
                if (id < 0 || targetPosition < 0)
                    throw new ArgumentException("Start character must be in the collection list: " + startCharacter);
            }
            if (guidedCollection && promptFont == null)
                throw new ArgumentException("Assign the Korean prompt font before collection.");
            if (guidedCollection)
                foreach (int id in collectionIds)
                    if (!promptFont.HasCharacter((int)labels[id][0]))
                        throw new ArgumentException("Prompt font cannot display character: " + labels[id]);
            vocabularyReady = true;
            initialTargetPosition = targetPosition;
            if (guidedCollection) CreatePromptUI();
            UpdateTargetUI();
            SetStatus(guidedCollection ? "按上方提示书写" : "请在框内写字");
        }
        catch (Exception error)
        {
            vocabularyReady = false;
            if (finishButton != null) finishButton.interactable = false;
            SetStatus("标签或提示配置错误，请检查控制台");
            Debug.LogError("Research collection configuration error: " + error.Message, this);
        }
    }

    private void CreateParticipantUI()
    {
        Transform parent = drawingImage.transform.parent;
        TMP_FontAsset font = statusText != null ? statusText.font : TMP_Settings.defaultFontAsset;
        participantInfoText = CreateText(parent, "ResearchParticipantInfo", new Vector2(-125, 239), new Vector2(350, 78), font, 18);
        newParticipantButton = CreateButton(parent, "ResearchNewParticipant", "新参与者", new Vector2(200, 253), font);
        resumeParticipantButton = CreateButton(parent, "ResearchResumeParticipant", "继续上次", new Vector2(200, 205), font);
        Layout(newParticipantButton.GetComponent<RectTransform>(), new Vector2(200, 253), new Vector2(260, 40));
        Layout(resumeParticipantButton.GetComponent<RectTransform>(), new Vector2(200, 205), new Vector2(260, 40));
        newParticipantButton.onClick.AddListener(BeginNewParticipant);
        resumeParticipantButton.onClick.AddListener(ResumeLastParticipant);

        participantDialog = new GameObject("ResearchParticipantDialog", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        participantDialog.layer = parent.gameObject.layer;
        participantDialog.transform.SetParent(parent, false);
        RectTransform overlay = participantDialog.GetComponent<RectTransform>();
        overlay.anchorMin = Vector2.zero; overlay.anchorMax = Vector2.one;
        overlay.offsetMin = overlay.offsetMax = Vector2.zero;
        participantDialog.GetComponent<Image>().color = new Color(0.94f, 0.97f, 1f, 0.98f);
        participantDialogText = CreateText(overlay, "Message", new Vector2(0, 55), new Vector2(660, 140), font, 25);
        CreateButton(overlay, "Confirm", "确认新建", new Vector2(-95, -65), font).onClick.AddListener(ConfirmNewParticipant);
        CreateButton(overlay, "Cancel", "取消", new Vector2(95, -65), font).onClick.AddListener(CancelNewParticipant);
        participantDialog.SetActive(false);
    }

    private void InitializeParticipants()
    {
        activeParticipantId = "";
        try
        {
            ReloadParticipants();
            string configured = (participantId ?? "").Trim();
            if (ResearchParticipantRegistry.IsValidId(configured))
            {
                activeParticipantId = configured;
                activeSessionId = ResearchParticipantRegistry.IsValidId((sessionId ?? "").Trim()) ? sessionId.Trim() : "S01";
            }
            if (vocabularyReady && string.IsNullOrEmpty(activeParticipantId)) SetStatus("请先新建参与者或继续上次");
        }
        catch (Exception error) { ParticipantFailure(error); }
        UpdateParticipantUI();
    }

    private void ReloadParticipants()
    {
        participants = new ResearchParticipantRegistry(outputDirectory);
        participants.Reload(activeParticipantId.Length > 0 ? activeParticipantId : (participantId ?? "").Trim());
    }

    private bool CanSwitchParticipant()
    {
        if (hasInk)
        {
            SetStatus("请先保存或清除笔迹，再换参与者");
            return false;
        }
        return vocabularyReady && !participantDialogOpen;
    }

    public void BeginNewParticipant()
    {
        if (!CanSwitchParticipant()) return;
        try
        {
            ReloadParticipants();
            pendingParticipantId = participants.NextId;
            participantDialogText.text = "确认换成一位新的书写者？\n将创建 " + pendingParticipantId + " / S01\n同一个人继续采集，不要重复新建。";
            participantDialogOpen = true;
            participantDialog.SetActive(true);
            participantDialog.transform.SetAsLastSibling();
        }
        catch (Exception error) { ParticipantFailure(error); }
        UpdateParticipantUI();
    }

    public void ConfirmNewParticipant()
    {
        if (!participantDialogOpen || hasInk || !vocabularyReady) return;
        try
        {
            string id = participants.Create(pendingParticipantId, activeParticipantId);
            activeParticipantId = participantId = id;
            activeSessionId = sessionId = "S01";
            CancelNewParticipant();
            ClearBoard();
            // Every new writer starts at the configured first character.
            targetPosition = initialTargetPosition;
            UpdateTargetUI();
            UpdateParticipantUI();
            SetStatus("已开始采集 " + id);
        }
        catch (Exception error)
        {
            CancelNewParticipant();
            ParticipantFailure(error);
        }
    }

    public void CancelNewParticipant()
    {
        participantDialogOpen = false;
        pendingParticipantId = null;
        if (participantDialog != null) participantDialog.SetActive(false);
        UpdateParticipantUI();
    }

    public void ResumeLastParticipant()
    {
        if (!CanSwitchParticipant()) return;
        try
        {
            ReloadParticipants();
            if (!ResearchParticipantRegistry.IsValidId(participants.LastParticipant))
            {
                SetStatus("没有可继续的参与者，请新建");
                UpdateParticipantUI();
                return;
            }
            activeParticipantId = participantId = participants.LastParticipant;
            activeSessionId = sessionId = participants.SessionFor(activeParticipantId);
            UpdateParticipantUI();
            SetStatus("继续采集 " + activeParticipantId);
        }
        catch (Exception error) { ParticipantFailure(error); }
    }

    private void ParticipantFailure(Exception error)
    {
        participants = null; // Fail closed rather than risk allocating a duplicate ID.
        Debug.LogError("Research participant records could not be read/written: " + error, this);
        SetStatus("参与者记录异常，请检查存储与控制台");
        UpdateParticipantUI();
    }

    private void UpdateParticipantUI()
    {
        bool ready = participants != null && ResearchParticipantRegistry.IsValidId(activeParticipantId);
        if (participantInfoText != null)
        {
            participantInfoText.text = (ready ? "当前参与者 " + activeParticipantId + " / " + activeSessionId : "尚未选择参与者") +
                "\n本参与者已保存 " + CurrentParticipantSampleCount + " 张\n本机已有样本的参与者 " + SampledParticipantCount + " 人";
            if (participants == null) participantInfoText.text = "参与者记录读取失败\n请检查存储与控制台";
        }
        if (finishButton != null) finishButton.interactable = vocabularyReady && ready && !participantDialogOpen && !uploadInProgress;
        if (newParticipantButton != null) newParticipantButton.interactable = vocabularyReady && !participantDialogOpen;
        if (resumeParticipantButton != null)
            resumeParticipantButton.interactable = vocabularyReady && participants != null && !participantDialogOpen &&
                ResearchParticipantRegistry.IsValidId(participants.LastParticipant) &&
                !string.Equals(activeParticipantId, participants.LastParticipant, StringComparison.OrdinalIgnoreCase);
    }

    // These are siblings of the RawImage, not part of the saved pixel buffer.
    // Runtime construction also upgrades already-saved research scenes without
    // rebuilding them from Level1-1 or altering any shared font asset.
    private void CreatePromptUI()
    {
        Transform parent = drawingImage.transform.parent;
        TMP_FontAsset uiFont = statusText != null ? statusText.font : TMP_Settings.defaultFontAsset;
        TMP_Text heading = CreateText(parent, "ResearchTargetHeading", new Vector2(200, 157), new Vector2(280, 30), uiFont, 20);
        heading.text = "请写下面的韩文字";
        targetCharacterText = CreateText(parent, "ResearchTargetCharacter", new Vector2(200, 99), new Vector2(270, 84), promptFont, 68);
        targetInfoText = CreateText(parent, "ResearchTargetInfo", new Vector2(200, 34), new Vector2(280, 46), uiFont, 17);
        previousTargetButton = CreateButton(parent, "ResearchPreviousTarget", "上一字", new Vector2(132, -15), uiFont);
        nextTargetButton = CreateButton(parent, "ResearchNextTarget", "下一字", new Vector2(268, -15), uiFont);
        previousTargetButton.onClick.AddListener(PreviousTarget);
        nextTargetButton.onClick.AddListener(NextTarget);
        if (finishButton != null) Layout(finishButton.GetComponent<RectTransform>(), new Vector2(200, -69), new Vector2(260, 52));
        if (clearButton != null) Layout(clearButton.GetComponent<RectTransform>(), new Vector2(200, -122), new Vector2(260, 42));
        if (statusText != null)
        {
            Layout(statusText.rectTransform, new Vector2(200, -170), new Vector2(286, 42));
            statusText.enableAutoSizing = true;
            statusText.fontSizeMin = 13;
            statusText.fontSizeMax = 18;
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.raycastTarget = false;
        }
    }

    private static void Layout(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition3D = new Vector3(position.x, position.y, 0);
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static TMP_Text CreateText(Transform parent, string name, Vector2 position, Vector2 size, TMP_FontAsset font, float fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        TMP_Text text = go.GetComponent<TMP_Text>();
        Layout(text.rectTransform, position, size);
        text.font = font;
        text.fontSize = fontSize;
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.enableWordWrapping = true;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string caption, Vector2 position, TMP_FontAsset font)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        Layout(go.GetComponent<RectTransform>(), position, new Vector2(124, 38));
        Image background = go.GetComponent<Image>();
        background.color = new Color(0.83f, 0.91f, 0.97f, 1);
        Button button = go.GetComponent<Button>();
        button.targetGraphic = background;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        TMP_Text text = CreateText(go.transform, "Label", Vector2.zero, new Vector2(118, 34), font, 20);
        text.text = caption;
        return button;
    }

    private void UpdateTargetUI()
    {
        if (!guidedCollection || !vocabularyReady) return;
        if (targetCharacterText != null) targetCharacterText.text = CurrentTargetCharacter;
        if (targetInfoText != null)
            targetInfoText.text = "ID " + CurrentTargetId + "  |  U+" + ((int)CurrentTargetCharacter[0]).ToString("X4") +
                "\n" + (targetPosition + 1) + " / " + collectionIds.Length;
        if (previousTargetButton != null) previousTargetButton.interactable = collectionIds.Length > 1;
        if (nextTargetButton != null) nextTargetButton.interactable = collectionIds.Length > 1;
    }

    public void PreviousTarget() { MoveTarget(-1); }
    public void NextTarget() { MoveTarget(1); }

    private void MoveTarget(int direction)
    {
        if (!guidedCollection || !vocabularyReady || participantDialogOpen) return;
        if (hasInk)
        {
            SetStatus("请先保存或清除，再换字");
            return;
        }
        targetPosition = (targetPosition + direction + collectionIds.Length) % collectionIds.Length;
        UpdateTargetUI();
        SetStatus("按上方提示书写");
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (participantDialogOpen || participants == null || !ResearchParticipantRegistry.IsValidId(activeParticipantId))
        {
            SetStatus("请先新建参与者或继续上次");
            return;
        }
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
        if (pixels == null || participantDialogOpen) return;
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
        ReleaseTexture(result);
        return jpeg;
    }

    private static void ReleaseTexture(Texture2D texture)
    {
        if (Application.isPlaying) Destroy(texture);
        else DestroyImmediate(texture); // Allows isolated editor smoke checks.
    }

    public void SaveSample()
    {
        if (uploadInProgress) { SetStatus("请等待上一张返回后再保存"); return; }
        if (participantDialogOpen) return;
        if (!vocabularyReady)
        {
            SetStatus("标签配置错误，不能保存");
            return;
        }
        if (participants == null || !ResearchParticipantRegistry.IsValidId(activeParticipantId))
        {
            SetStatus("请先新建参与者或继续上次");
            return;
        }
        if (!hasInk)
        {
            SetStatus("请先写字");
            return;
        }
        try
        {
            // Snapshot before clearing, navigating or asynchronously uploading.
            string targetLabel = guidedCollection ? CurrentTargetCharacter : (groundTruthLabel ?? "").Trim().Normalize(NormalizationForm.FormC);
            int targetId = string.IsNullOrEmpty(targetLabel) ? -1 : Array.IndexOf(labels, targetLabel);
            if (!string.IsNullOrEmpty(targetLabel) && targetId < 0)
                throw new ArgumentException("The target is outside the trained vocabulary: " + targetLabel);
            Directory.CreateDirectory(outputDirectory);
            string manifest = Path.Combine(outputDirectory, "samples_guided_v2.csv");
            if (File.Exists(manifest))
                using (var reader = new StreamReader(manifest, Encoding.UTF8))
                    if (reader.ReadLine() != ResearchLabelCatalog.ManifestHeader)
                        throw new IOException("Unexpected CSV schema. Existing records were not modified: " + manifest);
            DateTime capturedAt = DateTime.UtcNow;
            string stamp = capturedAt.ToString("yyyyMMdd'T'HHmmssfff'Z'", CultureInfo.InvariantCulture);
            string idPart = targetId >= 0 ? targetId.ToString("D4", CultureInfo.InvariantCulture) : "unlabeled";
            string name = "hangul_" + idPart + "_" + stamp + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".jpg";
            string path = Path.Combine(outputDirectory, name);
            byte[] jpeg = EncodeModelJpeg();
            File.WriteAllBytes(path, jpeg);

            if (!File.Exists(manifest))
                File.WriteAllText(manifest, ResearchLabelCatalog.ManifestHeader + "\n", new UTF8Encoding(true));
            File.AppendAllText(manifest,
                Csv(name) + "," + Csv(targetLabel) + "," + (targetId >= 0 ? targetId.ToString(CultureInfo.InvariantCulture) : "") +
                ",,," + (targetId >= 0 ? "pending_review" : "unlabeled") + "," + Csv(activeParticipantId) + "," +
                Csv(activeSessionId) + "," + Csv(capturedAt.ToString("o", CultureInfo.InvariantCulture)) +
                ",64,64,jpeg," + ResearchLabelCatalog.ExpectedSha256 + "\n",
                new UTF8Encoding(false));

            participants.RecordSaved(activeParticipantId, activeSessionId);
            UpdateParticipantUI();
            Debug.Log("Research image saved: " + path);
            SetStatus("已保存");
            if (clearAfterSave) ClearBoardAfterSave();
            if (guidedCollection && advanceAfterSave && clearAfterSave) NextTarget();
            if (uploadAfterSave)
            {
                Uri endpoint;
                if (!TryGetServerEndpoint(out endpoint) || !ValidApiToken(apiToken))
                {
                    Debug.LogWarning("Research cloud configuration is not ready; image has been saved locally.");
                    SetStatus("已本地保存，服务器配置无效");
                }
                else
                {
                    SetStatus("上传中");
                    StartCoroutine(Upload(endpoint.AbsoluteUri, jpeg, name, targetId, activeParticipantId));
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
        if (!string.IsNullOrEmpty(endpoint.UserInfo) || !string.IsNullOrEmpty(endpoint.Query) || !string.IsNullOrEmpty(endpoint.Fragment)) return false;
        return endpoint.Scheme == Uri.UriSchemeHttps || (endpoint.Scheme == Uri.UriSchemeHttp && allowHttpForTesting);
    }

    private IEnumerator Upload(string endpoint, byte[] jpeg, string name, int submittedTargetId, string submittedParticipant)
    {
        uploadInProgress = true;
        UpdateParticipantUI();
        try
        {
        List<IMultipartFormSection> form = new List<IMultipartFormSection>
        {
            // Do not leak the target class encoded in the local filename.
            new MultipartFormFileSection("image", jpeg, "sample_" + Guid.NewGuid().ToString("N") + ".jpg", "image/jpeg")
        };
        using (UnityWebRequest request = UnityWebRequest.Post(endpoint, form))
        {
            request.SetRequestHeader("Authorization", "Bearer " + apiToken);
            request.redirectLimit = 0; // Never forward the token through an unexpected redirect.
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
            Debug.Log("Research server HTTP " + request.responseCode + "; local result logged for " + name);
            if (!string.IsNullOrEmpty(error)) SetStatus(request.responseCode == 401 ? "令牌无效，图片已保存" :
                request.responseCode == 503 ? "服务器忙，图片已保存" : "上传失败，图片已保存");
            else
            {
                PredictionResponse prediction = null;
                try { prediction = JsonUtility.FromJson<PredictionResponse>(response); }
                catch (ArgumentException) { /* Other server response formats remain in CSV. */ }
                bool valid = prediction != null && prediction.labels_sha256 == ResearchLabelCatalog.ExpectedSha256 &&
                    prediction.class_id >= 0 && prediction.class_id < labels.Length && labels[prediction.class_id] == prediction.label;
                SetStatus(valid ? submittedParticipant + " / ID " + submittedTargetId + " 预测: " + prediction.label : "返回标签不匹配，请检查服务");
            }
        }
        }
        finally
        {
            uploadInProgress = false;
            UpdateParticipantUI();
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
