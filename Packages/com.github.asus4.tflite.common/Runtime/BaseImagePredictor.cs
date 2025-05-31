using System;
using System.Threading;
using UnityEngine;
#if TFLITE_UNITASK_ENABLED
using Cysharp.Threading.Tasks;
#endif // TFLITE_UNITASK_ENABLED

namespace TensorFlowLite
{
    /// <summary>
    ///     Base class for predictor that takes a Texture as an input
    /// </summary>
    /// <typeparam name="T">A type of input tensor (float, sbyte etc.)</typeparam>
    [Obsolete("BaseImagePredictor is obsolete, use BaseVisionTask instead")]
    public abstract class BaseImagePredictor<T> : IDisposable
        where T : struct
    {
        protected readonly int channels;
        protected readonly int height;
        protected readonly T[,,] inputTensor;
        protected readonly Interpreter interpreter;
        protected readonly TextureResizer resizer;
        protected readonly TextureToTensor tex2tensor;
        protected readonly int width;
        protected TextureResizer.ResizeOptions resizeOptions;

        public BaseImagePredictor(byte[] modelData, InterpreterOptions options)
        {
            try
            {
                interpreter = new Interpreter(modelData, options);
            }
            catch (Exception e)
            {
                interpreter?.Dispose();
                throw e;
            }

#if UNITY_EDITOR
            interpreter.LogIOInfo();
#endif

            // Initialize inputs
            {
                var inputShape0 = interpreter.GetInputTensorInfo(0).shape;
                height = inputShape0[1];
                width = inputShape0[2];
                channels = inputShape0[3];
                inputTensor = new T[height, width, channels];

                var inputCount = interpreter.GetInputTensorCount();
                for (var i = 0; i < inputCount; i++)
                {
                    var shape = interpreter.GetInputTensorInfo(i).shape;
                    interpreter.ResizeInputTensor(i, shape);
                }

                interpreter.AllocateTensors();
            }

            tex2tensor = new TextureToTensor();
            resizer = new TextureResizer();
            resizeOptions = new TextureResizer.ResizeOptions
            {
                aspectMode = AspectMode.Fill,
                rotationDegree = 0,
                mirrorHorizontal = false,
                mirrorVertical = false,
                width = width,
                height = height
            };
        }

        public BaseImagePredictor(string modelPath, InterpreterOptions options)
            : this(FileUtil.LoadFile(modelPath), options)
        {
        }

        public BaseImagePredictor(string modelPath, TfLiteDelegateType delegateType)
            : this(modelPath, CreateOptions(delegateType))
        {
        }

        public Texture inputTex =>
            tex2tensor.texture != null
                ? tex2tensor.texture
                : resizer.texture;

        public Material transformMat => resizer.material;

        public TextureResizer.ResizeOptions ResizeOptions
        {
            get => resizeOptions;
            set => resizeOptions = value;
        }

        public virtual void Dispose()
        {
            interpreter?.Dispose();
            tex2tensor?.Dispose();
            resizer?.Dispose();
        }

        protected static InterpreterOptions CreateOptions(TfLiteDelegateType delegateType)
        {
            var options = new InterpreterOptions();
            options.AutoAddDelegate(delegateType, typeof(T));
            return options;
        }

        public abstract void Invoke(Texture inputTex);

        protected void ToTensor(Texture inputTex, float[,,] inputs)
        {
            var tex = resizer.Resize(inputTex, resizeOptions);
            tex2tensor.ToTensor(tex, inputs);
        }

        protected void ToTensor(RenderTexture inputTex, float[,,] inputs, bool resize)
        {
            var tex = resize ? resizer.Resize(inputTex, resizeOptions) : inputTex;
            tex2tensor.ToTensor(tex, inputs);
        }

        protected void ToTensor(Texture inputTex, float[,,] inputs, float offset, float scale)
        {
            var tex = resizer.Resize(inputTex, resizeOptions);
            tex2tensor.ToTensor(tex, inputs, offset, scale);
        }

        protected void ToTensor(Texture inputTex, byte[,,] inputs)
        {
            var tex = resizer.Resize(inputTex, resizeOptions);
            tex2tensor.ToTensor(tex, inputs);
        }

        protected void ToTensor(Texture inputTex, int[,,] inputs)
        {
            var tex = resizer.Resize(inputTex, resizeOptions);
            tex2tensor.ToTensor(tex, inputs);
        }

        // ToTensorAsync methods are only available when UniTask is installed via Unity Package Manager.
        // TODO: consider using native Task or Unity Coroutine
#if TFLITE_UNITASK_ENABLED
        protected async UniTask<bool> ToTensorAsync(Texture inputTex, float[,,] inputs,
            CancellationToken cancellationToken)
        {
            var tex = resizer.Resize(inputTex, resizeOptions);
            await tex2tensor.ToTensorAsync(tex, inputs, cancellationToken);
            return true;
        }

        protected async UniTask<bool> ToTensorAsync(RenderTexture inputTex, float[,,] inputs, bool resize,
            CancellationToken cancellationToken)
        {
            var tex = resize ? resizer.Resize(inputTex, resizeOptions) : inputTex;
            await tex2tensor.ToTensorAsync(tex, inputs, cancellationToken);
            return true;
        }

        protected async UniTask<bool> ToTensorAsync(Texture inputTex, byte[,,] inputs,
            CancellationToken cancellationToken)
        {
            var tex = resizer.Resize(inputTex, resizeOptions);
            await tex2tensor.ToTensorAsync(tex, inputs, cancellationToken);
            return true;
        }

        protected async UniTask<bool> ToTensorAsync(Texture inputTex, int[,,] inputs,
            CancellationToken cancellationToken)
        {
            var tex = resizer.Resize(inputTex, resizeOptions);
            await tex2tensor.ToTensorAsync(tex, inputs, cancellationToken);
            return true;
        }
#endif // TFLITE_UNITASK_ENABLED
    }
}