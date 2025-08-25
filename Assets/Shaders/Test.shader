Shader "Custom/KelvinWake"
{
  Properties
  {
    _Speed("Speed", Float) = 1.0
    _TimeScale("Time Scale", Float) = 1.0
    _Amplitude("Amplitude", Float) = 0.1
    _Frequency("Frequency", Float) = 20.0
    _Sharpness("Sharpness", Float) = 4.0
    _MainTex("MainTex", 2D) = "white" {}
  }
  SubShader
  {
    Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }
    LOD 100

    Pass
    {
      Name "KelvinWake"
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

      sampler2D _MainTex;
      float _Speed;
      float _TimeScale;
      float _Amplitude;
      float _Frequency;
      float _Sharpness;

      Varyings vert(Attributes IN)
      {
        Varyings OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
        OUT.uv = IN.uv;
        return OUT;
      }

      float kelvinWake(float2 uv, float time)
      {
        float2 center = float2(0.5, 0.5); // wake origin
        float2 p = uv - center;
        float r = length(p);
        float theta = atan2(p.y, p.x);

        float sum = 0.0;
        const int steps = 64;
        for (int i = 0; i < steps; i ++)
        {
          float phi = lerp(- PI / 2.0, PI / 2.0, i / (steps - 1.0));
          float cosTerm = cos(_Frequency * r * cos(phi - theta) - time * _Speed);
          float weight = 1.0 / pow(cos(phi) + 0.0001, _Sharpness);
          sum += cosTerm * weight;
        }

        return sum / steps;
      }

      half4 frag(Varyings IN) : SV_Target
      {
        float time = _Time.y * _TimeScale;
        float z = kelvinWake(IN.uv, time);
        float brightness = saturate(0.5 + _Amplitude * z);

        return float4(brightness, brightness, brightness, 1.0);
      }
      ENDHLSL
    }
  }
}
