using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// This pass takes the outputs from the CopyColor pass and the Tunnel pass as inputs and blits them to the screen with a Material that creates the distortion effect.
public class DistortTunnelPass_Distort : ScriptableRenderPass
{
    private readonly Material m_Material;
    private RTHandle m_OutputHandle;
    private readonly ProfilingSampler m_ProfilingSampler = new("DistortTunnelPass_Distort");

    public DistortTunnelPass_Distort(Material mat, RenderPassEvent evt)
    {
        renderPassEvent = evt;
        m_Material = mat;
    }

    public void SetRTHandles(ref RTHandle copyColorRT, ref RTHandle tunnelRT, RTHandle dest)
    {
        if (m_Material == null)
            return;

        m_OutputHandle = dest;
        m_Material.SetTexture(copyColorRT.name, copyColorRT);
        m_Material.SetTexture(tunnelRT.name, tunnelRT);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureTarget(m_OutputHandle);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cameraData = renderingData.cameraData;
        if (cameraData.camera.cameraType != CameraType.Game)
            return;

        if (m_Material == null)
            return;

        var cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            Blitter.BlitCameraTexture(cmd, m_OutputHandle, m_OutputHandle, m_Material, 0);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }
}