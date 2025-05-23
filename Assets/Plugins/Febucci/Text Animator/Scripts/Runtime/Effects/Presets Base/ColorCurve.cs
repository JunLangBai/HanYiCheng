﻿using System;
using Febucci.Attributes;
using UnityEngine;

namespace Febucci.UI.Core
{
    [Serializable]
    internal class ColorCurve
    {
        private bool isAppearance;

        public float GetDuration()
        {
            return duration;
        }

        public void Initialize(bool isAppearance)
        {
            this.isAppearance = isAppearance;
            if (duration < .1f) duration = .1f;
        }

        public Color32 GetColor(float time, int characterIndex)
        {
            if (isAppearance)
                return gradient.Evaluate(Mathf.Clamp01(time / duration));

            return gradient.Evaluate((time / duration % 1f + characterIndex * (charsTimeOffset / 100f)) % 1f);
        }

#pragma warning disable 0649 //disabling the error or unity will throw "field is never assigned" [..], because we actually assign the variables from the custom drawers
        [SerializeField] public bool enabled;

        [SerializeField] protected Gradient gradient;
        [SerializeField] [MinValue(0.1f)] protected float duration;
        [SerializeField] [Range(0, 100)]
        protected float charsTimeOffset; //clamping to 100 because it repeates the behavior after it
#pragma warning restore 0649
    }
}