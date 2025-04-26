using UnityEngine;
using UnityEngine.UI;

namespace LeTai.Asset.TranslucentImage.Demo
{
    public class MainDemoViewController : MonoBehaviour
    {
        public Toggle toggleLightMode;
        public Toggle toggleDarkMode;

        public Slider sliderBlurStrength;
        public Slider sliderVibrancy;
        public Slider sliderUpdateRate;

        public TranslucentImage[] translucentImages;

        private float backupBlurStrength;
        private float[] backupVibrancy;

        private TranslucentImageSource source;

        private void Start()
        {
            source = Shims.FindObjectOfType<TranslucentImageSource>();
            var colorSchemeManager = GetComponent<ColorSchemeManager>();

            BackupValues();

            toggleLightMode.onValueChanged.AddListener(isOn =>
            {
                if (isOn) colorSchemeManager.SetColorScheme(ColorSchemeManager.DemoColorScheme.Light);
            });
            toggleDarkMode.onValueChanged.AddListener(isOn =>
            {
                if (isOn) colorSchemeManager.SetColorScheme(ColorSchemeManager.DemoColorScheme.Dark);
            });

            sliderBlurStrength.onValueChanged.AddListener(value => { source.BlurConfig.Strength = value; });
            sliderVibrancy.onValueChanged.AddListener(value =>
            {
                for (var i = 0; i < translucentImages.Length; i++)
                    translucentImages[i].materialForRendering.SetFloat(ShaderID.VIBRANCY, value);
            });
            sliderUpdateRate.onValueChanged.AddListener(value =>
            {
                source.MaxUpdateRate = Mathf.Approximately(value, sliderUpdateRate.maxValue)
                    ? float.PositiveInfinity
                    : value;
            });
        }

        private void OnDestroy()
        {
            source.BlurConfig.Strength = backupBlurStrength;
            for (var i = 0; i < translucentImages.Length; i++)
                translucentImages[i].materialForRendering.SetFloat(ShaderID.VIBRANCY, backupVibrancy[i]);
        }

        private void BackupValues()
        {
            backupBlurStrength = source.BlurConfig.Strength;
            backupVibrancy = new float[translucentImages.Length];
            for (var i = 0; i < translucentImages.Length; i++)
                backupVibrancy[i] = translucentImages[i].materialForRendering.GetFloat(ShaderID.VIBRANCY);
        }
    }
}