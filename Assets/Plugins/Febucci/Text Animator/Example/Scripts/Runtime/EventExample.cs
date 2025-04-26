using UnityEngine;
using UnityEngine.Assertions;

namespace Febucci.UI.Examples
{
    //Prevents this example to show up in the inspector
    [AddComponentMenu("")]
    public class EventExample : MonoBehaviour
    {
        public TextAnimatorPlayer textAnimatorPlayer;


        public Camera cam;
        public Color[] bgColors;


        private int lastBGIndex;

        private void Awake()
        {
            Assert.IsNotNull(textAnimatorPlayer, $"Text Animator Player component is null in {gameObject.name}");
            textAnimatorPlayer.textAnimator.onEvent += OnEvent;
        }

        private void OnEvent(string message)
        {
            switch (message)
            {
                case "bg":
                    cam.backgroundColor = bgColors[lastBGIndex];
                    lastBGIndex++;
                    if (lastBGIndex >= bgColors.Length) lastBGIndex = 0;
                    break;
            }
        }
    }
}