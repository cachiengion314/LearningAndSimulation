Shader "Custom/TransparentUVEdgeGlow"
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
      Name "AlphaEdgeGlow"
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

        // Sample base alpha
        float alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;

        // Sample surrounding alphas to detect edges
        float2 offset = ddx(uv) * _GlowWidth * 500;
        float aDiag = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset).a;
        // float aX = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(_GlowWidth, 0)).a;
        // float aY = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, _GlowWidth)).a;

        float diff = abs(aDiag - alpha);
        float glow = saturate(diff * 1);

        float3 glowColor = _GlowColor.rgb * glow;

        float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
        float alphaOut = texColor.a;

        return float4(texColor.rgb + glowColor, alphaOut);
      }
      ENDHLSL
    }
  }

  FallBack Off
}
