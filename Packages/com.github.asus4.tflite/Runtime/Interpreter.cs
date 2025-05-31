/* Copyright 2018 The TensorFlow Authors. All Rights Reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
==============================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using TfLiteInterpreter = System.IntPtr;
using TfLiteInterpreterOptions = System.IntPtr;
using TfLiteModel = System.IntPtr;
using TfLiteTensor = System.IntPtr;

namespace TensorFlowLite
{
    /// <summary>
    ///     Simple C# bindings for the experimental TensorFlowLite C API.
    /// </summary>
    public class Interpreter : IDisposable
    {
        public struct TensorInfo
        {
            public string name { get; internal set; }
            public DataType type { get; internal set; }
            public int[] shape { get; internal set; }
            public QuantizationParams quantizationParams { get; internal set; }

            public override string ToString()
            {
                return string.Format("name: {0}, type: {1}, dimensions: {2}, quantizationParams: {3}",
                    name,
                    type,
                    "[" + string.Join(",", shape) + "]",
                    "{" + quantizationParams + "}");
            }
        }

        private TfLiteModel model = IntPtr.Zero;
        private readonly InterpreterOptions options;
        private readonly GCHandle modelDataHandle;
        private readonly Dictionary<int, GCHandle> inputDataHandles = new();
        private readonly Dictionary<int, GCHandle> outputDataHandles = new();


        internal TfLiteInterpreter InterpreterPointer { get; private set; } = IntPtr.Zero;

        public Interpreter(byte[] modelData) : this(modelData, null)
        {
        }

        public Interpreter(byte[] modelData, InterpreterOptions options)
        {
            modelDataHandle = GCHandle.Alloc(modelData, GCHandleType.Pinned);
            var modelDataPtr = modelDataHandle.AddrOfPinnedObject();
            model = TfLiteModelCreate(modelDataPtr, modelData.Length);
            if (model == IntPtr.Zero) throw new Exception("Failed to create TensorFlowLite Model");

            this.options = options ?? new InterpreterOptions();

            InterpreterPointer = TfLiteInterpreterCreate(model, options.nativePtr);
            if (InterpreterPointer == IntPtr.Zero) throw new Exception("Failed to create TensorFlowLite Interpreter");
        }


        public virtual void Dispose()
        {
            if (InterpreterPointer != IntPtr.Zero)
            {
                TfLiteInterpreterDelete(InterpreterPointer);
                InterpreterPointer = IntPtr.Zero;
            }

            if (model != IntPtr.Zero)
            {
                TfLiteModelDelete(model);
                model = IntPtr.Zero;
            }

            options?.Dispose();

            foreach (var handle in inputDataHandles.Values) handle.Free();
            foreach (var handle in outputDataHandles.Values) handle.Free();
            modelDataHandle.Free();
        }

        public virtual void Invoke()
        {
            ThrowIfError(TfLiteInterpreterInvoke(InterpreterPointer));
        }

        public int GetInputTensorCount()
        {
            return TfLiteInterpreterGetInputTensorCount(InterpreterPointer);
        }

        public void SetInputTensorData(int inputTensorIndex, Array inputTensorData)
        {
            if (!inputDataHandles.TryGetValue(inputTensorIndex, out var tensorDataHandle))
            {
                tensorDataHandle = GCHandle.Alloc(inputTensorData, GCHandleType.Pinned);
                inputDataHandles.Add(inputTensorIndex, tensorDataHandle);
            }

            var tensorDataPtr = tensorDataHandle.AddrOfPinnedObject();
            var tensor = TfLiteInterpreterGetInputTensor(InterpreterPointer, inputTensorIndex);
            ThrowIfError(TfLiteTensorCopyFromBuffer(tensor, tensorDataPtr, Buffer.ByteLength(inputTensorData)));
        }

        public unsafe void SetInputTensorData<T>(int inputTensorIndex, in ReadOnlySpan<T> inputTensorData)
            where T : unmanaged
        {
            fixed (T* dataPtr = inputTensorData)
            {
                var tensorDataPtr = (IntPtr)dataPtr;
                var tensor = TfLiteInterpreterGetInputTensor(InterpreterPointer, inputTensorIndex);
                ThrowIfError(TfLiteTensorCopyFromBuffer(
                    tensor, tensorDataPtr, inputTensorData.Length * UnsafeUtility.SizeOf<T>()));
            }
        }

        public unsafe void SetInputTensorData<T>(int inputTensorIndex, in NativeArray<T> inputTensorData)
            where T : unmanaged
        {
            var tensorDataPtr = (IntPtr)inputTensorData.GetUnsafePtr();
            var tensor = TfLiteInterpreterGetInputTensor(InterpreterPointer, inputTensorIndex);
            ThrowIfError(TfLiteTensorCopyFromBuffer(
                tensor, tensorDataPtr, inputTensorData.Length * UnsafeUtility.SizeOf<T>()));
        }

        public void ResizeInputTensor(int inputTensorIndex, int[] inputTensorShape)
        {
            ThrowIfError(TfLiteInterpreterResizeInputTensor(
                InterpreterPointer, inputTensorIndex, inputTensorShape, inputTensorShape.Length));
        }

        public void AllocateTensors()
        {
            ThrowIfError(TfLiteInterpreterAllocateTensors(InterpreterPointer));
        }

        /// <summary>
        ///     Returns the number of output tensors associated with the model.
        /// </summary>
        /// <returns>The number of output</returns>
        public int GetOutputTensorCount()
        {
            return TfLiteInterpreterGetOutputTensorCount(InterpreterPointer);
        }

        public void GetOutputTensorData(int outputTensorIndex, Array outputTensorData)
        {
            if (!outputDataHandles.TryGetValue(outputTensorIndex, out var tensorDataHandle))
            {
                tensorDataHandle = GCHandle.Alloc(outputTensorData, GCHandleType.Pinned);
                outputDataHandles.Add(outputTensorIndex, tensorDataHandle);
            }

            var tensorDataPtr = tensorDataHandle.AddrOfPinnedObject();
            var tensor = TfLiteInterpreterGetOutputTensor(InterpreterPointer, outputTensorIndex);
            ThrowIfError(TfLiteTensorCopyToBuffer(tensor, tensorDataPtr, Buffer.ByteLength(outputTensorData)));
        }

        public unsafe void GetOutputTensorData<T>(int outputTensorIndex, in Span<T> outputTensorData)
            where T : unmanaged
        {
            fixed (T* dataPtr = outputTensorData)
            {
                var tensorDataPtr = (IntPtr)dataPtr;
                var tensor = TfLiteInterpreterGetOutputTensor(InterpreterPointer, outputTensorIndex);
                ThrowIfError(TfLiteTensorCopyToBuffer(
                    tensor, tensorDataPtr, outputTensorData.Length * UnsafeUtility.SizeOf<T>()));
            }
        }

        /// <summary>
        ///     Tries to cancel any in-flight invocation.
        ///     \note This only cancels `TfLiteInterpreterInvoke` calls that happen before
        ///     calling this and it does not cancel subsequent invocations.
        ///     \note Calling this function will also cancel any in-flight invocations of
        ///     SignatureRunners constructed from this interpreter.
        ///     Non-blocking and thread safe.
        /// </summary>
        public void Cancel()
        {
            ThrowIfError(TfLiteInterpreterCancel(InterpreterPointer));
        }

        public TensorInfo GetInputTensorInfo(int index)
        {
            var tensor = TfLiteInterpreterGetInputTensor(InterpreterPointer, index);
            return GetTensorInfo(tensor);
        }

        public TensorInfo GetOutputTensorInfo(int index)
        {
            var tensor = TfLiteInterpreterGetOutputTensor(InterpreterPointer, index);
            return GetTensorInfo(tensor);
        }

        /// <summary>
        ///     Returns a string describing version information of the TensorFlow Lite library.
        ///     TensorFlow Lite uses semantic versioning.
        /// </summary>
        /// <returns>A string describing version information</returns>
        public static string GetVersion()
        {
            return Marshal.PtrToStringAnsi(TfLiteVersion());
        }

        public static string GetExtensionApisVersion()
        {
            return Marshal.PtrToStringAnsi(TfLiteExtensionApisVersion());
        }

        public static int GetSchemaVersion()
        {
            return TfLiteSchemaVersion();
        }

        private static string GetTensorName(TfLiteTensor tensor)
        {
            return Marshal.PtrToStringAnsi(TfLiteTensorName(tensor));
        }

        protected static TensorInfo GetTensorInfo(TfLiteTensor tensor)
        {
            var dimensions = new int[TfLiteTensorNumDims(tensor)];
            for (var i = 0; i < dimensions.Length; i++) dimensions[i] = TfLiteTensorDim(tensor, i);
            return new TensorInfo
            {
                name = GetTensorName(tensor),
                type = TfLiteTensorType(tensor),
                shape = dimensions,
                quantizationParams = TfLiteTensorQuantizationParams(tensor)
            };
        }

        protected TfLiteTensor GetInputTensor(int inputTensorIndex)
        {
            return TfLiteInterpreterGetInputTensor(InterpreterPointer, inputTensorIndex);
        }

        protected TfLiteTensor GetOutputTensor(int outputTensorIndex)
        {
            return TfLiteInterpreterGetOutputTensor(InterpreterPointer, outputTensorIndex);
        }

        protected static void ThrowIfError(Status status)
        {
            switch (status)
            {
                case Status.Ok:
                    return;
                case Status.Error:
                    throw new Exception("TensorFlowLite operation failed.");
                case Status.DelegateError:
                    throw new Exception("TensorFlowLite delegate operation failed.");
                case Status.ApplicationError:
                    throw new Exception("Applying TensorFlowLite delegate operation failed.");
                case Status.DelegateDataNotFound:
                    throw new Exception("Serialized delegate data not being found.");
                case Status.DelegateDataWriteError:
                    throw new Exception("Writing data to delegate failed.");
                case Status.DelegateDataReadError:
                    throw new Exception("Reading data from delegate failed.");
                case Status.UnresolvedOps:
                    throw new Exception("Ops not found.");
                default:
                    throw new Exception($"Unknown TensorFlowLite error: {status}");
            }
        }

        #region Externs

#if UNITY_IOS && !UNITY_EDITOR
        internal const string TensorFlowLibrary = "__Internal";
#elif UNITY_ANDROID && !UNITY_EDITOR
        internal const string TensorFlowLibrary = "libtensorflowlite_jni";
#else
        internal const string TensorFlowLibrary = "libtensorflowlite_c";
#endif

        // TfLiteStatus
        public enum Status
        {
            Ok = 0,
            Error = 1,
            DelegateError = 2,
            ApplicationError = 3,
            DelegateDataNotFound = 4,
            DelegateDataWriteError = 5,
            DelegateDataReadError = 6,
            UnresolvedOps = 7,
            Cancelled = 8
        }

        // TfLiteType
        public enum DataType
        {
            NoType = 0,
            Float32 = 1,
            Int32 = 2,
            UInt8 = 3,
            Int64 = 4,
            String = 5,
            Bool = 6,
            Int16 = 7,
            Complex64 = 8,
            Int8 = 9,
            Float16 = 10,
            Float64 = 11,
            Complex128 = 12,
            UInt64 = 13,
            Resource = 14,
            Variant = 15,
            UInt32 = 16,
            UInt16 = 17,
            Int4 = 18,
            BFloat16 = 19
        }

        public struct QuantizationParams
        {
            public float scale;
            public int zeroPoint;

            public readonly override string ToString()
            {
                return string.Format("scale: {0} zeroPoint: {1}", scale, zeroPoint);
            }
        }

        [DllImport(TensorFlowLibrary)]
        private static extern IntPtr TfLiteVersion();

        [DllImport(TensorFlowLibrary)]
        private static extern IntPtr TfLiteExtensionApisVersion();

        [DllImport(TensorFlowLibrary)]
        private static extern int TfLiteSchemaVersion();

        [DllImport(TensorFlowLibrary)]
        private static extern TfLiteInterpreter TfLiteModelCreate(IntPtr model_data, int model_size);

        [DllImport(TensorFlowLibrary)]
        private static extern void TfLiteModelDelete(TfLiteModel model);

        [DllImport(TensorFlowLibrary)]
        private static extern TfLiteInterpreter TfLiteInterpreterCreate(
            TfLiteModel model,
            TfLiteInterpreterOptions optional_options);

        [DllImport(TensorFlowLibrary)]
        private static extern void TfLiteInterpreterDelete(TfLiteInterpreter interpreter);

        [DllImport(TensorFlowLibrary)]
        private static extern int TfLiteInterpreterGetInputTensorCount(
            TfLiteInterpreter interpreter);

        [DllImport(TensorFlowLibrary)]
        private static extern TfLiteTensor TfLiteInterpreterGetInputTensor(
            TfLiteInterpreter interpreter,
            int input_index);

        [DllImport(TensorFlowLibrary)]
        private static extern Status TfLiteInterpreterResizeInputTensor(
            TfLiteInterpreter interpreter,
            int input_index,
            int[] input_dims,
            int input_dims_size);

        [DllImport(TensorFlowLibrary)]
        private static extern Status TfLiteInterpreterAllocateTensors(
            TfLiteInterpreter interpreter);

        [DllImport(TensorFlowLibrary)]
        private static extern Status TfLiteInterpreterInvoke(TfLiteInterpreter interpreter);

        [DllImport(TensorFlowLibrary)]
        private static extern int TfLiteInterpreterGetOutputTensorCount(
            TfLiteInterpreter interpreter);

        [DllImport(TensorFlowLibrary)]
        private static extern TfLiteTensor TfLiteInterpreterGetOutputTensor(
            TfLiteInterpreter interpreter,
            int output_index);

        [DllImport(TensorFlowLibrary)]
        private static extern Status TfLiteInterpreterCancel(TfLiteInterpreter interpreter);

        [DllImport(TensorFlowLibrary)]
        private static extern DataType TfLiteTensorType(TfLiteTensor tensor);

        [DllImport(TensorFlowLibrary)]
        private static extern int TfLiteTensorNumDims(TfLiteTensor tensor);

        [DllImport(TensorFlowLibrary)]
        private static extern int TfLiteTensorDim(TfLiteTensor tensor, int dim_index);

        [DllImport(TensorFlowLibrary)]
        private static extern uint TfLiteTensorByteSize(TfLiteTensor tensor);

        [DllImport(TensorFlowLibrary)]
        private static extern IntPtr TfLiteTensorName(TfLiteTensor tensor);

        [DllImport(TensorFlowLibrary)]
        private static extern QuantizationParams TfLiteTensorQuantizationParams(TfLiteTensor tensor);

        [DllImport(TensorFlowLibrary)]
        private static extern Status TfLiteTensorCopyFromBuffer(
            TfLiteTensor tensor,
            IntPtr input_data,
            int input_data_size);

        [DllImport(TensorFlowLibrary)]
        private static extern Status TfLiteTensorCopyToBuffer(
            TfLiteTensor tensor,
            IntPtr output_data,
            int output_data_size);

        #endregion
    }
}