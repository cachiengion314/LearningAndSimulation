Shader "Custom/TransparentUVEdgeGlow"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" { }
    _GlowColor ("Glow Color", Color) = (1, 0, 0, 1)
    _GlowWidth ("Glow Width", Float) = 0.05
  }

  SubShader
  {
    Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" }
    Blend SrcAlpha OneMinusSrcAlpha
    ZWrite Off
    Cull Off

    Pass
    {
      Name "EdgeGlowPass"
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
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
        OUT.uv = IN.uv;
        return OUT;
      }

      float4 frag(Varyings IN) : SV_Target
      {
        float2 uv = IN.uv;

        // Distance to nearest UV edge (good for 0–1 UVs)
        float edgeDist = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));

        // Edge-based glow
        float glowFactor = smoothstep(_GlowWidth, 0.0, edgeDist);

        // Sample main texture
        float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

        // Apply glow
        float4 glow = _GlowColor * glowFactor;

        // Final color with transparency
        float4 finalColor = texColor + glow;

        // Respect original texture's alpha
        finalColor.a = texColor.a;

        return finalColor;
      }
      ENDHLSL
    }
  }

  // FallBack "Hidden/Shader Graph/FallbackError"

}
