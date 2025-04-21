using UnityEngine;
using UnityEngine.UI;
using Rokid.UXR.Module;

namespace Rokid.UXR.Demo
{
    public class WebViewDemo : MonoBehaviour
    {
        public Button openRokid;

        private void Start()
        {
            openRokid.onClick.AddListener(() =>
            {
                RKVirtualController.Instance.AutoLoadWebView("https://www.rokid.com");
            });
        }
    }

}
