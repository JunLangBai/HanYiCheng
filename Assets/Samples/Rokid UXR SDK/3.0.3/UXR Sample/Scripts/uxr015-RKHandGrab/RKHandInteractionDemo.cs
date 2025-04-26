using System.Collections.Generic;
using Rokid.UXR.Interaction;
using UnityEngine;

namespace Rokid.UXR.Demo
{
    public class RKHandInteractionDemo : MonoBehaviour
    {
        [SerializeField] private GrabInteractable[] interactableObjs;

        private readonly List<Vector3> oriPositions = new();

        private void Start()
        {
            interactableObjs = GetComponentsInChildren<GrabInteractable>();
            for (var i = 0; i < interactableObjs.Length; i++)
                oriPositions.Add(interactableObjs[i].transform.localPosition);
        }

        private void Update()
        {
            if (IsDoubleClick())
                for (var i = 0; i < interactableObjs.Length; i++)
                {
                    var rigidbody = interactableObjs[i].GetComponent<Rigidbody>();
                    rigidbody.angularVelocity = Vector3.zero;

#if UNITY_6000_0_OR_NEWER
                    rigidbody.linearVelocity = Vector3.zero;
#else
                    rigidbody.velocity = Vector3.zero;
#endif
                    interactableObjs[i].transform.localPosition = oriPositions[i];
                    interactableObjs[i].transform.localRotation = Quaternion.identity;
                    interactableObjs[i].gameObject.SetActive(true);
                }
        }


        #region IsDoubleClick

        private readonly float doubleClickTime = 0.5f;
        private float clickTime;
        private int clickCount;

        private bool IsDoubleClick()
        {
            clickTime += Time.deltaTime;
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton0)) clickCount++;
            if (clickTime < doubleClickTime)
            {
                if (clickCount == 2)
                {
                    clickTime = 0;
                    clickCount = 0;
                    return true;
                }
            }
            else
            {
                clickCount = 0;
                clickTime = 0;
            }

            return false;
        }

        #endregion
    }
}