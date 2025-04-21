using System.Collections.Generic;
using UnityEngine;
using Rokid.UXR.Interaction;
namespace Rokid.UXR.Demo
{
    public class RKHandInteractionDemo : MonoBehaviour
    {
        [SerializeField]
        private GrabInteractable[] interactableObjs;

        private List<Vector3> oriPositions = new List<Vector3>();

        void Start()
        {
            interactableObjs = GetComponentsInChildren<GrabInteractable>();
            for (int i = 0; i < interactableObjs.Length; i++)
            {
                oriPositions.Add(interactableObjs[i].transform.localPosition);
            }
        }

        void Update()
        {
            if (IsDoubleClick())
            {
                for (int i = 0; i < interactableObjs.Length; i++)
                {
                    Rigidbody rigidbody = interactableObjs[i].GetComponent<Rigidbody>();
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
        }


        #region IsDoubleClick
        float doubleClickTime = 0.5f;
        float clickTime = 0;
        int clickCount = 0;
        private bool IsDoubleClick()
        {
            clickTime += Time.deltaTime;
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                clickCount++;
            }
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
