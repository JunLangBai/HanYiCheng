using System;
using Febucci.Attributes;
using UnityEngine;

namespace Febucci.UI.Core
{
    [Serializable]
    internal struct EmissionControl
    {
#pragma warning disable 0649 //disables warning 0649, "value declared but never assigned", since Unity actually assigns the variable in the inspector through the [SerializeField] attribute, but the compiler doesn't know this and throws warnings
        [SerializeField] [MinValue(0)] private int cycles;

        [SerializeField] private AnimationCurve attackCurve;
        [SerializeField] [MinValue(0)] private float overrideDuration;
        [SerializeField] private bool continueForever;
        [SerializeField] private AnimationCurve decayCurve;
#pragma warning restore 0649

        [NonSerialized] private float maxDuration;
        [NonSerialized] private AnimationCurve intensityOverDuration;
        [NonSerialized] private float passedTime;
        [NonSerialized] private float cycleDuration;

        [NonSerialized] public float effectWeigth;


        public void Initialize(float effectsMaxDuration)
        {
            effectWeigth = 0;
            passedTime = 0;

            var totalKeys = new Keyframe[
                attackCurve.length + (continueForever ? 0 : decayCurve.length)
            ];

            for (var i = 0; i < attackCurve.length; i++) totalKeys[i] = attackCurve[i];

            if (!continueForever)
            {
                if (overrideDuration > 0) effectsMaxDuration = overrideDuration;

                var attackDuration = attackCurve.CalculateCurveDuration();

                for (var i = attackCurve.length; i < totalKeys.Length; i++)
                {
                    totalKeys[i] = decayCurve[i - attackCurve.length];
                    totalKeys[i].time += effectsMaxDuration + attackDuration;
                }
            }

            intensityOverDuration = new AnimationCurve(totalKeys);
            intensityOverDuration.preWrapMode = WrapMode.Loop;
            intensityOverDuration.postWrapMode = WrapMode.Loop;

            cycleDuration = intensityOverDuration.CalculateCurveDuration();
            maxDuration = cycleDuration * cycles;
        }

        public float IncreaseEffectTime(float deltaTime)
        {
            if (deltaTime == 0)
                return passedTime;

            passedTime += deltaTime;

            if (passedTime < 0)
                passedTime = 0;

            //inside duration
            if (passedTime > cycleDuration) //increases cycle
                if (continueForever)
                {
                    effectWeigth = 1;
                    return passedTime;
                }

            //outside cycles
            if (cycles > 0 && passedTime >= maxDuration)
            {
                effectWeigth = 0;
                return 0;
            }

            effectWeigth = intensityOverDuration.Evaluate(passedTime);

            return passedTime;
        }
    }
}