Shader "Custom/SimpleSpiral"
{
  Properties
  {
    _Color ("Color Tint", Color) = (1, 1, 1, 1)
    _MainTex ("Main Texture", 2D) = "white" { }
    _SpiralColor ("Spiral Color", Color) = (1, 1, 0, 1)
    _SpiralTightness ("Spiral Tightness", Range(-15.2, 15.2)) = 5.0
    _SpiralThickness ("Spiral Line Thickness", Range(0.000, 0.7)) = 0.05
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
        float4 _SpiralColor;
        float _SpiralTightness;
        float _SpiralThickness;
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
        // olright let create a polar coordinate all by myself... soft of
        // first of all we have to take IN.uv
        float2 uv = IN.uv * 2.0 - 1.0; // unified mathematic operator
        float r = length(uv); // the length from 0 to the pixel itself
        float theta = atan2(uv.y, uv.x); // the angles created by position of the pixel and Ox axis
        
        // we will apply this equation to form a spiral shape: P = θ + r*s
        float P = theta + r * _SpiralTightness;
        // P += _Time.y;
        P = frac(P / (2 * UNITY_PI)); // the pixel will have the value between [0, 1]. It will create a repeating pattern with each 2π length
        // the theta will ensure the offset that can make each of angles will eventually gain a little bit of value
        P = smoothstep(_SpiralThickness, 0, abs(P - .5));
        float2 UV = IN.uv + P;
        float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, UV);
        float3 blendColor = lerp(texColor.rgb, _SpiralColor.rgb, P);
        return float4(blendColor.rgb * _Color.rgb, _SpiralColor.a);
      }
      ENDHLSL
    }
  }

  FallBack Off
}
