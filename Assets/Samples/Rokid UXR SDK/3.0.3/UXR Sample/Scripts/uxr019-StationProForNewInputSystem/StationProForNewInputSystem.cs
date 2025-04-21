using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

namespace Rokid.UXR.Demo
{
    public class StationProForNewInputSystem : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        public InputAction select;
        public InputAction cancel;
        public InputAction upArrow;
        public InputAction downArrow;
        public InputAction leftArrow;
        public InputAction rightArrow;
        public InputAction menu;
        public Text logText;

        private void OnEnable()
        {
            select.Enable();
            cancel.Enable();
            upArrow.Enable();
            downArrow.Enable();
            leftArrow.Enable();
            rightArrow.Enable();
            menu.Enable();
        }

        private void OnDisable()
        {
            select.Disable();
            cancel.Disable();
            upArrow.Disable();
            downArrow.Disable();
            leftArrow.Disable();
            rightArrow.Disable();
            menu.Disable();
        }

        private void Start()
        {
            select.performed += OnPerformed;
            cancel.performed += OnPerformed;
            upArrow.performed += OnPerformed;
            downArrow.performed += OnPerformed;
            leftArrow.performed += OnPerformed;
            rightArrow.performed += OnPerformed;
            menu.performed += OnPerformed;

            select.canceled += OnCanceled;
            cancel.canceled += OnCanceled;
            upArrow.canceled += OnCanceled;
            downArrow.canceled += OnCanceled;
            leftArrow.canceled += OnCanceled;
            rightArrow.canceled += OnCanceled;
            menu.canceled += OnCanceled;
        }

        private void OnDestroy()
        {
            select.performed -= OnPerformed;
            cancel.performed -= OnPerformed;
            upArrow.performed -= OnPerformed;
            downArrow.performed -= OnPerformed;
            leftArrow.performed -= OnPerformed;
            rightArrow.performed -= OnPerformed;
            menu.performed -= OnPerformed;

            select.canceled -= OnCanceled;
            cancel.canceled -= OnCanceled;
            upArrow.canceled -= OnCanceled;
            downArrow.canceled -= OnCanceled;
            leftArrow.canceled -= OnCanceled;
            rightArrow.canceled -= OnCanceled;
            menu.canceled -= OnCanceled;
        }

        private void OnPerformed(InputAction.CallbackContext context)
        {
            if (logText != null)
            {
                logText.text = context.ToString();
                RKLog.KeyInfo($"====StationProForNewInputSystem==== {context.action} preformed");
            }
        }


        private void OnCanceled(InputAction.CallbackContext context)
        {
            if (logText != null)
            {
                logText.text += context.ToString();
                RKLog.KeyInfo($"====StationProForNewInputSystem==== {context.action} canceled");
            }
        }
#endif
    }
}
