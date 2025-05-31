using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DragHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private bool isDragging;
    private RectTransform rectTransform;

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
        var delta = eventData.delta / canvas.scaleFactor;
        var newPosition = rectTransform.anchoredPosition + delta;

        var canvasRectTransform = canvas.GetComponent<RectTransform>();
        var canvasRect = canvasRectTransform.rect;
        var elementRect = rectTransform.rect;

        var parentWidth = canvasRect.width;
        var parentHeight = canvasRect.height;
        var elementWidth = elementRect.width;
        var elementHeight = elementRect.height;
        var pivot = rectTransform.pivot;

        // 计算X轴的边界限制
        var minX = -parentWidth / 2f + elementWidth * pivot.x;
        var maxX = parentWidth / 2f - elementWidth * (1f - pivot.x);

        // 计算Y轴的边界限制
        var minY = -parentHeight / 2f + elementHeight * pivot.y;
        var maxY = parentHeight / 2f - elementHeight * (1f - pivot.y);

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
        if (!isDragging) Debug.Log("Button Clicked!");
        // 点击处理逻辑
    }
}