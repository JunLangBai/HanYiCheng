﻿using UnityEngine.Scripting;

namespace Febucci.UI.Core
{
    [Preserve]
    [EffectInfo(TAnimTags.ap_Size)]
    internal class SizeAppearance : AppearanceBase
    {
        private float amplitude;

        public override void SetDefaultValues(AppearanceDefaultValues data)
        {
            effectDuration = data.defaults.sizeDuration;
            amplitude = data.defaults.sizeAmplitude * -1 + 1;
        }

        public override void ApplyEffect(ref CharacterData data, int charIndex)
        {
            data.vertices.LerpUnclamped(
                data.vertices.GetMiddlePos(),
                Tween.EaseIn(1 - data.passedTime / effectDuration) * amplitude
            );
        }

        public override void SetModifier(string modifierName, string modifierValue)
        {
            base.SetModifier(modifierName, modifierValue);
            switch (modifierName)
            {
                case "a": ApplyModifierTo(ref amplitude, modifierValue); break;
            }
        }
    }
}