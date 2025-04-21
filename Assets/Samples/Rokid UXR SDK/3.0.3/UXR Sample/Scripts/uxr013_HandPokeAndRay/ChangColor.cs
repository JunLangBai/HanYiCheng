using UnityEngine;

namespace Rokid.UXR.Demo
{
    public class ChangColor : MonoBehaviour
    {
        Color[] colors = new Color[] { Color.white, Color.yellow, Color.green, Color.red };
        int colorIndex = 0;
        void Start()
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
