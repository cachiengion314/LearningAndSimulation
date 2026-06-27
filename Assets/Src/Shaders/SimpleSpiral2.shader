Shader "Custom/SimpleSpiral2"
{
  Properties
  {
    _Color ("Spiral Color", Color) = (1, 0.5, 0, 1)
    _Background ("Background Color", Color) = (0, 0, 0, 1)
    _A ("Spiral A", Range(0, 5)) = 0
    _B ("Spiral B", Range(.5, 5)) = 1
    _Thickness ("Spiral Thickness", Range(.1, 5)) = 1
    _OffsetAngle ("Offset Angle", Range(-100, 0)) = -100.0
  }

  SubShader
  {
    Tags { "RenderPipeline" = "UniversalRenderPipeline" "RenderType" = "Opaque" }

    Pass
    {
      Name "SpiralPass"
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

      struct v2f
      {
        float4 positionHCS : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float4 _Background;
        float _A;
        float _B;
        float _Thickness;
        float _OffsetAngle;
      CBUFFER_END

      v2f vert(Attributes IN)
      {
        v2f OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        OUT.uv = IN.uv;
        return OUT;
      }

      float4 frag(v2f IN) : SV_Target
      {
        float2 uv = IN.uv * 20 - 10;
        float r = length(uv);
        float theta = atan2(uv.y, uv.x);

        // Ensure theta is positive (0 to 2PI)
        if (theta < 0) theta += TWO_PI;

        // Animate spiral (offset angle)
        float expectedTheta = theta + _OffsetAngle;
        // Spiral radius at this theta
        float expectedR = _A + _B * expectedTheta;
        while (r > expectedR)
        {
          expectedTheta += TWO_PI;
          expectedR = _A + _B * expectedTheta;
        }

        // Draw pixels near the spiral
        if (abs(r - expectedR) < _Thickness)
          return _Color;
        else
          return _Background;
      }

      ENDHLSL
    }
  }
  FallBack Off
}
