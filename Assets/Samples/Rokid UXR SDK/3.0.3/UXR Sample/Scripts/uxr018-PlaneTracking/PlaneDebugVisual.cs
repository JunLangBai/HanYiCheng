using Rokid.UXR.Module;
using TMPro;
using UnityEngine;

namespace Rokid.UXR.Config
{
    public class PlaneDebugVisual : MonoBehaviour
    {
        [SerializeField] private ARPlane plane;

        [SerializeField] private TextMeshPro text;


        private void Update()
        {
            text.text = plane.boundedPlane.ToString();
        }
    }
}