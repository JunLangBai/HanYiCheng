using UnityEngine.Scripting;

namespace Febucci.UI.Core
{
    [Preserve]
    [EffectInfo(TAnimTags.bh_Rot)]
    internal class RotationBehavior : BehaviorBase
    {
        private float angleDiffBetweenChars = 10;

        private float angleSpeed = 180;

        public override void SetDefaultValues(BehaviorDefaultValues data)
        {
            angleSpeed = data.defaults.angleSpeed;
            angleDiffBetweenChars = data.defaults.angleDiffBetweenChars;
        }

        public override void SetModifier(string modifierName, string modifierValue)
        {
            switch (modifierName)
            {
                //frequency
                case "f": ApplyModifierTo(ref angleSpeed, modifierValue); break;
                //angle diff
                case "s": ApplyModifierTo(ref angleDiffBetweenChars, modifierValue); break;
            }
        }

        public override void ApplyEffect(ref CharacterData data, int charIndex)
        {
            data.vertices.RotateChar(-time.timeSinceStart * angleSpeed + angleDiffBetweenChars * charIndex);
        }


        public override string ToString()
        {
            return $"angleSpeed: {angleSpeed}\n" +
                   $"angleDiffBetweenChars: {angleDiffBetweenChars}" +
                   $"\n{base.ToString()}";
        }
    }
}