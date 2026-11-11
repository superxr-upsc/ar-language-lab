// AdvancedOutlineFillURP.shader
// URP-compatible outline shader that extrudes vertices along smooth normals and renders color where stencil != 1.
// Supports custom screen-space dashed animation patterns.
Shader "Custom/Advanced Outline Fill URP" {
  Properties {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
    _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
    _OutlineWidth("Outline Width", Range(0, 10)) = 2
    
    [Header(Dashed Pattern)]
    _DashScale("Dash Scale", Float) = 0
    _DashSpeed("Dash Speed", Float) = 0
  }

  SubShader {
    Tags {
      "RenderPipeline" = "UniversalPipeline"
      "Queue" = "Transparent+110"
      "RenderType" = "Transparent"
      "DisableBatching" = "True"
    }

    Pass {
      Name "Fill"
      Cull Off
      ZTest [_ZTest]
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha
      ColorMask RGB

      Stencil {
        Ref 1
        Comp NotEqual
      }

      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #pragma multi_compile_instancing

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

      struct Attributes {
        float4 positionOS   : POSITION;
        float3 normalOS     : NORMAL;
        float3 smoothNormal : TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct Varyings {
        float4 positionCS   : SV_POSITION;
        float4 screenPos    : TEXCOORD4;
        float4 color        : COLOR;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      float4 _OutlineColor;
      float _OutlineWidth;
      float _DashScale;
      float _DashSpeed;

      Varyings vert(Attributes input) {
        Varyings output;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        // Choose smoothNormal if present, fallback to standard normal
        float3 normal = any(input.smoothNormal) ? input.smoothNormal : input.normalOS;
        
        // Transform normal to world space and then to view space
        float3 normalWS = TransformObjectToWorldNormal(normal);
        float3 normalVS = TransformWorldToViewNormal(normalWS);

        // Transform position to view space
        float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
        float3 positionVS = TransformWorldToView(positionWS);

        // Extrude in view space
        float3 extrudedPositionVS = positionVS + normalVS * -positionVS.z * _OutlineWidth / 1000.0;

        // Project to clip space
        output.positionCS = mul(GetViewToHClipMatrix(), float4(extrudedPositionVS, 1.0));
        output.color = _OutlineColor;
        
        // Compute screen space position for the dashed effect
        output.screenPos = ComputeScreenPos(output.positionCS);

        return output;
      }

      float4 frag(Varyings input) : SV_Target {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        // Calculate dashed/crawling pattern if _DashScale is non-zero
        if (_DashScale > 0.001) {
          float2 screenPos = input.screenPos.xy / input.screenPos.w;
          
          // Multiply by screen resolution to keep dash length consistent across aspect ratios
          float2 pixelPos = screenPos * _ScreenParams.xy;
          
          // scrolling diagonal dashes pattern
          float pattern = sin((pixelPos.x + pixelPos.y) * _DashScale - _Time.y * _DashSpeed);
          if (pattern < 0.0) {
            discard;
          }
        }

        return input.color;
      }
      ENDHLSL
    }
  }
}
