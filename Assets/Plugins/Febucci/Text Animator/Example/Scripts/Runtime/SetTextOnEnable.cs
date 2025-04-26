using UnityEngine;
using UnityEngine.Assertions;

namespace Febucci.UI.Examples
{
    /// <summary>
    ///     Starts the textAnimator once this object is active
    ///     Disables it once it's deactivated
    /// </summary>
    [AddComponentMenu("Febucci/TextAnimator/Utilities/SetTextOnEnable")]
    public class SetTextOnEnable : MonoBehaviour
    {
        public TextAnimatorPlayer tanimPlayer;

        private string textToSet;

        private void Awake()
        {
            Assert.IsNotNull(
                "TextAnimator: The object 'SetTextOnEnable' has no TextAnimatorPlayer component reference.");

            textToSet = tanimPlayer.textAnimator.tmproText.text;
            tanimPlayer.ShowText("");
        }

        private void OnEnable()
        {
            tanimPlayer.ShowText(textToSet);
        }

        private void OnDisable()
        {
            if (tanimPlayer != null)
            {
                //Forces the text to hide
                tanimPlayer.StopShowingText();
                tanimPlayer.ShowText("");
            }
        }
    }
}