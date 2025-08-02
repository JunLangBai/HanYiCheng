// UIManager.cs

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlowWindows : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private CanvasGroup settingsPanel;

    [SerializeField] private float fadeDuration = 0.3f;

    private Coroutine currentFade;
    private Image image;
    private readonly Color normalColor = new(0.372549f, 0.372549f, 0.372549f, 1f);
    private readonly Color pressedColor = new(0f, 0f, 0f, 1f);

    private void Start()
    {
        image = GetComponent<Image>();
        settingsPanel.alpha = 0;
        settingsPanel.interactable = false;
        settingsPanel.blocksRaycasts = false;
    }

    public void ToggleSettingsUI()
    {
        if (settingsPanel.interactable)
            image.color = normalColor;
        else if (settingsPanel.interactable == false) image.color = pressedColor;
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadePanel(!settingsPanel.interactable));
    }

    private IEnumerator FadePanel(bool show)
    {
        var startAlpha = settingsPanel.alpha;
        float targetAlpha = show ? 1 : 0;
        float elapsed = 0;

        settingsPanel.interactable = show;
        settingsPanel.blocksRaycasts = show;

        while (elapsed < fadeDuration)
        {
            settingsPanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // 确保渐变结束时精确设置目标透明度
        settingsPanel.alpha = targetAlpha;
    }
}