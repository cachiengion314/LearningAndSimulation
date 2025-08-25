Shader "Custom/WaveFunction"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" { }
    _t ("Time speed", Range(0, 1)) = 0
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }
    Pass
    {
      Name "That khong the tin noi"
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
      CBUFFER_START(UnityPerMaterial)
        float _t;
      CBUFFER_END
      float4 _MainTex_ST;

      Varyings vert(Attributes v)
      {
        Varyings Out;
        Out.positionHCS = TransformObjectToHClip(v.positionOS);
        Out.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return Out;
      }

      float2 SinWave(float2 uv)
      {
        float r = 1 - length(uv);
        float T1 = 1 - _t;
        float rt = 22 * (r - T1);
        rt = clamp(rt, 0, 2 * PI);
        
        float w = .1 * sin(rt) * T1;

        return uv + w;
        
        // float2 uv = In;
        // float r = 1 - length(uv);
        // float one_minus_time = 1 - frac(Time * TimeSpeed);
        // float E = 13 * (r - (one_minus_time)); // extension equation
        // float waveInput = clamp(E, 0, 2 * customPI);
        // float wave = sin(waveInput) * one_minus_time * .1;
        // OUT = uv - wave;

      }

      float4 frag(Varyings In) : SV_Target
      {
        float2 uv = In.uv * 2.0 - 1.0;
        float2 sinwave = SinWave(uv);

        float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sinwave);
        
        return texColor;
      }

      ENDHLSL
    }
  }
}
