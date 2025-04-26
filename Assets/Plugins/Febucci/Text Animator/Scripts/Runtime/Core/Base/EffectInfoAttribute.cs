using System;

namespace Febucci.UI.Core
{
    /// <summary>
    ///     Attribute used to set effect settings
    /// </summary>
    /// <example>
    ///     In order to set a "jump" tag to the below class:
    ///     <code>
    /// [EffectInfo(tag: "jump")]
    /// public class JumpEffect : BehaviorEffect
    /// {
    /// ///[...]
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class EffectInfoAttribute : Attribute
    {
        public readonly string tag;

        public EffectInfoAttribute(string tag)
        {
            this.tag = tag;
        }
    }
}