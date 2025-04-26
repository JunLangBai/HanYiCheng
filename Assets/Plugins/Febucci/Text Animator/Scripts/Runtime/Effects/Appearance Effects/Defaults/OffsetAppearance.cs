using UnityEngine;
using UnityEngine.Scripting;

namespace Febucci.UI.Core
{
    [Preserve]
    [EffectInfo(TAnimTags.ap_Offset)]
    internal class OffsetAppearance : AppearanceBase
    {
        private float amount;
        private Vector2 direction;

        public override void SetDefaultValues(AppearanceDefaultValues data)
        {
            direction = data.defaults.offsetDir;
            amount = data.defaults.offsetAmplitude * uniformIntensity;
            effectDuration = data.defaults.offsetDuration;
        }

        public override void ApplyEffect(ref CharacterData data, int charIndex)
        {
            //Moves all towards a direction
            data.vertices.MoveChar(direction * amount * Tween.EaseIn(1 - data.passedTime / effectDuration));
        }

        public override void SetModifier(string modifierName, string modifierValue)
        {
            base.SetModifier(modifierName, modifierValue);
            switch (modifierName)
            {
                case "a": ApplyModifierTo(ref amount, modifierValue); break;
            }
        }
    }
}