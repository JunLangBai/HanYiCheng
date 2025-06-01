using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DragHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private bool isDragging;
    private RectTransform canvasRectTransform;
    private Camera mainCamera;
    private Plane dragPlane;
    private Vector3 initialCanvasPosition;
    private Vector3 initialPointerPosition;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasRectTransform = canvas.GetComponent<RectTransform>();
        mainCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        
        // 创建拖拽平面（基于Canvas的朝向）
        dragPlane = new Plane(canvasRectTransform.forward, canvasRectTransform.position);
        
        // 记录初始位置
        initialCanvasPosition = canvasRectTransform.position;
        initialPointerPosition = GetPointerWorldPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // 获取当前指针的世界位置
        Vector3 currentPointerPosition = GetPointerWorldPosition(eventData);
        
        // 计算偏移量并更新Canvas位置
        Vector3 offset = currentPointerPosition - initialPointerPosition;
        canvasRectTransform.position = initialCanvasPosition + offset;
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

    /// <summary>
    /// 获取指针在拖拽平面上的世界坐标
    /// </summary>
    private Vector3 GetPointerWorldPosition(PointerEventData eventData)
    {
        // 创建射线
        Ray ray = eventData.pressEventCamera == null ? 
            Camera.main.ScreenPointToRay(eventData.position) : 
            eventData.pressEventCamera.ScreenPointToRay(eventData.position);
        
        // 获取射线与平面的交点
        float enter;
        if (dragPlane.Raycast(ray, out enter))
        {
            return ray.GetPoint(enter);
        }
        
        // 如果射线未命中平面，返回默认值
        return Vector3.zero;
    }
}