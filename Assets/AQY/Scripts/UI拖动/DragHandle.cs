using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DragHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isDragging;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.delta / canvas.scaleFactor;
        Vector2 newPosition = rectTransform.anchoredPosition + delta;

        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
        Rect canvasRect = canvasRectTransform.rect;
        Rect elementRect = rectTransform.rect;

        float parentWidth = canvasRect.width;
        float parentHeight = canvasRect.height;
        float elementWidth = elementRect.width;
        float elementHeight = elementRect.height;
        Vector2 pivot = rectTransform.pivot;

        // 计算X轴的边界限制
        float minX = (-parentWidth / 2f) + (elementWidth * pivot.x);
        float maxX = (parentWidth / 2f) - (elementWidth * (1f - pivot.x));

        // 计算Y轴的边界限制
        float minY = (-parentHeight / 2f) + (elementHeight * pivot.y);
        float maxY = (parentHeight / 2f) - (elementHeight * (1f - pivot.y));

        // 应用限制
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        rectTransform.anchoredPosition = newPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnClick()
    {
        if (!isDragging)
        {
            Debug.Log("Button Clicked!");
            // 点击处理逻辑
        }
    }
}