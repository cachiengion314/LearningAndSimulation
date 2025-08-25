Shader "Custom/ObjectBlur_SampleTex2D"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" { }
    _BlurStrength ("Blur Strength", Float) = 1.0
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }
    Pass
    {
      Name "BlurPass"
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
      float4 _MainTex_ST;
      float _BlurStrength;

      Varyings vert(Attributes v)
      {
        Varyings o;
        o.positionHCS = TransformObjectToHClip(v.positionOS);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
      }

      float4 frag(Varyings i) : SV_Target
      {
        float2 dx = ddx(i.uv) * _BlurStrength;
        float2 dy = ddy(i.uv) * _BlurStrength;

        float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
        col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + dx);
        col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - dx);
        col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + dy);
        col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - dy);

        return col / 5.0;
      }

      ENDHLSL
    }
  }
}
