using UnityEngine;
using UnityEngine.UI;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;

namespace Rokid.UXR.Demo
{
    public class CameraPreview : MonoBehaviour
    {
        private bool isInit;
        private Texture2D previewTex;
        public RawImage rawImage;
        private int width, height;


        public void Init()
        {
            width = NativeInterface.NativeAPI.GetPreviewWidth();
            height = NativeInterface.NativeAPI.GetPreviewHeight();
            if (NativeInterface.NativeAPI.GetGlassName().Equals("Rokid Max Plus"))
            {
                NativeInterface.NativeAPI.SetCameraPreviewDataType(3);
                previewTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            }
            else
            {
                NativeInterface.NativeAPI.SetCameraPreviewDataType(1);
                previewTex = new Texture2D(width, height, TextureFormat.BGRA32, false);
            }
            NativeInterface.NativeAPI.OnCameraDataUpdate += OnCameraDataUpdate;
            isInit = true;
        }

        private void OnCameraDataUpdate(int width, int height, byte[] data, long timestamp)
        {
            Loom.QueueOnMainThread(() =>
            {
                previewTex.LoadRawTextureData(data);
                previewTex.Apply();
                rawImage.texture = previewTex;
            });
        }

        private void Awake()
        {
#if !UNITY_EDITOR
	        rawImage.color = Color.white;
#endif
            NativeInterface.NativeAPI.StartCameraPreview();
        }

        public void Release()
        {
            if (isInit)
            {
                NativeInterface.NativeAPI.OnCameraDataUpdate -= OnCameraDataUpdate;
                NativeInterface.NativeAPI.StopCameraPreview();
                NativeInterface.NativeAPI.ClearCameraDataUpdate();
                isInit = false;
            }
        }

        private void OnDestroy()
        {
            Release();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Release();
            }
            else
            {
                NativeInterface.NativeAPI.StartCameraPreview();
            }
        }

        private void Update()
        {
            if (isInit == false && NativeInterface.NativeAPI.IsPreviewing())
            {
                Init();
            }
        }
    }
}
