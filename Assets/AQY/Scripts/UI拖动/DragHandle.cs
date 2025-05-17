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
        rectTransform.anchoredPosition += delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    // 按钮点击事件处理（通过 UnityEvent 绑定）
    public void OnClick()
    {
        if (!isDragging)
        {
            Debug.Log("Button Clicked!");
            // 在这里添加你的点击处理逻辑
        }
    }
}