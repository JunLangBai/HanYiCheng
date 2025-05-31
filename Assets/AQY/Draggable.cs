using System.Collections.Generic;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    ///     Draggable 拖拽组件
    /// </summary>
    // [RequireComponent(typeof(RayInteractable))]
    public class Draggable : MonoBehaviour, IBezierCurveDrag, IRayBeginDrag, IRayDragToTarget, IRayEndDrag, IDraggable
    {
        [SerializeField] [Tooltip("The min point to relative to the camera")]
        private Vector3 minPoint = new(-2, -0.8f, -2);

        [SerializeField] [Tooltip("The farthest point to relative to the camera")]
        private Vector3 maxPoint = new(2, 1.2f, 2);

        [SerializeField] [Tooltip("The drag obj is look at camera")]
        private bool lookAtCamera;

        [SerializeField] [Tooltip("The drag obj is clamp in target filed")]
        private bool clampInTargetFiled;

        [SerializeField] [Tooltip("The obj follow this drag obj")]
        private Transform followObj;

        [SerializeField] [Tooltip("The drag smooth speed")]
        private int smoothSpeed = 10;

        [SerializeField] [Tooltip("The look at change threshold")]
        private float lookAtChangeThreshold = 0.3f;

        private readonly Dictionary<int, BezierPointerData> bezierPointerDatas = new();
        private bool dragging;
        private Vector3 offsetPos, dragOffset;

        private void Start()
        {
            if (followObj != null) offsetPos = transform.InverseTransformPoint(followObj.position);
        }


        public bool IsEnablePinchBezierCurve()
        {
            return true && enabled;
        }

        public bool IsEnableGripBezierCurve()
        {
            return true && enabled;
        }

        public bool IsInBezierCurveDragging()
        {
            return dragging && enabled;
        }

        public Vector3 GetBezierCurveEndPoint(int pointerId)
        {
            var pointerData = GetBezierPointerData(pointerId);
            if (pointerData != null) return transform.TransformPoint(pointerData.hitLocalPos);

            return transform.TransformPoint(Vector3.zero);
        }

        public Vector3 GetBezierCurveEndNormal(int pointerId)
        {
            var pointerData = GetBezierPointerData(pointerId);
            if (pointerData != null) return transform.TransformPoint(pointerData.hitLocalNormal);

            return transform.TransformPoint(Vector3.zero);
        }

        public void OnRayBeginDrag(PointerEventData eventData)
        {
            AddBezierPointerData(eventData);
            dragging = true;
        }

        public void OnRayDragToTarget(Vector3 targetPoint)
        {
            if (enabled == false)
                return;
            ProcessDrag(targetPoint);
        }

        public void OnRayEndDrag(PointerEventData eventData)
        {
            dragging = false;
            RemoveBezierPointerData(eventData);
        }

        public void SetOrUpdateFollowObj(Transform followObj)
        {
            this.followObj = followObj;
            offsetPos = transform.InverseTransformPoint(followObj.position);
        }

        private void AddBezierPointerData(PointerEventData eventData)
        {
            var bezierPointerData = new BezierPointerData
            {
                pointerId = eventData.pointerId,
                hitLocalNormal = transform.InverseTransformVector(eventData.pointerCurrentRaycast.worldNormal),
                hitLocalPos = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition)
            };
            if (bezierPointerDatas.ContainsKey(bezierPointerData.pointerId))
                bezierPointerDatas[bezierPointerData.pointerId] = bezierPointerData;
            else
                bezierPointerDatas.Add(bezierPointerData.pointerId, bezierPointerData);
        }

        private void RemoveBezierPointerData(PointerEventData eventData)
        {
            bezierPointerDatas.Remove(eventData.pointerId);
        }

        private BezierPointerData GetBezierPointerData(int pointerId)
        {
            if (bezierPointerDatas.TryGetValue(pointerId, out var bezierPointerData)) return bezierPointerData;

            RKLog.KeyInfo($"====Draggable====: Can not find pointerId {pointerId}");
            return null;
        }

        private void ProcessDrag(Vector3 targetPos)
        {
            var cameraPos = MainCameraCache.mainCamera.transform.position;
            if (clampInTargetFiled)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minPoint.x + cameraPos.x, maxPoint.x + cameraPos.x);
                targetPos.y = Mathf.Clamp(targetPos.y, minPoint.y + cameraPos.y, maxPoint.y + cameraPos.y);
                targetPos.z = Mathf.Clamp(targetPos.z, minPoint.z + cameraPos.z, maxPoint.z + cameraPos.z);
            }

            transform.position = Vector3.Slerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
            if (lookAtCamera)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
                if (transform.position.y > lookAtChangeThreshold + cameraPos.y ||
                    transform.position.y < -lookAtChangeThreshold + cameraPos.y)
                {
                    var forward = transform.position - cameraPos;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forward),
                        smoothSpeed * Time.deltaTime);
                }
                else
                {
                    var forward = transform.position - cameraPos;
                    forward.y = 0;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forward),
                        smoothSpeed * Time.deltaTime);
                }
            }

            if (followObj != null)
            {
                followObj.position = transform.TransformPoint(offsetPos);
                followObj.rotation = transform.rotation;
            }
        }
    }
}