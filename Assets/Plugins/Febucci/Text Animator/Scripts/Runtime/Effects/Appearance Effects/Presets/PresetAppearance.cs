using UnityEngine;
using UnityEngine.Scripting;

namespace Febucci.UI.Core
{
    [Preserve]
    [EffectInfo("")]
    internal class PresetAppearance : AppearanceBase
    {
        private Color32 color;
        private ColorCurve colorCurve;

        private bool enabled;

        private bool hasTransformEffects;

        //management
        private Matrix4x4 matrix;

        private ThreeAxisEffector movement;
        private Vector3 offset;
        private ThreeAxisEffector rotation;
        private Quaternion rotationQua;
        private TwoAxisEffector scale;

        private bool setColor;

        public override void SetDefaultValues(AppearanceDefaultValues data)
        {
            effectDuration = 0;

            enabled = false;

            void AssignValues(PresetAppearanceValues result)
            {
                enabled = SetPreset(
                    true,
                    result,
                    ref movement,
                    ref effectDuration,
                    ref rotation,
                    ref scale,
                    ref rotationQua,
                    ref hasTransformEffects,
                    ref setColor,
                    ref colorCurve);
            }


            PresetAppearanceValues values;
            //searches for local presets first, which override global presets
            if (TAnimBuilder.GetPresetFromArray(effectTag, data.presets, out values))
            {
                AssignValues(values);
                return;
            }

            //global presets
            if (TAnimBuilder.TryGetGlobalPresetAppearance(effectTag, out values)) AssignValues(values);
        }

        public static bool SetPreset<T>(
            bool isAppearance,
            T values,
            ref ThreeAxisEffector movement,
            ref float showDuration,
            ref ThreeAxisEffector rotation,
            ref TwoAxisEffector scale,
            ref Quaternion rotationQua,
            ref bool hasTransformEffects,
            ref bool setColor,
            ref ColorCurve colorCurve
        ) where T : PresetBaseValues
        {
            values.Initialize(isAppearance);
            showDuration = values.GetMaxDuration();


            movement = new ThreeAxisEffector(
                values.movementX,
                values.movementY,
                values.movementZ);

            scale = new TwoAxisEffector(
                values.scaleX,
                values.scaleY
            );

            rotation = new ThreeAxisEffector(
                values.rotX,
                values.rotY,
                values.rotZ
            );

            rotationQua = Quaternion.identity;

            hasTransformEffects = values.movementX.enabled || values.movementY.enabled || values.movementZ.enabled
                                  || values.rotX.enabled || values.rotY.enabled || values.rotZ.enabled
                                  || values.scaleX.enabled || values.scaleY.enabled;

            setColor = values.color.enabled;
            if (setColor)
            {
                colorCurve = values.color;
                colorCurve.Initialize(isAppearance);
            }

            return hasTransformEffects || setColor;
        }


        public override void ApplyEffect(ref CharacterData data, int charIndex)
        {
            if (!enabled)
                return;

            if (hasTransformEffects)
            {
                offset = (data.vertices[0] + data.vertices[2]) / 2f;


                rotationQua.eulerAngles = rotation.EvaluateEffect(data.passedTime, charIndex);


                matrix.SetTRS(
                    movement.EvaluateEffect(data.passedTime, charIndex) * uniformIntensity,
                    rotationQua,
                    scale.EvaluateEffect(data.passedTime, charIndex));

                for (byte i = 0; i < data.vertices.Length; i++)
                {
                    data.vertices[i] -= offset;
                    data.vertices[i] = matrix.MultiplyPoint3x4(data.vertices[i]);
                    data.vertices[i] += offset;
                }
            }

            if (setColor)
            {
                color = colorCurve.GetColor(data.passedTime, charIndex);
                data.colors.LerpUnclamped(color, 1 - data.passedTime / effectDuration);
            }
        }

        #region Effector classes

        internal abstract class Effector
        {
            protected abstract Vector3 _EvaluateEffect(float passedTime, int charInde);

            public Vector3 EvaluateEffect(float passedTime, int charIndex)
            {
                return _EvaluateEffect(passedTime, charIndex);
            }
        }

        internal sealed class ThreeAxisEffector : Effector
        {
            private readonly EffectEvaluator x;
            private readonly EffectEvaluator y;
            private readonly EffectEvaluator z;

            public ThreeAxisEffector(EffectEvaluator x, EffectEvaluator y, EffectEvaluator z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            protected override Vector3 _EvaluateEffect(float passedTime, int charIndex)
            {
                return new Vector3(
                    x.Evaluate(passedTime, charIndex),
                    y.Evaluate(passedTime, charIndex),
                    z.Evaluate(passedTime, charIndex)
                );
            }
        }

        internal sealed class TwoAxisEffector : Effector
        {
            private readonly EffectEvaluator x;
            private readonly EffectEvaluator y;

            public TwoAxisEffector(EffectEvaluator x, EffectEvaluator y)
            {
                this.x = x;
                this.y = y;
            }

            protected override Vector3 _EvaluateEffect(float passedTime, int charIndex)
            {
                return new Vector3(
                    x.Evaluate(passedTime, charIndex),
                    y.Evaluate(passedTime, charIndex),
                    1
                );
            }
        }

        #endregion
    }
}