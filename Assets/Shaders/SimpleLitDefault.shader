Shader "Custom/SimpleLitDefault"
{
  Properties
  {
    _Color ("Color", Color) = (1, 0, 0, 1)
    _MainTex ("Main Texture", 2D) = "white" { }
  }

  SubShader
  {
    Tags { "RenderPipeline" = "UniversalRenderPipeline" "RenderType" = "Opaque" }
    LOD 100

    Pass
    {
      Name "ForwardLit"
      Tags { "LightMode" = "UniversalForward" }

      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

      struct Attributes
      {
        float4 positionOS : POSITION; // Object Space Position
        float3 normalOS : NORMAL; // Object Space Normal
        float2 uv : TEXCOORD0; // Add this line

      };

      struct v2f
      {
        float4 positionHCS : SV_POSITION; // Homogeneous Clip Space Position (for screen rendering)
        float3 normalWS : TEXCOORD0; // Texture Coordinates (if passed through)
        float2 uv : TEXCOORD1; // Add this line

      };

      TEXTURE2D(_MainTex);
      SAMPLER(sampler_MainTex);

      // The Constant Buffer (UnityPerMaterial) : This is the ingredient list for that specific cake.
      // You (the CPU) tell the baker (the GPU) to use these specific ingredients
      // (e.g., 2 cups of sugar for _Color.x, 1 cup of flour for _Color.y, etc.) for this particular batch of cakes.
      // The baker (GPU) then gets those ingredients from the provided list and uses them while following the recipe.
      // -- -- -- -- -- -- -- -- --
      // At a fundamental level in graphics programming (especially with DirectX 11 +, OpenGL 4.x +, Vulkan, Metal),
      // data is passed from the CPU to the GPU in highly optimized memory blocks called Constant Buffers.
      // Efficiency : Instead of sending individual uniform variables one by one (which is inefficient),
      // constant buffers allow you to group many uniform variables into a single, contiguous chunk of memory
      // that can be uploaded to the GPU in one go. The GPU can then access these variables very quickly.
      // -- -- -- -- -- -- -- -- --
      // Unity uses several predefined constant buffers to group shader properties based on how frequently they change :
      // UnityPerDraw : For variables that change per draw call (e.g., the object's model matrix, per - object light probes).
      // UnityPerFrame : For variables that change per frame (e.g., time, camera position, global ambient light).
      // UnityPerMaterial : For variables that change per material (like _Color, _MainTex_ST, _Metallic, _Smoothness, etc.).
      // These are the properties you define in your shader's Properties block and set in the Material Inspector.
      // UnityPerPass : For variables that change per rendering pass (e.g., projection matrices for shadow maps,
      // light parameters for specific passes).
      // so these line of code are effectively telling the HLSL compiler (and Unity's rendering pipeline) :
      // "Create a constant buffer named UnityPerMaterial. Inside this buffer, there's a float4 variable called _Color.
      // When this shader runs, Unity will automatically put the value of the _Color property
      // from the currently active Material into this _Color variable within the UnityPerMaterial buffer on the GPU."
      CBUFFER_START(UnityPerMaterial)
        float4 _Color;
      CBUFFER_END

      v2f vert(Attributes IN)
      {
        v2f OUT;
        // Its purpose is to transform a normal vector from object space to world space.
        float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
        float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
        OUT.positionHCS = TransformWorldToHClip(positionWS);
        OUT.normalWS = normalWS;
        OUT.uv = IN.uv; // Pass UVs through

        return OUT;
      }

      half4 frag(v2f IN) : SV_Target
      {
        float3 normal = normalize(IN.normalWS);
        Light mainLight = GetMainLight();

        float NdotL = saturate(dot(normal, mainLight.direction));

        // Toon step logic
        float toonShade;
        if (NdotL > 0.66) toonShade = 1.0;
        else if (NdotL > 0.33) toonShade = 0.6;
        else toonShade = 0.3;

        // Sample texture and apply _Color tint
        float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
        float3 litColor = texColor.rgb * _Color.rgb * mainLight.color * toonShade;

        return half4(litColor, texColor.a * _Color.a);
      }
      ENDHLSL
    }
  }

  FallBack Off
}
