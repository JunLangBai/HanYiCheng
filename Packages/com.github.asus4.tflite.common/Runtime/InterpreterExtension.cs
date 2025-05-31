using System;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace TensorFlowLite
{
    /// <summary>
    ///     Extension methods for interpreter
    /// </summary>
    public static class InterpreterExtension
    {
        /// <summary>
        ///     Print the information about the model Inputs/Outputs for debug.
        /// </summary>
        /// <param name="interpreter">A TFLite Interpreter</param>
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        public static void LogIOInfo(this Interpreter interpreter)
        {
            var sb = new StringBuilder();
            sb.Append($"Version: {Interpreter.GetVersion()}, ");
            sb.Append($"Extensions: {Interpreter.GetExtensionApisVersion()}, ");
            sb.Append($"Schema: {Interpreter.GetSchemaVersion()}");

            sb.AppendLine();

            var inputCount = interpreter.GetInputTensorCount();
            var outputCount = interpreter.GetOutputTensorCount();
            for (var i = 0; i < inputCount; i++) sb.AppendLine($"Input [{i}]: {interpreter.GetInputTensorInfo(i)}");
            sb.AppendLine();
            for (var i = 0; i < outputCount; i++) sb.AppendLine($"Output [{i}]: {interpreter.GetOutputTensorInfo(i)}");
            Debug.Log(sb.ToString());
        }

        /// <summary>
        ///     Print the information about the model Inputs/Outputs for debug.
        /// </summary>
        /// <param name="runner">A TFLite SignatureRunner</param>
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("UNITY_EDITOR")]
        public static void LogIOInfo(this SignatureRunner runner)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Version: {Interpreter.GetVersion()}");
            sb.AppendLine();

            var signatureCount = runner.GetSignatureCount();
            for (var i = 0; i < signatureCount; i++) sb.AppendLine($"Signature [{i}]: {runner.GetSignatureKey(i)}");
            sb.AppendLine();

            var signatureInputCount = (int)runner.GetSignatureInputCount();
            for (var i = 0; i < signatureInputCount; i++)
            {
                var name = runner.GetSignatureInputName(i);
                sb.AppendLine($"Signature Input [{i}]: {name},\t info: {runner.GetSignatureInputInfo(name)}");
            }

            sb.AppendLine();

            var signatureOutputCount = (int)runner.GetSignatureOutputCount();
            for (var i = 0; i < signatureOutputCount; i++)
            {
                var name = runner.GetSignatureOutputName(i);
                sb.AppendLine($"Signature Output [{i}]: {name},\t info: {runner.GetSignatureOutputInfo(name)}");
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        ///     Gets total element count in the tensor.
        /// </summary>
        /// <param name="info">A tensor info</param>
        /// <returns>The total count of the element</returns>
        public static int GetElementCount(this Interpreter.TensorInfo info)
        {
            var shape = info.shape;
            var total = 1;
            for (var i = 0; i < shape.Length; i++) total *= shape[i];

            if (total < 1) throw new NotSupportedException("Dynamic shape is not supported");
            return total;
        }
    }
}