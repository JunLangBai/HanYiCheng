using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BlurEffec : MonoBehaviour
{
    [Range(1, 4)] public int iterations = 3;

    [Range(0.2f, 0.3f)] public float blurSpread = 0.6f;

    [Range(1, 8)] public int downSample = 2;

    private Material material;

    private void Awake()
    {
        material = new Material(Shader.Find("AZ/BlurEffect"));
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        var rtW = src.width / downSample;
        var rtH = src.height / downSample;

        var buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
        buffer0.filterMode = FilterMode.Bilinear;
        Graphics.Blit(src, buffer0);

        var buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

        for (var i = 0; i < iterations; i++)
        {
            material.SetFloat("_BlurSize", 1.0f + i * blurSpread);

            Graphics.Blit(buffer0, buffer1, material, 0);

            Graphics.Blit(buffer1, buffer0, material, 1);
        }

        Graphics.Blit(buffer0, dest);
        RenderTexture.ReleaseTemporary(buffer0);
        RenderTexture.ReleaseTemporary(buffer1);
    }
}