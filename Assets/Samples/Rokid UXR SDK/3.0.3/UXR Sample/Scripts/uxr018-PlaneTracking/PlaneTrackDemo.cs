using Rokid.UXR.Module;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Rokid.UXR.Demo
{
    public class PlaneTrackDemo : AutoInjectBehaviour
    {
        [Autowrited, SerializeField]
        private Button openPlaneTracker;
        [Autowrited, SerializeField]
        private Button closePlaneTracker;
        [Autowrited, SerializeField]
        private Text statusText;
        [Autowrited, SerializeField]
        private Toggle horizontal;
        [Autowrited, SerializeField]
        private Toggle vertical;
        [Autowrited, SerializeField]
        private Toggle horizontalAndVertical;
        void Start()
        {
            openPlaneTracker.onClick.AddListener(() =>
            {
                ARPlaneManager.Instance.OpenPlaneTracker();
                statusText.text = "PlaneTracking";
            });

            closePlaneTracker.onClick.AddListener(() =>
            {
                ARPlaneManager.Instance.ClosePlaneTracker();
                statusText.text = "Closed";
            });

            horizontal.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    ARPlaneManager.Instance.SetPlaneDetectMode(PlaneDetectMode.Horizontal);
                }
            });

            vertical.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    ARPlaneManager.Instance.SetPlaneDetectMode(PlaneDetectMode.Vertical);
                }
            });

            horizontalAndVertical.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    ARPlaneManager.Instance.SetPlaneDetectMode(PlaneDetectMode.HorizontalAndVertical);
                }
            });

            switch (ARPlaneManager.Instance.GetPlaneDetectMode())
            {
                case PlaneDetectMode.Horizontal:
                    horizontal.isOn = true;
                    break;
                case PlaneDetectMode.Vertical:
                    vertical.isOn = true;
                    break;
                case PlaneDetectMode.HorizontalAndVertical:
                    horizontalAndVertical.isOn = true;
                    break;
            }
        }
    }
}

