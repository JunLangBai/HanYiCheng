using UnityEngine;
using UnityEngine.UI;

public class DrawingBoard : MonoBehaviour
{
    public RawImage drawingImage; // 用于显示绘制的RawImage
    public Texture2D drawingTexture; // 实际绘制的纹理
    public float brushSize = 5f; // 画笔大小
    public Camera uiCamera; // 渲染该UI的摄像机
    private bool isDrawing; // 是否正在绘制
    private Color[] pixels; // 像素数组

    private Vector2 previousMousePos; // 上一帧鼠标位置

    private void Start()
    {
        // 初始化纹理
        drawingTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
        drawingTexture.filterMode = FilterMode.Point;
        if (uiCamera == null) Debug.LogWarning("⚠️ uiCamera 未设置，绘图可能无法正常工作！");

        drawingTexture.wrapMode = TextureWrapMode.Clamp;
        pixels = new Color[drawingTexture.width * drawingTexture.height];
        ClearCanvas();
        drawingImage.texture = drawingTexture;
    }

    private void Update()
    {
        var currentCoord = GetTextureCoord();
        var isValidCoord = IsValidCoordinate(currentCoord);

        if (Input.GetMouseButtonDown(0))
        {
            // 仅在有效坐标时开始绘制
            if (isValidCoord)
            {
                isDrawing = true;
                previousMousePos = currentCoord;
                DrawCircle(previousMousePos, brushSize);
                UpdateTexture();
            }
            else
            {
                isDrawing = false; // 确保点击外部不开始绘制
            }
        }
        else if (Input.GetMouseButton(0) && isDrawing)
        {
            // 如果当前坐标无效，停止绘制
            if (!isValidCoord)
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
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }
    }

    // 获取鼠标在纹理上的坐标
    private Vector2 GetTextureCoord()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 localPoint;

        var success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawingImage.rectTransform,
            mousePos,
            uiCamera,
            out localPoint);

        // 如果转换失败，返回无效坐标
        if (!success) return new Vector2(-1, -1);

        var rect = drawingImage.rectTransform.rect;
        var width = rect.width;
        var height = rect.height;

        var uv = new Vector2(
            (localPoint.x + width * 0.5f) / width,
            (localPoint.y + height * 0.5f) / height
        );

        // 直接判断UV是否在0-1范围内
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
            return new Vector2(-1, -1);

        var texX = Mathf.FloorToInt(uv.x * drawingTexture.width);
        var texY = Mathf.FloorToInt(uv.y * drawingTexture.height);

        return new Vector2(texX, texY);
    }


    private bool IsValidCoordinate(Vector2 coord)
    {
        return coord.x >= 0 && coord.x < drawingTexture.width &&
               coord.y >= 0 && coord.y < drawingTexture.height;
    }


    // 绘制圆形画笔
    private void DrawCircle(Vector2 center, float radius)
    {
        var centerX = (int)center.x;
        var centerY = (int)center.y;
        var radiusInt = Mathf.CeilToInt(radius);

        for (var x = Mathf.Max(0, centerX - radiusInt); x < Mathf.Min(drawingTexture.width, centerX + radiusInt); x++)
        for (var y = Mathf.Max(0, centerY - radiusInt); y < Mathf.Min(drawingTexture.height, centerY + radiusInt); y++)
            if (Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2) <= Mathf.Pow(radius, 2))
                pixels[y * drawingTexture.width + x] = Color.black;
    }

    // 绘制两点之间的线段
    private void DrawLine(Vector2 start, Vector2 end)
    {
        // 使用Bresenham算法绘制直线
        var x0 = (int)start.x;
        var y0 = (int)start.y;
        var x1 = (int)end.x;
        var y1 = (int)end.y;

        var dx = Mathf.Abs(x1 - x0);
        var dy = Mathf.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = dx - dy;

        while (true)
        {
            DrawCircle(new Vector2(x0, y0), brushSize);

            if (x0 == x1 && y0 == y1) break;

            var e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    // 更新纹理
    private void UpdateTexture()
    {
        drawingTexture.SetPixels(pixels);
        drawingTexture.Apply();
    }

    // 清空画布
    public void ClearCanvas()
    {
        for (var i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        UpdateTexture();
    }

    // 获取绘制纹理
    public Texture2D GetDrawingTexture()
    {
        return drawingTexture;
    }
}