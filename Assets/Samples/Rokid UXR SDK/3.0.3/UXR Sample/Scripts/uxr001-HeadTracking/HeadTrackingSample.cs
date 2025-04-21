using UnityEngine;
using UnityEngine.UI;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;
using Rokid.UXR.Native;

namespace Rokid.UXR.Demo
{
    public class HeadTrackingSample : AutoInjectBehaviour
    {
        [Autowrited]
        private Text infoTxt;
        [Autowrited]
        private Text engineTxt;
        [Autowrited]
        private Button zeroDofButton;
        [Autowrited]
        private Button threeDofButton;
        [Autowrited]
        private Button sixDofButton;
        [Autowrited]
        private Text slamTrackingStatus;
        [Autowrited]
        private Text slamImageQuality;
        [Autowrited]
        private Text slamKineticQuality;

        [Autowrited]
        private Toggle mixThreeDof;
        [Autowrited]
        private Button recenterMixThreeDof;

        private RKCameraRig cameraRig;

        public Camera mainCamera;


        void Start()
        {
            // Configures the app to not shut down the screen
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            NativeInterface.NativeAPI.Recenter();  // reset glass 3dof
            if (mainCamera == null)
            {
                mainCamera = MainCameraCache.mainCamera;
            }

            cameraRig = mainCamera.GetComponent<RKCameraRig>();
            zeroDofButton.onClick.AddListener(() =>
            {
                cameraRig.headTrackingType = RKCameraRig.HeadTrackingType.ZeroDof;
            });

            threeDofButton.onClick.AddListener(() =>
            {
                cameraRig.headTrackingType = RKCameraRig.HeadTrackingType.RotationOnly;
            });

            sixDofButton.onClick.AddListener(() =>
            {
                cameraRig.headTrackingType = RKCameraRig.HeadTrackingType.RotationAndPosition;
            });

            mixThreeDof.onValueChanged.AddListener(isOn =>
            {
                cameraRig.mixThreeDof = isOn;
            });

            recenterMixThreeDof.onClick.AddListener(() =>
            {
                MixThreeDof.Instance.Recenter();
            });
        }

        private void Update()
        {
            if (RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_RESET)) //长按虚拟面板 HOME键事件
            {
                RKLog.Info("UXR-UNITY::KEY_RESET");
                NativeInterface.NativeAPI.Recenter();
            }

            infoTxt.text = string.Format("Position:{0}\r\nEuler:{1}\r\nRotation:{2}", mainCamera.transform.position.ToString("f3"), mainCamera.transform.rotation.eulerAngles.ToString(), mainCamera.transform.rotation.ToString("f3"));

            engineTxt.text = $"DebugInfo:{NativeInterface.NativeAPI.GetHeadTrackingStatus()},{NativeInterface.NativeAPI.GetDebugInfo()}";


            NativeInterface.NativeAPI.GetSLAMQuality(out SlamTrackingStatus trackingStatus, out SlamImageQuality imageQuality, out SlamKineticQuality kineticQuality);
            slamTrackingStatus.text = $"SlamTrackingStatus:{trackingStatus}";
            slamImageQuality.text = $"SlamImageQuality:{imageQuality}";
            slamKineticQuality.text = $"SlamKineticQuality:{kineticQuality}";
        }

        private void OnEnable()
        {
            RKLog.Info("-UXR-  HeadTrackingScene OnEnable");
        }

        private void OnBecameVisible()
        {
            RKLog.Info("-UXR-  HeadTrackingScene OnBecameVisible ");
        }

        private void OnDisable()
        {
            RKLog.Info("-UXR-  HeadTrackingScene OnDisable ");
        }
    }
}
