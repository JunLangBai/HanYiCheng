using UnityEngine;
using UnityEngine.UI;
using Rokid.UXR.Module;
using Rokid.UXR.Interaction;

namespace Rokid.UXR.Demo
{
    public class TouchPadSample : AutoInjectBehaviour
    {
        [Autowrited]
        private Text touchLogText;
        private void Update()
        {

            if (touchLogText != null)
            {
                if (RKTouchInput.Instance.GetInsideTouchCount() > 0)
                {
                    touchLogText.text = $"screen.orientation:{Screen.orientation}\ntouch.count:{RKTouchInput.Instance.GetInsideTouchCount()}\ntouch.Position:{RKTouchInput.Instance.GetInsideTouch(0).position}\ntouch.delta:{RKTouchInput.Instance.GetInsideTouch(0).deltaPosition}\nscreen.width:{UnityPlayerAPI.Instance.PhoneScreenWidth}\nscreen.height:{UnityPlayerAPI.Instance.PhoneScreenHeight}\ntouchMove:{TouchPadEventInput.Instance.TouchMove()}\ntouchLongPress:{TouchPadEventInput.Instance.LongPress()}\ntouchPointerUp:{TouchPadEventInput.Instance.PointerUp()}\ntouchPhase:{RKTouchInput.Instance.GetInsideTouch(0).phase}";
                }
                else
                {
                    touchLogText.text = $"screen.orientation:{Screen.orientation}\ntouch.count:{RKTouchInput.Instance.GetInsideTouchCount()}\nscreen.width:{UnityPlayerAPI.Instance.PhoneScreenWidth}\nscreen.height:{UnityPlayerAPI.Instance.PhoneScreenHeight}\ntouchMove:{TouchPadEventInput.Instance.TouchMove()}\ntouchLongPress:{TouchPadEventInput.Instance.LongPress()}\ntouchPointerUp:{TouchPadEventInput.Instance.PointerUp()}";
                }
            }
        }
    }
}

