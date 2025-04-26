using UnityEngine;
using UnityEngine.Scripting;

namespace Febucci.UI.Core
{
    [Preserve]
    [EffectInfo(TAnimTags.bh_Rainb)]
    internal class RainbowBehavior : BehaviorBase
    {
        private float hueShiftSpeed = 0.8f;
        private float hueShiftWaveSize = 0.08f;

        public override void SetDefaultValues(BehaviorDefaultValues data)
        {
            hueShiftSpeed = data.defaults.hueShiftSpeed;
            hueShiftWaveSize = data.defaults.hueShiftWaveSize;
        }

        public override void SetModifier(string modifierName, string modifierValue)
        {
            switch (modifierName)
            {
                //frequency
                case "f": ApplyModifierTo(ref hueShiftSpeed, modifierValue); break;
                //wave size
                case "s": ApplyModifierTo(ref hueShiftWaveSize, modifierValue); break;
            }
        }

        public override void ApplyEffect(ref CharacterData data, int charIndex)
        {
            for (byte i = 0; i < data.colors.Length; i++)
                data.colors[i] =
                    Color.HSVToRGB(
                        Mathf.PingPong(time.timeSinceStart * hueShiftSpeed + charIndex * hueShiftWaveSize, 1), 1, 1);
        }


        public override string ToString()
        {
            return $"hueShiftSpeed: {hueShiftSpeed}\n" +
                   $"hueShiftWaveSize: {hueShiftWaveSize}" +
                   $"\n{base.ToString()}";
        }
    }
}