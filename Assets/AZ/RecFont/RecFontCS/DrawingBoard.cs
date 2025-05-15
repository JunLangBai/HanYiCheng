using UnityEngine;
using UnityEngine.UI;

public class DrawingBoard : MonoBehaviour
{
    public RawImage drawingImage;       // 用于显示绘制的RawImage
    public Texture2D drawingTexture;    // 实际绘制的纹理
    private Color[] pixels;             // 像素数组
    public float brushSize = 5f;        // 画笔大小
    public Camera uiCamera;             // 渲染该UI的摄像机
    
    private Vector2 previousMousePos;   // 上一帧鼠标位置
    private bool isDrawing = false;     // 是否正在绘制

    void Start()
    {
        // 初始化纹理
        drawingTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
        drawingTexture.filterMode = FilterMode.Point;
        drawingTexture.wrapMode = TextureWrapMode.Clamp;
        pixels = new Color[drawingTexture.width * drawingTexture.height];
        ClearCanvas();
        drawingImage.texture = drawingTexture;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            previousMousePos = GetTextureCoord();
            DrawCircle(previousMousePos, brushSize);
            UpdateTexture();
        }
        else if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector2 currentMousePos = GetTextureCoord();
            
            // 只在鼠标移动了一定距离后才绘制，避免重复绘制相同位置
            if (Vector2.Distance(currentMousePos, previousMousePos) > 0.1f)
            {
                DrawLine(previousMousePos, currentMousePos);
                previousMousePos = currentMousePos;
                UpdateTexture();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }
    }

    // 获取鼠标在纹理上的坐标
    Vector2 GetTextureCoord()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 localPoint;
        
        // 转换屏幕坐标到UI的本地坐标
        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawingImage.rectTransform, 
            mousePos, 
            uiCamera, 
            out localPoint);

        if (!success) return Vector2.negativeInfinity;

        // 获取UI元素的尺寸
        Rect rect = drawingImage.rectTransform.rect;
        float width = rect.width;
        float height = rect.height;

        // 转换为UV坐标（0到1范围）
        Vector2 uv = new Vector2(
            (localPoint.x + width * 0.5f) / width,
            (localPoint.y + height * 0.5f) / height
        );

        // 转换为纹理坐标
        int texX = Mathf.Clamp((int)(uv.x * drawingTexture.width), 0, drawingTexture.width - 1);
        int texY = Mathf.Clamp((int)(uv.y * drawingTexture.height), 0, drawingTexture.height - 1);

        return new Vector2(texX, texY);
    }

    bool IsValidCoordinate(Vector2 coord)
    {
        return coord.x >= 0 && coord.x < drawingTexture.width && 
               coord.y >= 0 && coord.y < drawingTexture.height;
    }

    // 绘制圆形画笔
    void DrawCircle(Vector2 center, float radius)
    {
        int centerX = (int)center.x;
        int centerY = (int)center.y;
        int radiusInt = Mathf.CeilToInt(radius);
        
        for (int x = Mathf.Max(0, centerX - radiusInt); x < Mathf.Min(drawingTexture.width, centerX + radiusInt); x++)
        {
            for (int y = Mathf.Max(0, centerY - radiusInt); y < Mathf.Min(drawingTexture.height, centerY + radiusInt); y++)
            {
                if (Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2) <= Mathf.Pow(radius, 2))
                {
                    pixels[y * drawingTexture.width + x] = Color.black;
                }
            }
        }
    }

    // 绘制两点之间的线段
    void DrawLine(Vector2 start, Vector2 end)
    {
        // 使用Bresenham算法绘制直线
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
    void UpdateTexture()
    {
        drawingTexture.SetPixels(pixels);
        drawingTexture.Apply();
    }

    // 清空画布
    public void ClearCanvas()
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        UpdateTexture();
    }

    // 获取绘制纹理
    public Texture2D GetDrawingTexture()
    {
        return drawingTexture;
    }
}