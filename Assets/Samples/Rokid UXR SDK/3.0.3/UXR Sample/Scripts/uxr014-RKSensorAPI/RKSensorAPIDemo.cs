using Newtonsoft.Json;
using Rokid.UXR.Module;
using Rokid.UXR.Native;
using UnityEngine;
using UnityEngine.UI;

namespace Rokid.UXR.Demo
{
    public class RKSensorAPIDemo : AutoInjectBehaviour
    {
        [SerializeField] [Autowrited] private Text cameraIntrinsices;

        [SerializeField] [Autowrited] private Text imuData;

        [SerializeField] [Autowrited] private Text trackingStatus;

        [SerializeField] [Autowrited] private Text cameraPose;

        [SerializeField] [Autowrited] private Text yuvImage;

        [SerializeField] [Autowrited] private Text imuFPS;

        [SerializeField] [Autowrited] private Text frustum;

        private bool deviceInited;
        private float fps = 60;
        private Pose mCameraPose;

        private void Start()
        {
            RKSensorAPI.Instance.Initialize(true, true);
            RKSensorAPI.OnUsbDeviceInited += OnUsbDeviceInited;
            RKSensorAPI.OnIMUDataCallBack += OnIMUDataCallBack;
            RKSensorAPI.OnIMUUpdate += OnIMUUpdate;
        }

        private void Update()
        {
            if (deviceInited)
            {
                var imageData = RKSensorAPI.Instance.GetYUVImage();
                if (imageData.success)
                    yuvImage.text = $"YUVImage timeStamp:{RKSensorAPI.Instance.GetYUVImage().timeStamp}";
                mCameraPose = NativeInterface.NativeAPI.GetCameraPhysicsPose(out var timeStamp);
                cameraPose.text =
                    $"CameraPose eulerAngles:{mCameraPose.rotation.eulerAngles},pos:{mCameraPose.position}";
                imuData.text = $"IMUData:{JsonUtility.ToJson(RKSensorAPI.Instance.GetIMUData())}";
            }

            trackingStatus.text = $"HeadTrackingStatus:{NativeInterface.NativeAPI.GetHeadTrackingStatus()}";
        }

        private void OnDestroy()
        {
            RKSensorAPI.OnUsbDeviceInited -= OnUsbDeviceInited;
            RKSensorAPI.OnIMUDataCallBack -= OnIMUDataCallBack;
            RKSensorAPI.OnIMUUpdate -= OnIMUUpdate;
        }

        private void OnIMUUpdate(float delta)
        {
            var text = "IMUFPS: ";
            var interp = delta / (0.5f + delta);
            var currentFPS = 1.0f / delta;
            fps = Mathf.Lerp(fps, currentFPS, interp);
            text += Mathf.RoundToInt(fps);
            imuFPS.text = text;
        }

        private void OnIMUDataCallBack(RKSensorAPI.IMUData imuData)
        {
            // this.imuData.text = $"IMUData:{JsonUtility.ToJson(imuData)}";
        }

        private void OnUsbDeviceInited()
        {
            deviceInited = true;
            cameraIntrinsices.text = $"CameraIntrinsices: {NativeInterface.NativeAPI.GetOSTInfo()}";
            var frustum_left = new float[4];
            var frustum_right = new float[4];
            NativeInterface.NativeAPI.GetUnityFrustum(ref frustum_left, ref frustum_right);
            frustum.text =
                $"frustum_left: {JsonConvert.SerializeObject(frustum_left)} \r\n frustum_right: {JsonConvert.SerializeObject(frustum_right)} ";
        }
    }
}