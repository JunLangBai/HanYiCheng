using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DragHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Transform targetObject; // 要移动的物体（通常是Canvas的父物体）
    [SerializeField] private Camera eventCamera;     // 用于坐标转换的摄像机

    private Vector3 dragStartPosition; // 记录拖拽起始位置
    private Vector3 objectStartPosition; // 目标物体的初始位置
    private bool isDragging;

    private void Start()
    {
        // 自动获取必要组件
        if (targetObject == null)
            targetObject = transform.root; // 默认移动根物体
        
        if (eventCamera == null)
            eventCamera = GetComponentInParent<Canvas>().worldCamera;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        
        // 记录初始位置
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventCamera,
            out dragStartPosition);

        objectStartPosition = targetObject.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                GetComponent<RectTransform>(),
                eventData.position,
                eventCamera,
                out Vector3 currentPosition))
        {
            // 计算世界空间中的位移差值
            Vector3 offset = currentPosition - dragStartPosition;
            targetObject.position = objectStartPosition + offset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    // 点击事件处理（通过UnityEvent绑定）
    public void OnClick()
    {
        if (!isDragging)
        {
            Debug.Log("Button Clicked!");
            // 你的点击处理逻辑
        }
    }
}