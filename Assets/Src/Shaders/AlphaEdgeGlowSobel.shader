Shader "Custom/AlphaEdgeGlowSobel"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" { }
    _GlowColor ("Glow Color", Color) = (1, 0, 0, 1)
    _GlowWidth ("Glow Width", Range(0.001, 0.05)) = 0.01
  }

  SubShader
  {
    Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" }
    Blend SrcAlpha OneMinusSrcAlpha
    ZWrite Off
    Cull Off

    Pass
    {
      Name "AlphaEdgeGlowSobel"
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

      struct Varyings
      {
        float4 positionHCS : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      TEXTURE2D(_MainTex);
      SAMPLER(sampler_MainTex);

      float4 _GlowColor;
      float _GlowWidth;

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
        float2 offset = float2(_GlowWidth, _GlowWidth);

        // 3x3 neighborhood alpha values
        float a00 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset * float2(-1, -1)).a;
        float a01 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset * float2(-1, 0)).a;
        float a02 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset * float2(-1, 1)).a;
        float a10 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset * float2(0, -1)).a;
        float a11 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
        float a12 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset * float2(0, 1)).a;
        float a20 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset * float2(1, -1)).a;
        float a21 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset * float2(1, 0)).a;
        float a22 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset * float2(1, 1)).a;

        // Sobel edge detection
        float gx = a00 * - 1 + a02 * 1 + a10 * - 2 + a12 * 2 + a20 * - 1 + a22 * 1;
        float gy = a00 * - 1 + a20 * 1 + a01 * - 2 + a21 * 2 + a02 * - 1 + a22 * 1;

        float edge = sqrt(gx * gx + gy * gy);

        // Glow intensity (scale up and saturate)
        float glow = saturate(edge * 5);

        float4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
        float3 finalColor = baseCol.rgb + _GlowColor.rgb * glow;

        return float4(finalColor, baseCol.a);
      }
      ENDHLSL
    }
  }

  FallBack Off
}
