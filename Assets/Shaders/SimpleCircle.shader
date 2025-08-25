Shader "Custom/SimpleCircle"
{
  Properties
  {
    _Color ("Color", Color) = (0, 0.6, 1, 1)
    _Radius ("Circle Radius", Range(0, 1)) = 0.4
  }

  SubShader
  {
    Tags { "RenderPipeline" = "UniversalRenderPipeline" "RenderType" = "Opaque" }
    LOD 100

    Pass
    {
      Name "ForwardLit"
      Tags { "LightMode" = "UniversalForward" }

      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

      struct Attributes
      {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float4 positionHCS : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float _Radius;
      CBUFFER_END

      v2f vert(Attributes IN)
      {
        v2f OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        OUT.uv = IN.uv;
        return OUT;
      }

      half4 frag(v2f IN) : SV_Target
      {
        // Centered UV (- 1 to 1)
        float2 centeredUV = IN.uv * 2.0 - 1.0;

        // Polar coordinates
        float r = length(centeredUV);
        float theta = atan2(centeredUV.y, centeredUV.x); // not strictly needed here

        // Fill pixels inside radius
        if (r < _Radius)return _Color;
        return float4(0, 0, 0, 1); // transparent (or black background)

      }

      ENDHLSL
    }
  }

  FallBack Off
}
