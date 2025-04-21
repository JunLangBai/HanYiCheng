using Rokid.UXR.Module;
using UnityEngine;
using TMPro;
namespace Rokid.UXR.Config
{
    public class PlaneDebugVisual : MonoBehaviour
    {
        [SerializeField]
        private ARPlane plane;
        [SerializeField]
        private TextMeshPro text;


        void Update()
        {
            text.text = plane.boundedPlane.ToString();
        }
    }
}

