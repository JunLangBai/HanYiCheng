using UnityEngine;

namespace Rokid.UXR.Demo
{
    public class ChangColor : MonoBehaviour
    {
        private int colorIndex;
        private readonly Color[] colors = { Color.white, Color.yellow, Color.green, Color.red };

        private void Start()
        {
        }

        public void Change()
        {
            colorIndex++;
            if (colorIndex == colors.Length)
                colorIndex = 0;
            GetComponentInChildren<MeshRenderer>().material.color = colors[colorIndex];
        }
    }
}