using Rokid.UXR;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.UI;

public class WindowFollowSample : AutoInjectBehaviour
{
    [Autowrited]
    private Toggle horizontalFollow;
    [Autowrited]
    private Toggle yawLock;
    [Autowrited]
    private Toggle pitchLock;
    [Autowrited]
    private Toggle rollLock;
    private void Start()
    {
        RKCameraRig cameraRig = MainCameraCache.mainCamera.GetComponent<RKCameraRig>();
        WindowsFollow[] windowsFollow = transform.GetComponentsInChildren<WindowsFollow>();

        horizontalFollow.onValueChanged.AddListener(isOn =>
        {
            for (int i = 0; i < windowsFollow.Length; i++)
            {
                windowsFollow[i].FixedWindowRoll = isOn;
            }
        });

        yawLock.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                cameraRig.rotationLock |= RKCameraRig.RotationLock.Yaw;
            }
            else
            {
                cameraRig.rotationLock ^= RKCameraRig.RotationLock.Yaw;
            }
        });

        pitchLock.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                cameraRig.rotationLock |= RKCameraRig.RotationLock.Pitch;
            }
            else
            {
                cameraRig.rotationLock ^= RKCameraRig.RotationLock.Pitch;
            }
        });

        rollLock.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                cameraRig.rotationLock |= RKCameraRig.RotationLock.Roll;
            }
            else
            {
                cameraRig.rotationLock ^= RKCameraRig.RotationLock.Roll;
            }
        });
    }
}
