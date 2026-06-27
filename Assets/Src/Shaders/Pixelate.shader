Shader "Custom/URP_Pixelate_Modern"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" { }
    _BlockSize ("Block Size (Screen Pixels)", Range(1, 128)) = 8
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" }
    LOD 100

    Pass
    {
      Name "PixelatePass"
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
      float4 _MainTex_ST;
      float _BlockSize;

      Varyings vert(Attributes IN)
      {
        Varyings OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
        OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
        return OUT;
      }

      float2 GetBlockUV(float2 uv)
      {
        // when zoom out, the rate of change in the region of area under this pixel
        // will sure bigger therefore ddx(uv) will be bigger and make dx increase its length
        // ==> the final pixel size will look bigger in general
        //
        float2 dx = ddx(uv);
        float2 dy = ddy(uv);
        float2 uvPixelSize = float2(length(dx), length(dy));
        float2 blockSizeUV = uvPixelSize * _BlockSize;
        return floor(uv / blockSizeUV) * blockSizeUV;
      }

      float4 frag(Varyings IN) : SV_Target
      {
        float2 snappedUV = GetBlockUV(IN.uv);
        return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, snappedUV);
      }

      ENDHLSL
    }
  }
}
