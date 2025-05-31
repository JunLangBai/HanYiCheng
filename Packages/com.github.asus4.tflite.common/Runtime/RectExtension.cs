using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TensorFlowLite
{
    public static class RectExtension
    {
        public static float IntersectionOverUnion(this in Rect rect0, in Rect rect1)
        {
            var sx0 = rect0.xMin;
            var sy0 = rect0.yMin;
            var ex0 = rect0.xMax;
            var ey0 = rect0.yMax;
            var sx1 = rect1.xMin;
            var sy1 = rect1.yMin;
            var ex1 = rect1.xMax;
            var ey1 = rect1.yMax;

            var xmin0 = Mathf.Min(sx0, ex0);
            var ymin0 = Mathf.Min(sy0, ey0);
            var xmax0 = Mathf.Max(sx0, ex0);
            var ymax0 = Mathf.Max(sy0, ey0);
            var xmin1 = Mathf.Min(sx1, ex1);
            var ymin1 = Mathf.Min(sy1, ey1);
            var xmax1 = Mathf.Max(sx1, ex1);
            var ymax1 = Mathf.Max(sy1, ey1);

            var area0 = (ymax0 - ymin0) * (xmax0 - xmin0);
            var area1 = (ymax1 - ymin1) * (xmax1 - xmin1);
            if (area0 <= 0 || area1 <= 0) return 0.0f;

            var intersect_xmin = Mathf.Max(xmin0, xmin1);
            var intersect_ymin = Mathf.Max(ymin0, ymin1);
            var intersect_xmax = Mathf.Min(xmax0, xmax1);
            var intersect_ymax = Mathf.Min(ymax0, ymax1);

            var intersect_area = Mathf.Max(intersect_ymax - intersect_ymin, 0.0f) *
                                 Mathf.Max(intersect_xmax - intersect_xmin, 0.0f);

            return intersect_area / (area0 + area1 - intersect_area);
        }

        public static Rect GetBoundingBox(ReadOnlySpan<Vector2> arr)
        {
            var xMin = float.MaxValue;
            var yMin = float.MaxValue;
            var xMax = float.MinValue;
            var yMax = float.MinValue;

            foreach (var v in arr)
            {
                xMin = Math.Min(xMin, v.x);
                yMin = Math.Min(yMin, v.y);
                xMax = Math.Max(xMax, v.x);
                yMax = Math.Max(yMax, v.y);
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        public static Rect GetBoundingBox(ReadOnlySpan<Vector3> arr)
        {
            var xMin = float.MaxValue;
            var yMin = float.MaxValue;
            var xMax = float.MinValue;
            var yMax = float.MinValue;

            foreach (var v in arr)
            {
                xMin = Math.Min(xMin, v.x);
                yMin = Math.Min(yMin, v.y);
                xMax = Math.Max(xMax, v.x);
                yMax = Math.Max(yMax, v.y);
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        /// <summary>
        ///     Flip Y axis, useful for converting between CV and Unity space
        /// </summary>
        /// <param name="rect">A rect</param>
        /// <param name="height">Height of the space</param>
        /// <returns>A flipped rect</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect FlipY(this in Rect rect, float height = 1f)
        {
            return new Rect(rect.x, height - rect.yMax, rect.width, rect.height);
        }
    }
}