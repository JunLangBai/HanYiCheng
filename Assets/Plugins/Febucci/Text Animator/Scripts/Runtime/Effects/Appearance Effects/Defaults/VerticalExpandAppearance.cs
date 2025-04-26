using UnityEngine;
using UnityEngine.Scripting;

namespace Febucci.UI.Core
{
    [Preserve]
    [EffectInfo(TAnimTags.ap_VertExp)]
    internal class VerticalExpandAppearance : AppearanceBase
    {
        //--Temp variables--
        private float pct;

        private int startA, targetA;
        private int startB, targetB;

        public override void SetDefaultValues(AppearanceDefaultValues data)
        {
            effectDuration = data.defaults.verticalExpandDuration;
            SetOrientation(data.defaults.verticalFromBottom);
        }

        private void SetOrientation(bool fromBottom)
        {
            if (fromBottom) //From bottom to top 
            {
                //top left copies bottom left
                startA = 0;
                targetA = 1;

                //top right copies bottom right
                startB = 3;
                targetB = 2;
            }
            else //from top to bottom
            {
                //bottom left copies top left
                startA = 1;
                targetA = 0;

                //bottom right copies top right
                startB = 2;
                targetB = 3;
            }
        }

        public override void ApplyEffect(ref CharacterData data, int charIndex)
        {
            pct = Tween.EaseInOut(data.passedTime / effectDuration);

            data.vertices[targetA] = Vector3.LerpUnclamped(data.vertices[startA], data.vertices[targetA], pct);
            data.vertices[targetB] = Vector3.LerpUnclamped(data.vertices[startB], data.vertices[targetB], pct);
        }

        public override void SetModifier(string modifierName, string modifierValue)
        {
            base.SetModifier(modifierName, modifierValue);
            switch (modifierName)
            {
                case "bot": SetOrientation(modifierValue == "1"); break;
            }
        }
    }
}