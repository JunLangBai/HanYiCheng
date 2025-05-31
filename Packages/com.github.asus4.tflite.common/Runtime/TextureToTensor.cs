using System;
using System.Threading;
using UnityEngine;
#if TFLITE_UNITASK_ENABLED
using Cysharp.Threading.Tasks;
#endif // TFLITE_UNITASK_ENABLED

namespace TensorFlowLite
{
    /// <summary>
    ///     Converts Texture to Tensor (NHWC layout)
    /// </summary>
    [Obsolete("TextureToTensor is obsolete, use TextureToNativeTensor instead")]
    public class TextureToTensor : IDisposable
    {
        private static readonly int InputTexture = Shader.PropertyToID("InputTexture");
        private static readonly int OutputFloatTensor = Shader.PropertyToID("OutputFloatTensor");
        private static readonly int TextureWidth = Shader.PropertyToID("TextureWidth");
        private static readonly int TextureHeight = Shader.PropertyToID("TextureHeight");
        private readonly ComputeShader compute;
        private ComputeBuffer tensorBuffer;


        public TextureToTensor()
        {
            compute = Resources.Load<ComputeShader>("com.github.asus4.tflite.common/TextureToTensor");
        }

        public Texture2D texture { get; private set; }

        public void Dispose()
        {
            DisposeUtil.TryDispose(texture);
            DisposeUtil.TryDispose(tensorBuffer);
        }

        public void ToTensor(RenderTexture texture, byte[,,] inputs)
        {
            var pixels = FetchToTexture2D(texture).GetRawTextureData<Color32>();
            var width = texture.width;
            var height = texture.height - 1;
            for (var i = 0; i < pixels.Length; i++)
            {
                var y = height - i / width;
                var x = i % width;
                inputs[y, x, 0] = pixels[i].r;
                inputs[y, x, 1] = pixels[i].b;
                inputs[y, x, 2] = pixels[i].g;
            }
        }

        public void ToTensor(RenderTexture texture, int[,,] inputs)
        {
            var pixels = FetchToTexture2D(texture).GetRawTextureData<Color32>();
            var width = texture.width;
            var height = texture.height - 1;
            for (var i = 0; i < pixels.Length; i++)
            {
                var y = height - i / width;
                var x = i % width;
                inputs[y, x, 0] = pixels[i].r;
                inputs[y, x, 1] = pixels[i].g;
                inputs[y, x, 2] = pixels[i].b;
            }
        }

        public void ToTensor(RenderTexture texture, float[,,] inputs)
        {
            if (texture.width % 8 != 0 || texture.height % 8 != 0)
                ToTensorCPU(texture, inputs);
            else
                ToTensorGPU(texture, inputs);
        }

        public void ToTensor(RenderTexture texture, out ComputeBuffer buffer)
        {
            Debug.Assert(texture.width % 8 == 0);
            Debug.Assert(texture.height % 8 == 0);
            ToTensorGPU(texture);
            buffer = tensorBuffer;
        }

        public void ToTensor(RenderTexture texture, float[,,] inputs, float offset, float scale)
        {
            // TODO: optimize this
            var pixels = FetchToTexture2D(texture).GetRawTextureData<Color32>();
            var width = texture.width;
            var height = texture.height - 1;
            for (var i = 0; i < pixels.Length; i++)
            {
                var y = height - i / width;
                var x = i % width;
                inputs[y, x, 0] = (pixels[i].r - offset) * scale;
                inputs[y, x, 1] = (pixels[i].g - offset) * scale;
                inputs[y, x, 2] = (pixels[i].b - offset) * scale;
            }
        }

        private void ToTensorCPU(RenderTexture texture, float[,,] inputs)
        {
            var pixels = FetchToTexture2D(texture).GetRawTextureData<Color32>();

            var width = texture.width;
            var height = texture.height - 1;
            const float scale = 255f;
            for (var i = 0; i < pixels.Length; i++)
            {
                var y = height - i / width;
                var x = i % width;
                inputs[y, x, 0] = pixels[i].r / scale;
                inputs[y, x, 1] = pixels[i].g / scale;
                inputs[y, x, 2] = pixels[i].b / scale;
            }
        }

        private void ToTensorGPU(RenderTexture texture)
        {
            var width = texture.width;
            var height = texture.height;

            if (tensorBuffer == null || tensorBuffer.count != width * height)
            {
                DisposeUtil.TryDispose(tensorBuffer);
                tensorBuffer = new ComputeBuffer(width * height, sizeof(float) * 3);
            }

            var kernel = compute.FindKernel("TextureToFloatTensor");

            compute.SetTexture(kernel, InputTexture, texture);
            compute.SetBuffer(kernel, OutputFloatTensor, tensorBuffer);
            compute.SetInt(TextureWidth, width);
            compute.SetInt(TextureHeight, height);

            compute.Dispatch(kernel, width / 8, height / 8, 1);
        }

        private void ToTensorGPU(RenderTexture texture, float[,,] inputs)
        {
            ToTensorGPU(texture);
            tensorBuffer.GetData(inputs);
        }

        private Texture2D FetchToTexture2D(RenderTexture texture)
        {
            if (this.texture == null || !IsSameSize(this.texture, texture))
                // Due to GetRawTextureData() returns incorrect bytes length, we cant use liner color space options
                // https://fogbugz.unity3d.com/default.asp?1271542_g3prspfd30mhavcb
                // fetchTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, 0, false);
                this.texture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            var prevRT = RenderTexture.active;
            RenderTexture.active = texture;

            this.texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            this.texture.Apply();

            RenderTexture.active = prevRT;

            return this.texture;
        }

        private static bool IsSameSize(Texture a, Texture b)
        {
            return a.width == b.width && a.height == b.height;
        }

#if TFLITE_UNITASK_ENABLED
        public async UniTask<bool> ToTensorAsync(RenderTexture texture, float[,,] inputs,
            CancellationToken cancellationToken)
        {
            await UniTask.SwitchToMainThread(PlayerLoopTiming.FixedUpdate, cancellationToken);
            var pixels = FetchToTexture2D(texture).GetRawTextureData<Color32>();
            var width = texture.width;
            var height = texture.height - 1;
            await UniTask.SwitchToThreadPool();

            const float scale = 255f;
            for (var i = 0; i < pixels.Length; i++)
            {
                var y = height - i / width;
                var x = i % width;
                inputs[y, x, 0] = pixels[i].r / scale;
                inputs[y, x, 1] = pixels[i].g / scale;
                inputs[y, x, 2] = pixels[i].b / scale;
            }

            return true;
        }

        public async UniTask<bool> ToTensorAsync(RenderTexture texture, byte[,,] inputs,
            CancellationToken cancellationToken)
        {
            await UniTask.SwitchToMainThread(PlayerLoopTiming.FixedUpdate, cancellationToken);
            var pixels = FetchToTexture2D(texture).GetRawTextureData<Color32>();
            var width = texture.width;
            var height = texture.height - 1;
            await UniTask.SwitchToThreadPool();

            for (var i = 0; i < pixels.Length; i++)
            {
                var y = height - i / width;
                var x = i % width;
                inputs[y, x, 0] = pixels[i].r;
                inputs[y, x, 1] = pixels[i].g;
                inputs[y, x, 2] = pixels[i].b;
            }

            return true;
        }

        public async UniTask<bool> ToTensorAsync(RenderTexture texture, int[,,] inputs,
            CancellationToken cancellationToken)
        {
            await UniTask.SwitchToMainThread(PlayerLoopTiming.FixedUpdate, cancellationToken);
            var pixels = FetchToTexture2D(texture).GetRawTextureData<Color32>();
            var width = texture.width;
            var height = texture.height - 1;
            await UniTask.SwitchToThreadPool();

            for (var i = 0; i < pixels.Length; i++)
            {
                var y = height - i / width;
                var x = i % width;
                inputs[y, x, 0] = pixels[i].r;
                inputs[y, x, 1] = pixels[i].g;
                inputs[y, x, 2] = pixels[i].b;
            }

            return true;
        }
#endif // TFLITE_UNITASK_ENABLED
    }
}