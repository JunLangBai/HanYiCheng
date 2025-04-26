using UnityEngine;
using UnityEngine.Rendering.Universal;

// This Renderer Feature sets up the BlitToRTHandlePass pass and assigns the camera color texture as the input for the pass.
public class BlitToRTHandleRendererFeature : ScriptableRendererFeature
{
    public Material blitMaterial;
    private readonly RenderPassEvent m_CopyColorEvent = RenderPassEvent.AfterRenderingTransparents;
    private BlitToRTHandlePass m_CopyColorPass;

    public override void Create()
    {
        m_CopyColorPass = new BlitToRTHandlePass(m_CopyColorEvent, blitMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        renderer.EnqueuePass(m_CopyColorPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        m_CopyColorPass.SetInput(renderer.cameraColorTargetHandle);
    }

    protected override void Dispose(bool disposing)
    {
        m_CopyColorPass?.Dispose();
        m_CopyColorPass = null;
    }
}