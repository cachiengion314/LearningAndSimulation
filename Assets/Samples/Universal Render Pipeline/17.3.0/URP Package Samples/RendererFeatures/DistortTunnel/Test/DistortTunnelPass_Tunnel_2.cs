using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class DistortTunnelPass_Tunnel_2 : ScriptableRenderPass
{
  class PassData
  {
    public Renderer tunnelObject;
    public Material tunnelMaterial;
  }

  RTHandle m_OutputHandle;
  Renderer m_TunnelObject;
  int m_Slice;

  public DistortTunnelPass_Tunnel_2(RenderPassEvent evt)
  {
    renderPassEvent = evt;
  }

  void SetTunnelObject()
  {
    if (m_TunnelObject != null)
      return;
    var tunnelGO = GameObject.Find("Tunnel");
    if (tunnelGO != null)
      m_TunnelObject = tunnelGO.GetComponent<Renderer>();
  }

  public void SetRTHandles(ref RTHandle dest, int slice)
  {
    m_OutputHandle = dest;
    m_Slice = slice;
  }

  public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
  {
    var cameraData = frameData.Get<UniversalCameraData>();
    var texRefData = frameData.GetOrCreate<DistortTunnelRendererFeature_2.TexRefData2>();
    if (cameraData.camera.cameraType != CameraType.Game) return;
    SetTunnelObject();
    if (!m_TunnelObject) return;

    using var builder = renderGraph.AddRasterRenderPass<PassData>("Custom_Tunnel", out var passData);
    var destination = texRefData.distortTunnelTexHandle;
    if (!destination.IsValid()) return;

    passData.tunnelObject = m_TunnelObject;
    passData.tunnelMaterial = m_TunnelObject.sharedMaterial;
    builder.SetRenderAttachment(destination, 0, AccessFlags.Write, 0, depthSlice: m_Slice);
    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
    {
      context.cmd.DrawRenderer(data.tunnelObject, data.tunnelMaterial, 0, 0);
    });
  }
}
