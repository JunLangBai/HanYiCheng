using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rokid.UXR.Interaction;

namespace Rokid.UXR.Demo
{
    [RequireComponent(typeof(Image))]
    public class DragMe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public bool dragOnSurfaces = true;

        private Dictionary<int, GameObject> m_DraggingIcons = new Dictionary<int, GameObject>();
        private Dictionary<int, RectTransform> m_DraggingPlanes = new Dictionary<int, RectTransform>();

        [SerializeField]
        private int pointerId;

        public void OnBeginDrag(PointerEventData eventData)
        {
            // RKLog.Debug($"====DragMe====: OnBeginDrag");
            var canvas = FindInParents<Canvas>(gameObject);
            if (canvas == null)
                return;

            if (pointerId != 0)
                return;

            this.pointerId = eventData.pointerId;


            // We have clicked something that can be dragged.
            // What we want to do is create an icon for this.
            m_DraggingIcons[eventData.pointerId] = new GameObject("icon");

            m_DraggingIcons[eventData.pointerId].transform.SetParent(canvas.transform, false);
            m_DraggingIcons[eventData.pointerId].transform.SetAsLastSibling();

            var image = m_DraggingIcons[eventData.pointerId].AddComponent<Image>();
            // The icon will be under the cursor.
            // We want it to be ignored by the event system.
            var group = m_DraggingIcons[eventData.pointerId].AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            image.sprite = GetComponent<Image>().sprite;
            image.color = GetComponent<Image>().color;
            image.transform.localScale = Vector3.one * 0.3f;
            image.SetNativeSize();

            if (dragOnSurfaces)
                m_DraggingPlanes[eventData.pointerId] = transform as RectTransform;
            else
                m_DraggingPlanes[eventData.pointerId] = canvas.transform as RectTransform;

            SetDraggedPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_DraggingIcons[eventData.pointerId] != null)
                SetDraggedPosition(eventData);
        }

        private void SetDraggedPosition(PointerEventData eventData)
        {
            if (dragOnSurfaces && eventData.pointerEnter != null && eventData.pointerEnter.transform as RectTransform != null)
                m_DraggingPlanes[eventData.pointerId] = eventData.pointerEnter.transform as RectTransform;

            var rt = m_DraggingIcons[eventData.pointerId].GetComponent<RectTransform>();
            Vector3 globalMousePos;
            if (ScreenPointToWorldPointInRectangle(m_DraggingPlanes[eventData.pointerId], out globalMousePos))
            {
                rt.position = globalMousePos;
                rt.rotation = m_DraggingPlanes[eventData.pointerId].rotation;
            }
        }

        public bool ScreenPointToWorldPointInRectangle(RectTransform rect, out Vector3 worldPoint)
        {
            worldPoint = Vector2.zero;

            Ray ray = RayInteractor.GetRayByIdentifier(pointerId);

            Plane plane = new Plane(rect.rotation * Vector3.back, rect.position);
            float enter = 0f;
            float num = Vector3.Dot(Vector3.Normalize(rect.position - ray.origin), plane.normal);
            if (num != 0f && !plane.Raycast(ray, out enter))
            {
                return false;
            }
            worldPoint = ray.GetPoint(enter);
            return true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (m_DraggingIcons[eventData.pointerId] != null)
                Destroy(m_DraggingIcons[eventData.pointerId]);

            m_DraggingIcons[eventData.pointerId] = null;
            this.pointerId = 0;
        }

        static public T FindInParents<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            var comp = go.GetComponent<T>();

            if (comp != null)
                return comp;

            var t = go.transform.parent;
            while (t != null && comp == null)
            {
                comp = t.gameObject.GetComponent<T>();
                t = t.parent;
            }
            return comp;
        }
    }

}
