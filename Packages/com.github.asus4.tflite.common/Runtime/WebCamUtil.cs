using System.Linq;
using UnityEngine;

namespace TensorFlowLite
{
    public static class WebCamUtil
    {
        public static string FindName(PreferSpec spec = default)
        {
            var devices = WebCamTexture.devices;
            if (Application.isMobilePlatform)
            {
                var prefers = devices.OrderByDescending(d => spec.GetScore(d));
                return prefers.First().name;
            }

            return devices.First().name;
        }

        public static string FindName(WebCamKind kind, bool isFrontFacing)
        {
            return FindName(new PreferSpec(kind, isFrontFacing));
        }

        public struct PreferSpec
        {
            public readonly WebCamKind kind;
            public readonly bool isFrontFacing;

            public PreferSpec(WebCamKind kind, bool isFrontFacing)
            {
                this.kind = kind;
                this.isFrontFacing = isFrontFacing;
            }

            public int GetScore(in WebCamDevice device)
            {
                var score = 0;
                if (device.isFrontFacing == isFrontFacing) score++;
                if (device.kind == kind) score++;
                return score;
            }
        }
    }
}