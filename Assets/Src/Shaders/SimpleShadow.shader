Shader "Custom/SimpleShadow"
{
  Properties
  {
    _Color ("Color", Color) = (1, 0, 0, 1)
    _MainTex ("Main Texture", 2D) = "white" { }
  }

  SubShader
  {
    Tags { "RenderPipeline" = "UniversalRenderPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }
    LOD 100

    Pass
    {
      Name "ForwardLit"
      Tags { "LightMode" = "UniversalForward" }

      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      // Generates two shader variants : one with main light shadows disabled and one with them enabled.
      #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
      // This handles cascaded shadows, which improve shadow quality over distances.
      #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl" // <-- Add this

      struct Attributes
      {
        float4 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float4 positionHCS : SV_POSITION;
        float3 normalWS : TEXCOORD0;
        float2 uv : TEXCOORD1;
        float4 shadowCoord : TEXCOORD2; // ✅ Proper shadow coordinates

      };


      TEXTURE2D(_MainTex);
      SAMPLER(sampler_MainTex);

      CBUFFER_START(UnityPerMaterial)
        float4 _Color;
      CBUFFER_END

      v2f vert(Attributes IN)
      {
        v2f OUT;
        OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
        float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
        OUT.shadowCoord = TransformWorldToShadowCoord(positionWS); // ✅

        OUT.positionHCS = TransformWorldToHClip(positionWS);
        OUT.uv = IN.uv;

        return OUT;
      }

      half4 frag(v2f IN) : SV_Target
      {
        float3 normal = normalize(IN.normalWS);

        Light mainLight = GetMainLight(IN.shadowCoord);
        // Shadow attenuation (0 in shadow, 1 in light)
        float shadowAttenuation = mainLight.shadowAttenuation;
        float ambientFactor = 0.2; // Adjust this value as needed
        shadowAttenuation = shadowAttenuation > 0.5 ? 1.0 : 0.3;

        float NdotL = saturate(dot(normal, mainLight.direction));

        // Toon step logic
        float toonShade;
        if (NdotL > 0.66) toonShade = 1.0;
        else if (NdotL > 0.33) toonShade = 0.6;
        else toonShade = 0.3;

        toonShade = min(toonShade, shadowAttenuation); // Apply shadows

        float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
        float3 shadowTint = float3(0.5, 0.5, 0.8); // bluish shadow

        float3 litColor = texColor.rgb * _Color.rgb * mainLight.color * toonShade;

        return half4(litColor, texColor.a * _Color.a);
      }
      ENDHLSL
    }

    Pass
    {
      Name "ShadowCaster"
      Tags { "LightMode" = "ShadowCaster" }

      HLSLPROGRAM
      #pragma vertex ShadowPassVertex
      #pragma fragment ShadowPassFragment
      #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
      ENDHLSL
    }
  }

  FallBack Off
}
