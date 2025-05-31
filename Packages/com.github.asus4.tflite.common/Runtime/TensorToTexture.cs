using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using DataType = TensorFlowLite.Interpreter.DataType;
using Object = UnityEngine.Object;

namespace TensorFlowLite
{
    /// <summary>
    ///     Converts tensor to texture
    /// </summary>
    public sealed class TensorToTexture : IDisposable
    {
        private static readonly int _InputTensor = Shader.PropertyToID("_InputTensor");
        private static readonly int _InputSize = Shader.PropertyToID("_InputSize");
        private static readonly int _OutputTexture = Shader.PropertyToID("_OutputTexture");

        private static readonly Lazy<ComputeShader> DefaultComputeShaderFloat32 = new(()
            => Resources.Load<ComputeShader>("com.github.asus4.tflite.common/TensorToTextureFloat32"));

        private readonly int channels;

        private readonly ComputeShader compute;
        private readonly int height;
        private readonly int kernel;


        private readonly GraphicsBuffer tensorBuffer;
        private readonly int width;

        public TensorToTexture(Options options)
        {
            compute = options.compute != null
                ? options.compute
                : DefaultComputeShaderFloat32.Value;
            kernel = options.kernel;
            width = options.width;
            height = options.height;
            channels = options.channels;

            Assert.IsNotNull(compute, "ComputeShader is not set");

            var stride = channels * DataTypeToStride(options.inputType);
            tensorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, width * height, stride);
            OutputTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                enableRandomWrite = true
            };
            OutputTexture.Create();
        }

        public RenderTexture OutputTexture { get; }

        public void Dispose()
        {
            tensorBuffer.Dispose();
            OutputTexture.Release();
            Object.Destroy(OutputTexture);
        }

        public RenderTexture Convert(Array data)
        {
            tensorBuffer.SetData(data);
            compute.SetInts(_InputSize, width, height);
            compute.SetBuffer(kernel, _InputTensor, tensorBuffer);
            compute.SetTexture(kernel, _OutputTexture, OutputTexture);
            compute.Dispatch(kernel, Mathf.CeilToInt(width / 8f), Mathf.CeilToInt(height / 8f), 1);
            return OutputTexture;
        }

        public RenderTexture Convert<T>(NativeArray<T> data)
            where T : struct
        {
            tensorBuffer.SetData(data);
            compute.SetInts(_InputSize, width, height);
            compute.SetBuffer(kernel, _InputTensor, tensorBuffer);
            compute.SetTexture(kernel, _OutputTexture, OutputTexture);
            compute.Dispatch(kernel, Mathf.CeilToInt(width / 8f), Mathf.CeilToInt(height / 8f), 1);
            return OutputTexture;
        }

        private static int DataTypeToStride(DataType type)
        {
            return type switch
            {
                DataType.Float32 => sizeof(float),
                _ => throw new NotSupportedException($"Unsupported type: {type}")
            };
        }

        [Serializable]
        public class Options
        {
            public ComputeShader compute;
            public int kernel;
            public int width;
            public int height;
            public int channels;
            public DataType inputType = DataType.Float32;
        }
    }
}