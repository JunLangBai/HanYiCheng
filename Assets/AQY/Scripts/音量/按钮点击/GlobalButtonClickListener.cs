using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GlobalButtonClickListener : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        var clickedObject = eventData.pointerCurrentRaycast.gameObject;
        if (clickedObject != null)
        {
            var btn = clickedObject.GetComponentInParent<Button>();
            if (btn != null && btn.interactable) // 确保按钮可交互
            {
                Debug.Log("Clicked Button");
                AudioManager.Instance.PlayButtonSound();
            }
        }
    }
}