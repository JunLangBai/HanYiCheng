using UnityEngine;
using UnityEngine.Scripting;

namespace Febucci.UI.Core
{
    [Preserve]
    [EffectInfo(TAnimTags.bh_Shake)]
    internal class ShakeBehavior : BehaviorBase
    {
        private int lastRandomIndex;

        private int randIndex;
        public float shakeDelay;
        public float shakeStrength;

        private float timePassed;

        public override void SetDefaultValues(BehaviorDefaultValues data)
        {
            shakeDelay = data.defaults.shakeDelay;
            shakeStrength = data.defaults.shakeStrength;
            ClampValues();
        }

        private void ClampValues()
        {
            shakeDelay = Mathf.Clamp(shakeDelay, 0.002f, 500);
        }

        public override void Initialize(int charactersCount)
        {
            base.Initialize(charactersCount);

            randIndex = Random.Range(0, TextUtilities.fakeRandomsCount);
            lastRandomIndex = randIndex;
        }


        public override void SetModifier(string modifierName, string modifierValue)
        {
            switch (modifierName)
            {
                //amplitude
                case "a": ApplyModifierTo(ref shakeStrength, modifierValue); break;
                //delay
                case "d": ApplyModifierTo(ref shakeDelay, modifierValue); break;
            }

            ClampValues();
        }

        public override void Calculate()
        {
            timePassed += time.deltaTime;
            //Changes the shake direction if enough time passed
            if (timePassed >= shakeDelay)
            {
                timePassed = 0;

                randIndex = Random.Range(0, TextUtilities.fakeRandomsCount);

                //Avoids repeating the same index twice 
                if (lastRandomIndex == randIndex)
                {
                    randIndex++;
                    if (randIndex >= TextUtilities.fakeRandomsCount)
                        randIndex = 0;
                }

                lastRandomIndex = randIndex;
            }
        }

        public override void ApplyEffect(ref CharacterData data, int charIndex)
        {
            data.vertices.MoveChar
            (
                TextUtilities.fakeRandoms[
                    Mathf.RoundToInt((charIndex + randIndex) % (TextUtilities.fakeRandomsCount - 1))
                ] * shakeStrength * uniformIntensity
            );
        }


        public override string ToString()
        {
            return $"shake delay: {shakeDelay}\n" +
                   $"strength: {shakeStrength}" +
                   $"\n{base.ToString()}";
        }
    }
}