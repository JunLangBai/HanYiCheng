using Rokid.UXR;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.UI;

public class UIOverlaySample : AutoInjectBehaviour
{
    private bool adjustCenterByFov = true;

    private RKCameraRig cameraRig;

    [Autowrited] private Toggle centerToggle;

    private FollowCamera followCamera;

    [Autowrited] private Button sixDofButton;

    [Autowrited] private Button threeDofButton;

    private bool useLeftEyeFov = true;

    [Autowrited] private Toggle useLeftEyeFovToggle;

    [Autowrited] private Button zeroDofButton;


    private void Start()
    {
        cameraRig = MainCameraCache.mainCamera.transform.GetComponent<RKCameraRig>();
        followCamera = GameObject.Find("OverlayUI").GetComponent<FollowCamera>();
        zeroDofButton.onClick.AddListener(() => { cameraRig.headTrackingType = RKCameraRig.HeadTrackingType.ZeroDof; });

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