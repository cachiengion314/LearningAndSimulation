using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class DistortTunnelRendererFeature_2 : ScriptableRendererFeature
{
    public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingSkybox;
    public Shader distortShader;
    private Material m_DistortMaterial;
    private RTHandle m_DistortTunnelTexHandle;
    DistortTunnelPass_CopyColor_2 m_CopyColorPass;
    DistortTunnelPass_Tunnel_2 m_TunnelPass;
    DistortTunnelPass_Distort_2 m_DistortPass;

    public class TexRefData2 : ContextItem
    {
        public TextureHandle distortTunnelTexHandle = TextureHandle.nullHandle;

        public override void Reset()
        {
            distortTunnelTexHandle = TextureHandle.nullHandle;
        }
    }


    public override void Create()
    {
        m_CopyColorPass = new DistortTunnelPass_CopyColor_2(passEvent);
        m_TunnelPass = new DistortTunnelPass_Tunnel_2(passEvent);
        m_DistortMaterial = CoreUtils.CreateEngineMaterial(distortShader);
        m_DistortPass = new DistortTunnelPass_Distort_2(m_DistortMaterial, passEvent);
    }

    // Override the AddRenderPasses method to inject passes into the renderer. Unity calls AddRenderPasses once per camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Skip rendering if the camera isn't a game camera.
        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        // Create a 2D render texture array that contains 2 slices. We directly using this method instead of rely on sub-camera writing system
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1;
        desc.dimension = TextureDimension.Tex2DArray;
        desc.volumeDepth = 2;
        RenderingUtils.ReAllocateHandleIfNeeded(ref m_DistortTunnelTexHandle, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_DistortTunnelTexture");

        m_CopyColorPass.SetRTHandles(ref m_DistortTunnelTexHandle, 0);
        m_TunnelPass.SetRTHandles(ref m_DistortTunnelTexHandle, 1);
        m_DistortPass.SetRTHandles(ref m_DistortTunnelTexHandle);

        renderer.EnqueuePass(m_CopyColorPass);
        renderer.EnqueuePass(m_TunnelPass);
        renderer.EnqueuePass(m_DistortPass);
    }

    // Free the resources the Scriptable Renderer Feature uses.
    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_DistortMaterial);
        m_CopyColorPass = null;
        m_TunnelPass = null;
        m_DistortPass = null;
        m_DistortTunnelTexHandle?.Release();
    }
}
