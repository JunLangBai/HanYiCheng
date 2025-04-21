
using Rokid.UXR;
using UnityEngine.UI;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;
using UnityEngine;
public class UIOverlaySample : AutoInjectBehaviour
{
    [Autowrited]
    private Button zeroDofButton;
    [Autowrited]
    private Button threeDofButton;
    [Autowrited]
    private Button sixDofButton;
    [Autowrited]
    private Toggle centerToggle;
    [Autowrited]
    private Toggle useLeftEyeFovToggle;

    private RKCameraRig cameraRig;

    private FollowCamera followCamera;

    private bool useLeftEyeFov = true;
    private bool adjustCenterByFov = true;


    private void Start()
    {
        cameraRig = MainCameraCache.mainCamera.transform.GetComponent<RKCameraRig>();
        followCamera = GameObject.Find("OverlayUI").GetComponent<FollowCamera>();
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

        centerToggle.onValueChanged.AddListener(selected =>
        {
            adjustCenterByFov = selected;
            followCamera.AdjustCenterByCameraFov(adjustCenterByFov, useLeftEyeFov);
        });

        useLeftEyeFovToggle.onValueChanged.AddListener(selected =>
        {
            useLeftEyeFov = selected;
            followCamera.AdjustCenterByCameraFov(adjustCenterByFov, useLeftEyeFov);
        });
    }
}
