using Rokid.UXR.Native;
using UnityEngine.UI;

namespace Rokid.UXR.Demo
{
    public class IPDSample : AutoInjectBehaviour
    {
        [Autowrited] private Text ipdText;

        [Autowrited] private Slider slider;

        private void Start()
        {
            ipdText.text = "IPD:" + NativeInterface.NativeAPI.GetIPD();
            slider.value = (NativeInterface.NativeAPI.GetIPD() - 53) / 22.0f;
            slider.onValueChanged.AddListener(value =>
            {
                var val = (int)(53 + value * 22);
                NativeInterface.NativeAPI.SetIPD(val);
                ipdText.text = "IPD:" + val;
            });
        }
    }
}