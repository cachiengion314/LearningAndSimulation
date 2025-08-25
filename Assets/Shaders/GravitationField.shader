Shader "Custom/GravitationField"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
    _p ("Position", Vector) = (5, 5, 0.0, 0.0)
    _k ("Twist Strength", Range(0, 10)) = 1.0
  }
  SubShader
  {
    Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
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

      struct Varyings
      {
        float4 positionHCS : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      TEXTURE2D(_MainTex);
      SAMPLER(sampler_MainTex);
      float _k;
      float2 _p;

      Varyings vert(Attributes IN)
      {
        Varyings OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
        OUT.uv = IN.uv;
        return OUT;
      }

      half4 frag(Varyings IN) : SV_Target
      {
        float2 uv = IN.uv * 10;
        float2 centered = uv - _p;
        float x = centered.x;
        float y = centered.y;
        float r = _k * length(centered) / pow(length(centered), 2);
        float v = min(r, 2 * PI);

        float X = 1 * (x * cos(v) - y * sin(v));
        float Y = 1 * (x * sin(v) + y * cos(v));
        float2 warpedUV = float2(X + _p.x, Y + _p.y);

        return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, warpedUV);
      }
      ENDHLSL
    }
  }
}
