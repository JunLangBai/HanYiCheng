#if TFLITE_UNITASK_ENABLED
using Cysharp.Threading.Tasks;
#endif // TFLITE_UNITASK_ENABLED
using System;
using System.Threading;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;
using TensorInfo = TensorFlowLite.Interpreter.TensorInfo;

namespace TensorFlowLite
{
    /// <summary>
    ///     Base class for vision task that takes a Texture as an input
    /// </summary>
    public abstract class BaseVisionTask : IDisposable
    {
        // Profilers
        protected static readonly ProfilerMarker
            preprocessPerfMarker = new($"{typeof(BaseVisionTask).Name}.Preprocess");

        protected static readonly ProfilerMarker runPerfMarker = new($"{typeof(BaseVisionTask).Name}.Session.Run");

        protected static readonly ProfilerMarker postprocessPerfMarker =
            new($"{typeof(BaseVisionTask).Name}.Postprocess");

        protected int channels;
        protected int height;
        protected int inputTensorIndex = 0;
        protected Interpreter interpreter;

        private bool isDisposed;
        protected TextureToNativeTensor textureToTensor;
        protected int width;

        public AspectMode AspectMode { get; set; } = AspectMode.None;

        public virtual void Dispose()
        {
            if (!isDisposed)
            {
                interpreter?.Dispose();
                textureToTensor?.Dispose();
                isDisposed = true;
            }
        }

        /// <summary>
        ///     Load model from byte array
        /// </summary>
        /// <param name="model"></param>
        /// <param name="options"></param>
        public virtual void Load(byte[] model, InterpreterOptions options)
        {
            try
            {
                interpreter = new Interpreter(model, options);
            }
            catch (Exception e)
            {
                interpreter?.Dispose();
                throw e;
            }

            interpreter.LogIOInfo();

            var inputTensorInfo = interpreter.GetInputTensorInfo(inputTensorIndex);
            InitializeInputsOutputs(inputTensorInfo);
            textureToTensor = CreateTextureToTensor(inputTensorInfo);
        }

        /// <summary>
        ///     Run the model with the input texture
        /// </summary>
        /// <param name="texture">A texture for model input</param>
        public virtual void Run(Texture texture)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(BaseVisionTask));

            // Pre process
            preprocessPerfMarker.Begin();
            PreProcess(texture);
            preprocessPerfMarker.End();

            // Run inference
            runPerfMarker.Begin();
            interpreter.Invoke();
            runPerfMarker.End();

            // Post process
            postprocessPerfMarker.Begin();
            PostProcess();
            postprocessPerfMarker.End();
        }

        /// <summary>
        ///     Pre process the input texture
        ///     Set all input tensors for the model
        /// </summary>
        /// <param name="texture">An input texture</param>
        protected virtual void PreProcess(Texture texture)
        {
            var input = textureToTensor.Transform(texture, AspectMode);
            interpreter.SetInputTensorData(inputTensorIndex, input);
        }

        /// <summary>
        ///     Get the output tensors and do post process in subclass
        /// </summary>
        protected abstract void PostProcess();

        /// <summary>
        ///     Default implementation of InitializeInputsOutputs
        ///     Override this in subclass if needed
        /// </summary>
        protected virtual void InitializeInputsOutputs(TensorInfo inputTensorInfo)
        {
            var inputShape = inputTensorInfo.shape;
            Assert.AreEqual(4, inputShape.Length);
            Assert.AreEqual(1, inputShape[0], $"The batch size of the model must be 1. But got {inputShape[0]}");
            height = inputShape[1];
            width = inputShape[2];
            channels = inputShape[3];

            var inputCount = interpreter.GetInputTensorCount();
            for (var i = 0; i < inputCount; i++)
            {
                var shape = interpreter.GetInputTensorInfo(i).shape;
                interpreter.ResizeInputTensor(i, shape);
            }

            interpreter.AllocateTensors();
        }

        /// <summary>
        ///     Create TextureToTensor for this model.
        ///     Override this in subclass if needed
        /// </summary>
        /// <returns>A TextureToNativeTensor instance</returns>
        protected virtual TextureToNativeTensor CreateTextureToTensor(TensorInfo inputTensorInfo)
        {
            return TextureToNativeTensor.Create(new TextureToNativeTensor.Options
            {
                compute = null,
                kernel = 0,
                width = width,
                height = height,
                channels = channels,
                inputType = inputTensorInfo.type
            });
        }

        // Only available when UniTask is installed
#if TFLITE_UNITASK_ENABLED
        public virtual async UniTask RunAsync(Texture texture, CancellationToken cancellationToken)
        {
            await RunAsync(texture, cancellationToken, true);
        }

        public virtual async UniTask RunAsync(
            Texture texture,
            CancellationToken cancellationToken,
            bool waitForMainThread)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(BaseVisionTask));

            // Pre process
            // Note: Profiler doesn't work with async
            await PreProcessAsync(texture, cancellationToken);

            // Run inference in BG thread
            await UniTask.SwitchToThreadPool();
            runPerfMarker.Begin();
            interpreter.Invoke();
            runPerfMarker.End();

            // Post process
            await PostProcessAsync(cancellationToken);

            if (waitForMainThread)
                // Back to main thread
                await UniTask.SwitchToMainThread();
        }

        protected virtual async UniTask PreProcessAsync(Texture texture, CancellationToken cancellationToken)
        {
            var input = await textureToTensor.TransformAsync(texture, AspectMode, cancellationToken);
            interpreter.SetInputTensorData(inputTensorIndex, input);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected virtual async UniTask PostProcessAsync(CancellationToken cancellationToken)
        {
            PostProcess();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

#endif // TFLITE_UNITASK_ENABLED
    }
}