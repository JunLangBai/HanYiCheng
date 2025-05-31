using UnityEngine;

namespace TensorFlowLite
{
    /// <summary>
    ///     RectTransformationCalculator from MediaPipe
    ///     https://github.com/google/mediapipe/blob/master/mediapipe/calculators/util/rect_transformation_calculator.cc
    /// </summary>
    public class RectTransformationCalculator
    {
        public static readonly Matrix4x4 POP_MATRIX = Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0));
        public static readonly Matrix4x4 PUSH_MATRIX = Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0));

        public static Matrix4x4 CalcMatrix(in Options options)
        {
            var rotation = Quaternion.Euler(0, 0, options.rotationDegree);
            var size = Vector2.Scale(options.rect.size, options.scale);
            var shift = options.shift;

            // Calc center position
            var center = options.rect.center + new Vector2(-0.5f, -0.5f);
            center = rotation * center;
            center += shift * size;
            center /= size;

            var trs = Matrix4x4.TRS(
                new Vector3(-center.x, -center.y, 0),
                rotation,
                new Vector3(1 / size.x, -1 / size.y, 1)
            );

            if (!options.IsCameraModified) return POP_MATRIX * trs * PUSH_MATRIX;
            var cameraMtx = Matrix4x4.TRS(
                new Vector3(0, 0, 0),
                Quaternion.identity,
                new Vector3(
                    options.mirrorHorizontal ? -1 : 1,
                    options.mirrorVertical ? -1 : 1,
                    1
                )
            );
            return POP_MATRIX * trs * cameraMtx * PUSH_MATRIX;
        }

        public ref struct Options
        {
            /// <summary>
            ///     The Normalized Rect (0, 0, 1, 1)
            /// </summary>
            public Rect rect;

            public float rotationDegree;
            public Vector2 shift;
            public Vector2 scale;

            public bool mirrorHorizontal;
            public bool mirrorVertical;

            public bool IsCameraModified => mirrorHorizontal || mirrorVertical;
        }
    }
}