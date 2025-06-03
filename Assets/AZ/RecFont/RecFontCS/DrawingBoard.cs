using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawingBoard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public RawImage drawingImage;
    public Texture2D drawingTexture;
    public float brushSize = 5f;
    public Camera uiCamera;

    private bool isDrawing = false;
    private bool isPointerInside = false;
    private Vector2 previousMousePos;
    private Color[] pixels;

    private void Start()
    {
        drawingTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
        drawingTexture.filterMode = FilterMode.Point;
        drawingTexture.wrapMode = TextureWrapMode.Clamp;

        pixels = new Color[drawingTexture.width * drawingTexture.height];
        ClearCanvas();
        drawingImage.texture = drawingTexture;

        if (uiCamera == null) Debug.LogWarning("⚠️ uiCamera 未设置，绘图可能无法正常工作！");
    }

    private void Update()
    {
        if (!isDrawing) return;

        Vector2 currentCoord = GetTextureCoord();
        if (!IsValidCoordinate(currentCoord))
        {
            isDrawing = false;
            return;
        }

        if (Vector2.Distance(currentCoord, previousMousePos) > 0.1f)
        {
            DrawLine(previousMousePos, currentCoord);
            previousMousePos = currentCoord;
            UpdateTexture();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 coord = GetTextureCoord();
        if (!IsValidCoordinate(coord)) return;

        isDrawing = true;
        previousMousePos = coord;
        DrawCircle(previousMousePos, brushSize);
        UpdateTexture();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrawing = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
    }

    private Vector2 GetTextureCoord()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 localPoint;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawingImage.rectTransform,
            mousePos,
            uiCamera,
            out localPoint)) return new Vector2(-1, -1);

        Rect rect = drawingImage.rectTransform.rect;
        float width = rect.width;
        float height = rect.height;

        Vector2 uv = new Vector2(
            (localPoint.x + width * 0.5f) / width,
            (localPoint.y + height * 0.5f) / height
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
        int centerX = (int)center.x;
        int centerY = (int)center.y;
        int radiusInt = Mathf.CeilToInt(radius);

        for (int x = Mathf.Max(0, centerX - radiusInt); x < Mathf.Min(drawingTexture.width, centerX + radiusInt); x++)
        for (int y = Mathf.Max(0, centerY - radiusInt); y < Mathf.Min(drawingTexture.height, centerY + radiusInt); y++)
            if (Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2) <= Mathf.Pow(radius, 2))
                pixels[y * drawingTexture.width + x] = Color.black;
    }

    private void DrawLine(Vector2 start, Vector2 end)
    {
        int x0 = (int)start.x;
        int y0 = (int)start.y;
        int x1 = (int)end.x;
        int y1 = (int)end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
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
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        UpdateTexture();
    }

    public Texture2D GetDrawingTexture()
    {
        return drawingTexture;
    }
}
