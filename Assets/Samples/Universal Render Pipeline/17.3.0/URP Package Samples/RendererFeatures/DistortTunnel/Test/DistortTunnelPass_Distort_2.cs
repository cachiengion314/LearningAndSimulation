using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class DistortTunnelPass_Distort_2 : ScriptableRenderPass
{
  Material m_Material;
  RTHandle m_DistortTunnelTexHandle;

  public DistortTunnelPass_Distort_2(Material mat, RenderPassEvent evt)
  {
    renderPassEvent = evt;
    m_Material = mat;
  }

  public void SetRTHandles(ref RTHandle srcRT)
  {
    m_DistortTunnelTexHandle = srcRT;
  }


  public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
  {
    var resourceData = frameData.Get<UniversalResourceData>();
    var cameraData = frameData.Get<UniversalCameraData>();
    var texRefData = frameData.Get<DistortTunnelRendererFeature_2.TexRefData2>();
    if (cameraData.camera.cameraType != CameraType.Game) return;
    if (m_Material == null) return;

    var destination = resourceData.activeColorTexture;
    var source = texRefData.distortTunnelTexHandle;
    if (!source.IsValid() || !destination.IsValid()) return;

    var para = new RenderGraphUtils.BlitMaterialParameters(source, destination, m_Material, 0);
    renderGraph.AddBlitPass(para, "Custom_Distort");
  }
}
