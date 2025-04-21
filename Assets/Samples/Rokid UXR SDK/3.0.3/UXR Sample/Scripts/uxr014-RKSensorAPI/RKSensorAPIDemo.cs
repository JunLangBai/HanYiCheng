using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using Rokid.UXR.Module;
using Rokid.UXR.Native;
namespace Rokid.UXR.Demo
{
    public class RKSensorAPIDemo : AutoInjectBehaviour
    {

        [SerializeField, Autowrited]
        private Text cameraIntrinsices;
        [SerializeField, Autowrited]
        private Text imuData;
        [SerializeField, Autowrited]
        private Text trackingStatus;
        [SerializeField, Autowrited]
        private Text cameraPose;
        [SerializeField, Autowrited]
        private Text yuvImage;
        [SerializeField, Autowrited]
        private Text imuFPS;
        [SerializeField, Autowrited]
        private Text frustum;
        private bool deviceInited = false;
        private Pose mCameraPose;
        private float fps = 60;

        void Start()
        {
            RKSensorAPI.Instance.Initialize(true, true);
            RKSensorAPI.OnUsbDeviceInited += OnUsbDeviceInited;
            RKSensorAPI.OnIMUDataCallBack += OnIMUDataCallBack;
            RKSensorAPI.OnIMUUpdate += OnIMUUpdate;
        }

        private void OnIMUUpdate(float delta)
        {
            string text = "IMUFPS: ";
            float interp = delta / (0.5f + delta);
            float currentFPS = 1.0f / delta;
            fps = Mathf.Lerp(fps, currentFPS, interp);
            text += Mathf.RoundToInt(fps);
            imuFPS.text = text;
        }

        private void OnDestroy()
        {
            RKSensorAPI.OnUsbDeviceInited -= OnUsbDeviceInited;
            RKSensorAPI.OnIMUDataCallBack -= OnIMUDataCallBack;
            RKSensorAPI.OnIMUUpdate -= OnIMUUpdate;
        }

        private void OnIMUDataCallBack(RKSensorAPI.IMUData imuData)
        {
            // this.imuData.text = $"IMUData:{JsonUtility.ToJson(imuData)}";
        }

        private void OnUsbDeviceInited()
        {
            deviceInited = true;
            cameraIntrinsices.text = $"CameraIntrinsices: {NativeInterface.NativeAPI.GetOSTInfo()}";
            float[] frustum_left = new float[4];
            float[] frustum_right = new float[4];
            NativeInterface.NativeAPI.GetUnityFrustum(ref frustum_left, ref frustum_right);
            frustum.text = $"frustum_left: {JsonConvert.SerializeObject(frustum_left)} \r\n frustum_right: {JsonConvert.SerializeObject(frustum_right)} ";
        }

        void Update()
        {
            if (deviceInited)
            {
                RKSensorAPI.YUVImage imageData = RKSensorAPI.Instance.GetYUVImage();
                if (imageData.success)
                {
                    yuvImage.text = $"YUVImage timeStamp:{RKSensorAPI.Instance.GetYUVImage().timeStamp}";
                }
                mCameraPose = NativeInterface.NativeAPI.GetCameraPhysicsPose(out long timeStamp);
                cameraPose.text = $"CameraPose eulerAngles:{mCameraPose.rotation.eulerAngles},pos:{mCameraPose.position}";
                this.imuData.text = $"IMUData:{JsonUtility.ToJson(RKSensorAPI.Instance.GetIMUData())}";
            }
            trackingStatus.text = $"HeadTrackingStatus:{NativeInterface.NativeAPI.GetHeadTrackingStatus()}";
        }
    }
}


