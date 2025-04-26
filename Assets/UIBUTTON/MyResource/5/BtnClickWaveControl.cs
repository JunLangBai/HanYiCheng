using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BtnClickWaveControl : MonoBehaviour
{
    [SerializeField] private float waveDuration = 2f;

    // SDF
    [SerializeField] private bool useSDF;
    private Button btn;
    private Material material;
    private Vector2 mouseUVPos;
    private Sequence sequence;
    private Vector4 targetColor;
    private Vector4 tmp_color;

    private void Start()
    {
        material = GetComponent<Image>().material = new Material(GetComponent<Image>().material);
        btn = GetComponent<Button>();
        var rect = GetComponent<RectTransform>();

        material.SetFloat("_CircleScaleOffset", rect.sizeDelta.x / rect.sizeDelta.y);
        material.SetFloat("_Radius", -material.GetFloat("_RadiusThickness"));
        if (useSDF)
        {
            tmp_color = material.GetColor("_WaveColor");
            targetColor = new Vector4(tmp_color.x, tmp_color.y, tmp_color.z, 0);
        }

        btn.onClick.AddListener(() =>
        {
            Vector2 uipos = Vector3.one;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform,
                Input.mousePosition, Camera.main, out uipos);
            mouseUVPos = (uipos - (Vector2)rect.localPosition) / rect.sizeDelta;
            if (!useSDF)
            {
                material.SetVector("_MouseUVPos", mouseUVPos);

                sequence?.Kill();
                sequence = DOTween.Sequence().SetUpdate(true).SetId(transform)
                    .AppendCallback(() => { material.SetFloat("_Radius", -material.GetFloat("_RadiusThickness")); })
                    .Append(material.DOFloat(10f, "_Radius", waveDuration));
            }
            else
            {
                material.SetVector("_CirclePosOffset", mouseUVPos);

                sequence?.Kill();
                sequence = DOTween.Sequence().SetUpdate(true).SetId(transform)
                    .AppendCallback(() =>
                    {
                        material.SetFloat("_Radius", -material.GetFloat("_RadiusThickness"));
                        material.SetColor("_WaveColor", tmp_color);
                    })
                    .Append(material.DOFloat(6f, "_Radius", waveDuration))
                    .Join(material.DOColor(targetColor, "_WaveColor", waveDuration));
            }
        });
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);
    }
}