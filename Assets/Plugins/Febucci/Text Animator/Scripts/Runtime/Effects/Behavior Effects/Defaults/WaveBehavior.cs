using UnityEngine;
using UnityEngine.Scripting;

namespace Febucci.UI.Core
{
    [Preserve]
    [EffectInfo(TAnimTags.bh_Wave)]
    internal class WaveBehavior : BehaviorSine
    {
        public override void SetDefaultValues(BehaviorDefaultValues data)
        {
            amplitude = data.defaults.waveAmplitude;
            frequency = data.defaults.waveFrequency;
            waveSize = data.defaults.waveWaveSize;
        }

        public override void ApplyEffect(ref CharacterData data, int charIndex)
        {
            data.vertices.MoveChar(Vector3.up * Mathf.Sin(time.timeSinceStart * frequency + charIndex * waveSize) *
                                   amplitude * uniformIntensity);
        }
    }
}