Shader "Custom/Twirl"
{
  Properties
  {
    _Color ("Color Tint", Color) = (1, 1, 1, 1)
    _MainTex ("Main Texture", 2D) = "white" { }
    _Center ("Center", Vector) = (0.5, 0.5, 0.0, 0.0)
    _Offset ("Offset", Vector) = (0.0, 0.0, 0.0, 0.0)
    _Strength ("Strength", Range(-55.2, 55.2)) = 0.0
  }

  SubShader
  {
    Tags { "RenderPipeline" = "UniversalRenderPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

    Pass
    {
      Name "ForwardLit"
      Tags { "LightMode" = "UniversalForward" }

      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

      #ifndef UNITY_PI
        #define UNITY_PI 3.14159265359
      #endif

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

      TEXTURE2D(_MainTex);
      SAMPLER(sampler_MainTex);

      CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float2 _Center;
        float2 _Offset;
        float _Strength;
      CBUFFER_END

      v2f vert(Attributes IN)
      {
        v2f OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
        OUT.uv = IN.uv;
        return OUT;
      }

      float4 frag(v2f IN) : SV_Target
      {
        float2 uv = IN.uv;
        float2 delta = uv - _Center;
        float theta = _Strength * length(delta);
        float x = cos(theta) * delta.x - sin(theta) * delta.y;
        float y = sin(theta) * delta.x + cos(theta) * delta.y;
        float2 UV = float2(x + _Center.x + _Offset.x, y + _Center.y + _Offset.y);

        float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, UV);
        float3 blendColor = texColor.rgb * _Color.rgb;
        return float4(blendColor.rgb, texColor.a * _Color.a);
      }
      ENDHLSL
    }
  }

  FallBack Off
}
