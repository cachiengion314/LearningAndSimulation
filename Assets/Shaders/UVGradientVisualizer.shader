Shader "Custom/UVGradientVisualizer"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" { }
    _Scale ("Gradient Scale", Range(1, 200)) = 100.0
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" }
    LOD 100

    Pass
    {
      Name "Unlit"
      Tags { "LightMode" = "UniversalForward" }

      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

      // Properties
      TEXTURE2D(_MainTex);
      SAMPLER(sampler_MainTex);
      float _Scale;

      struct Attributes
      {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct Varyings
      {
        float4 positionHCS : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      Varyings vert(Attributes IN)
      {
        Varyings OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        OUT.uv = IN.uv;
        return OUT;
      }

      float4 frag(Varyings IN) : SV_Target
      {
        float2 uv = IN.uv;

        // Screen-space partial derivatives
        float2 gradX = ddx(uv);
        float2 gradY = ddy(uv);

        // Gradient magnitude
        float gradMag = length(gradX) + length(gradY);
        float strength = saturate(gradMag * _Scale);

        // Heatmap visualization: blue (low) to red (high)
        float3 color = lerp(float3(0.0, 0.0, 1.0), float3(1.0, 0.0, 0.0), strength);
        return float4(color, 1.0);
      }

      ENDHLSL
    }
  }

  FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
