using UnityEngine;

namespace LeTai.Asset.TranslucentImage.Demo
{
    public class UnrestrictFramerate : MonoBehaviour
    {
        private void Start()
        {
            if (Application.isMobilePlatform)
                Application.targetFrameRate = 120;
        }
    }
}