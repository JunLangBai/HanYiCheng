using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting;

namespace TensorFlowLite
{
    /// <summary>
    ///     An wrapper for WebCamTexture that corrects texture rotation
    /// </summary>
    [Obsolete("Use TextureSource.VirtualTextureSource instead")]
    public sealed class WebCamInput : MonoBehaviour
    {
        private static List<int> deviceIndexesOpened;

        [SerializeField] [WebCamName] private string editorCameraName;
        [SerializeField] private WebCamKind preferKind = WebCamKind.WideAngle;
        [SerializeField] private bool isFrontFacing;
        [SerializeField] private Vector2Int requestSize = new(1280, 720);
        [SerializeField] private int requestFps = 60;
        public TextureUpdateEvent OnTextureUpdate = new();
        private int deviceIndex;
        private WebCamDevice[] devices;

        private TextureResizer resizer;
        private WebCamTexture webCamTexture;

        public Vector2Int RequestSize
        {
            get => requestSize;
            set => requestSize = value;
        }

        public int RequestFps
        {
            get => requestFps;
            set => requestFps = value;
        }

        public string RequestCameraByDeviceName
        {
            get => editorCameraName;
            set => editorCameraName = value;
        }

        public string PreferKind
        {
            get => preferKind.ToString();
            set => Enum.TryParse(value, out preferKind);
        }

        public bool IsFrontFacing
        {
            get => isFrontFacing;
            set => isFrontFacing = value;
        }

        private void Update()
        {
            if (!webCamTexture.didUpdateThisFrame) return;

            var tex = NormalizeWebcam(webCamTexture, requestSize.x, requestSize.y, isFrontFacing);
            OnTextureUpdate.Invoke(tex);
        }

        private void OnEnable()
        {
            resizer = new TextureResizer();
            devices = WebCamTexture.devices;
            var cameraName = Application.isEditor || !string.IsNullOrEmpty(editorCameraName)
                ? editorCameraName
                : WebCamUtil.FindName(preferKind, isFrontFacing);

            WebCamDevice device = default;
            for (var i = 0; i < devices.Length; i++)
                if (devices[i].name == cameraName)
                {
                    device = devices[i];
                    deviceIndex = i;
                    break;
                }

            if (deviceIndexesOpened == null) deviceIndexesOpened = new List<int>();
            // trying to open a busy camera
            if (deviceIndexesOpened.Contains(deviceIndex))
            {
                // select next available camera
                var ordered = deviceIndexesOpened.OrderByDescending(x => x);
                deviceIndex = ordered.First();
                deviceIndex++;
                if (deviceIndex >= devices.Length)
                {
                    deviceIndex = 0;
                    while (deviceIndexesOpened.Contains(deviceIndex) && deviceIndex < devices.Length - 1) deviceIndex++;
                }

                device = devices[deviceIndex];
            }

            deviceIndexesOpened.Add(deviceIndex);
            editorCameraName = devices[deviceIndex].name;
            StartCamera(device);
        }

        private void OnDisable()
        {
            deviceIndexesOpened.Remove(deviceIndex);
            PauseCamera();
        }

        private void OnDestroy()
        {
            deviceIndexesOpened.Remove(deviceIndex);
            StopCamera();
            resizer?.Dispose();
        }

        // Invoked by Unity Event
        [Preserve]
        public void ToggleCamera()
        {
            deviceIndex = (deviceIndex + 1) % devices.Length;
            StartCamera(devices[deviceIndex]);
        }

        private void StartCamera(WebCamDevice device)
        {
            StopCamera();
            isFrontFacing = device.isFrontFacing;
            webCamTexture = new WebCamTexture(device.name, requestSize.x, requestSize.y, requestFps);
            try
            {
                webCamTexture.Play();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                webCamTexture = null;
            }
        }

        private void PauseCamera()
        {
            if (webCamTexture == null) return;
            webCamTexture.Stop();
        }

        private void StopCamera()
        {
            if (webCamTexture == null) return;
            webCamTexture.Stop();
            Destroy(webCamTexture);
        }

        private RenderTexture NormalizeWebcam(WebCamTexture texture, int width, int height, bool isFrontFacing)
        {
            var cameraWidth = texture.width;
            var cameraHeight = texture.height;
            var isPortrait = IsPortrait(texture);
            if (isPortrait) (cameraWidth, cameraHeight) = (cameraHeight, cameraWidth); // swap

            var cameraAspect = (float)cameraWidth / cameraHeight;
            var targetAspect = (float)width / height;

            int w, h;
            if (cameraAspect > targetAspect)
            {
                w = RoundToEven(cameraHeight * targetAspect);
                h = cameraHeight;
            }
            else
            {
                w = cameraWidth;
                h = RoundToEven(cameraWidth / targetAspect);
            }

            Matrix4x4 mtx;
            Vector4 uvRect;
            var rotation = texture.videoRotationAngle;

            // Seems to be bug in the android. might be fixed in the future.
            if (Application.platform == RuntimePlatform.Android) rotation = -rotation;

            if (isPortrait)
            {
                mtx = TextureResizer.GetVertTransform(rotation, texture.videoVerticallyMirrored, isFrontFacing);
                uvRect = TextureResizer.GetTextureST(targetAspect, cameraAspect, AspectMode.Fill);
            }
            else
            {
                mtx = TextureResizer.GetVertTransform(rotation, isFrontFacing, texture.videoVerticallyMirrored);
                uvRect = TextureResizer.GetTextureST(cameraAspect, targetAspect, AspectMode.Fill);
            }

            // Debug.Log($"camera: rotation:{texture.videoRotationAngle} flip:{texture.videoVerticallyMirrored}");
            return resizer.Resize(texture, w, h, false, mtx, uvRect);
        }

        private static bool IsPortrait(WebCamTexture texture)
        {
            return texture.videoRotationAngle == 90 || texture.videoRotationAngle == 270;
        }

        private static int RoundToEven(float n)
        {
            return Mathf.RoundToInt(n / 2) * 2;
        }

        [Serializable]
        public class TextureUpdateEvent : UnityEvent<Texture>
        {
        }
    }
}