using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 在 RawImage 上实现“按下 + 拖拽绘制”
/// 已实现：
/// - 禁用默认 Drag 阈值（IInitializePotentialDragHandler）
/// - 用 eventData.position 取屏幕坐标，不再直接用 Input.mousePosition
/// - 不再用 >0.1f 的距离判断，保证只要 OnDrag 就一直画
/// </summary>
public class DrawingBoard : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IDragHandler,
    IInitializePotentialDragHandler   // 新增：用于禁用默认的拖拽阈值
{
    [Header("UI 组件")]
    public RawImage drawingImage;

    [Header("绘图设置")]
    public float brushSize = 5f;

    // 私有字段
    private Texture2D drawingTexture;
    private Color[] pixels;
    private bool isDrawing = false;
    private Vector2 previousTexCoord;

    private void Start()
    {
        // 初始化一个 256×256 的白底画布
        drawingTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
        drawingTexture.filterMode = FilterMode.Point;
        drawingTexture.wrapMode = TextureWrapMode.Clamp;

        pixels = new Color[drawingTexture.width * drawingTexture.height];
        ClearCanvas();
        drawingImage.texture = drawingTexture;
    }

    private void Update()
    {
        // Update 里不再做绘制逻辑，全部都放到 OnDrag 里处理
    }

    /// <summary>
    /// 当按下时，取得初始坐标并画一个圆点
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // eventData.pressEventCamera 会自动交给正确的 UI 摄像机（Screen Space – Overlay 下就是 null）
        Camera cam = eventData.pressEventCamera;
        Vector2 coord = GetTextureCoord(eventData.position, cam);
        if (!IsValidCoordinate(coord)) return;

        isDrawing = true;
        previousTexCoord = coord;

        // 先画一个圆点
        DrawCircle(previousTexCoord, brushSize);
        UpdateTexture();
    }

    /// <summary>
    /// 当拖拽时持续绘制
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDrawing) return;

        Camera cam = eventData.pressEventCamera;
        Vector2 coord = GetTextureCoord(eventData.position, cam);
        if (!IsValidCoordinate(coord))
        {
            // 如果拖出纹理范围，就停止这次绘制
            isDrawing = false;
            return;
        }

        // 这里移除“> 0.1f”判断，改为只要 OnDrag 就一直画
        // 如果两个 texCoord 恰好相同，DrawLine 会退化成在同一个点上画圆，也没有问题
        DrawLine(previousTexCoord, coord);
        previousTexCoord = coord;
        UpdateTexture();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrawing = false;
    }

    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerExit(PointerEventData eventData)  { }

    /// <summary>
    /// 屏蔽 Unity 默认的 Drag 阈值：只要 PointerDown 后稍微移动就触发 OnDrag
    /// </summary>
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    /// <summary>
    /// 把屏幕坐标 screenPoint（eventData.position）映射到绘图纹理的像素坐标
    /// 返回 (-1,-1) 表示“点不在 RawImage 上”
    /// </summary>
    private Vector2 GetTextureCoord(Vector2 screenPoint, Camera cam)
    {
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawingImage.rectTransform,
                screenPoint,
                cam,
                out localPoint))
        {
            return new Vector2(-1, -1);
        }

        Rect rect = drawingImage.rectTransform.rect;
        float w = rect.width;
        float h = rect.height;

        Vector2 uv = new Vector2(
            (localPoint.x + w * 0.5f) / w,
            (localPoint.y + h * 0.5f) / h
        );

        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
            return new Vector2(-1, -1);

        int texX = Mathf.FloorToInt(uv.x * drawingTexture.width);
        int texY = Mathf.FloorToInt(uv.y * drawingTexture.height);
        return new Vector2(texX, texY);
    }

    private bool IsValidCoordinate(Vector2 coord)
    {
        return coord.x >= 0 && coord.x < drawingTexture.width &&
               coord.y >= 0 && coord.y < drawingTexture.height;
    }

    private void DrawCircle(Vector2 center, float radius)
    {
        int cx = (int)center.x;
        int cy = (int)center.y;
        int r = Mathf.CeilToInt(radius);

        for (int x = Mathf.Max(0, cx - r); x < Mathf.Min(drawingTexture.width, cx + r); x++)
        {
            for (int y = Mathf.Max(0, cy - r); y < Mathf.Min(drawingTexture.height, cy + r); y++)
            {
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius)
                {
                    pixels[y * drawingTexture.width + x] = Color.black;
                }
            }
        }
    }

    private void DrawLine(Vector2 start, Vector2 end)
    {
        int x0 = (int)start.x, y0 = (int)start.y;
        int x1 = (int)end.x,   y1 = (int)end.y;

        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1,       sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawCircle(new Vector2(x0, y0), brushSize);
            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    private void UpdateTexture()
    {
        drawingTexture.SetPixels(pixels);
        drawingTexture.Apply();
    }

    public void ClearCanvas()
    {
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        UpdateTexture();
    }

    public Texture2D GetDrawingTexture() => drawingTexture;
}
