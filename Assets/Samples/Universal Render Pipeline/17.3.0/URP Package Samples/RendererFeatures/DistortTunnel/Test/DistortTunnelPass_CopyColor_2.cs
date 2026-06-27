using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class DistortTunnelPass_CopyColor_2 : ScriptableRenderPass
{
  RTHandle m_OutputHandle;
  int m_Slice;
  Material m_Material;

  public DistortTunnelPass_CopyColor_2(RenderPassEvent evt)
  {
    renderPassEvent = evt;
  }

  public void SetRTHandles(ref RTHandle destTexture, int slice)
  {
    m_OutputHandle = destTexture;
    m_Slice = slice;
    m_Material = Blitter.GetBlitMaterial(TextureDimension.Tex2DArray);
  }

  public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
  {
    var resourceData = frameData.Get<UniversalResourceData>();
    var cameraData = frameData.Get<UniversalCameraData>();
    var texRefData = frameData.GetOrCreate<DistortTunnelRendererFeature_2.TexRefData2>();
    if (cameraData.camera.cameraType != CameraType.Game) return;

    var destination = renderGraph.ImportTexture(m_OutputHandle);
    texRefData.distortTunnelTexHandle = destination;
    var source = resourceData.activeColorTexture;
    if (!source.IsValid() || !destination.IsValid()) return;

    // perform a dead simple blit operation into TextureHandle therefore affect to original RenderTexture
    var para = new RenderGraphUtils.BlitMaterialParameters(source, destination, m_Material, 0)
    {
      destinationSlice = m_Slice
    };
    renderGraph.AddBlitPass(para, "Custom_CopyColor");
  }
}
