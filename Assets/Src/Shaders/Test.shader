Shader "Custom/SpriteOutline"
{
  Properties
  {
    _MainTex ("Sprite Texture", 2D) = "white" {}
    _Color ("Tint", Color) = (1, 1, 1, 1)
    _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
    _OutlineThickness ("Outline Thickness (pixels)", Float) = 1.0
  }

  SubShader
  {
    Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" }
    Blend SrcAlpha OneMinusSrcAlpha
    Cull Off
    ZWrite Off

    Pass
    {
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
      float4 _MainTex_TexelSize; // (1 / width, 1 / height, width, height)

      float4 _Color;
      float4 _OutlineColor;
      float _OutlineThickness;

      Varyings vert (Attributes IN)
      {
        Varyings OUT;
        OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        OUT.uv = IN.uv;
        return OUT;
      }

      half4 frag (Varyings IN) : SV_Target
      {
        float2 uv = IN.uv;
        float alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;

        // texel step for neighbor sampling
        float2 texel = _MainTex_TexelSize.xy * _OutlineThickness;

        // sample 8 neighbors
        float a1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(texel.x, 0)).a;
        float a2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(texel.x, 0)).a;
        float a3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, texel.y)).a;
        float a4 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(0, texel.y)).a;
        float a5 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(texel.x, texel.y)).a;
        float a6 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(- texel.x, texel.y)).a;
        float a7 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(texel.x, - texel.y)).a;
        float a8 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(- texel.x, - texel.y)).a;

        // max of neighbors
        float maxNeighbor = max(a1, max(a2, max(a3, max(a4, max(a5, max(a6, max(a7, a8)))))));

        // outline mask = neighbor filled but current empty
        float outline = saturate(maxNeighbor - alpha);

        // sprite color
        float4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Color;

        // final blend : outline where needed, else sprite
        float4 finalCol = lerp(baseCol, _OutlineColor, outline);

        // keep alpha correct (show sprite + outline)
        finalCol.a = max(baseCol.a, outline * _OutlineColor.a);

        return finalCol;
      }
      ENDHLSL
    }
  }
}
