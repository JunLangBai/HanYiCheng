
using Rokid.UXR.Native;
using UnityEngine.UI;

namespace Rokid.UXR.Demo
{
    public class IPDSample : AutoInjectBehaviour
    {
        [Autowrited]
        private Slider slider;
        [Autowrited]
        private Text ipdText;
        void Start()
        {
            ipdText.text = "IPD:" + NativeInterface.NativeAPI.GetIPD();
            slider.value = (NativeInterface.NativeAPI.GetIPD() - 53) / 22.0f;
            slider.onValueChanged.AddListener(value =>
            {
                int val = (int)(53 + value * 22);
                NativeInterface.NativeAPI.SetIPD(val);
                ipdText.text = "IPD:" + val;
            });
        }
    }
}