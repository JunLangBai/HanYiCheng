using Rokid.UXR;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;
using UnityEngine.UI;

public class WindowFollowSample : AutoInjectBehaviour
{
    [Autowrited] private Toggle horizontalFollow;

    [Autowrited] private Toggle pitchLock;

    [Autowrited] private Toggle rollLock;

    [Autowrited] private Toggle yawLock;

    private void Start()
    {
        var cameraRig = MainCameraCache.mainCamera.GetComponent<RKCameraRig>();
        var windowsFollow = transform.GetComponentsInChildren<WindowsFollow>();

        horizontalFollow.onValueChanged.AddListener(isOn =>
        {
            for (var i = 0; i < windowsFollow.Length; i++) windowsFollow[i].FixedWindowRoll = isOn;
        });

        yawLock.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
                cameraRig.rotationLock |= RKCameraRig.RotationLock.Yaw;
            else
                cameraRig.rotationLock ^= RKCameraRig.RotationLock.Yaw;
        });

        pitchLock.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
                cameraRig.rotationLock |= RKCameraRig.RotationLock.Pitch;
            else
                cameraRig.rotationLock ^= RKCameraRig.RotationLock.Pitch;
        });

        rollLock.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
                cameraRig.rotationLock |= RKCameraRig.RotationLock.Roll;
            else
                cameraRig.rotationLock ^= RKCameraRig.RotationLock.Roll;
        });
    }
}