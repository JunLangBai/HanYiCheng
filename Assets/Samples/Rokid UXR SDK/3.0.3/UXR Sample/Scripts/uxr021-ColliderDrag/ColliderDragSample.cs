using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.UI;
namespace Rokid.UXR.Demo
{
    public class ColliderDragSample : AutoInjectBehaviour
    {
        [Autowrited]
        private Text logText;

        void Update()
        {
            logText.text = $"Distance To Camera:{Vector3.Distance(MainCameraCache.mainCamera.transform.position, transform.position)}";
        }
    }
}
