namespace Febucci.UI.Core
{
    internal interface EffectEvaluator
    {
        bool isEnabled { get; }
        void Initialize(int type);
        float Evaluate(float time, int characterIndex);
        float GetDuration();
    }
}