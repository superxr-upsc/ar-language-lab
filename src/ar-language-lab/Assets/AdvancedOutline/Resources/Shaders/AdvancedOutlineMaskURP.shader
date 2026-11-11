// AdvancedOutlineMaskURP.shader
// URP-compatible stencil mask shader that does not write color, only masking the object's geometry.
Shader "Custom/Advanced Outline Mask URP" {
  Properties {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
  }

  SubShader {
    Tags {
      "RenderPipeline" = "UniversalPipeline"
      "Queue" = "Transparent+100"
      "RenderType" = "Transparent"
    }

    Pass {
      Name "Mask"
      Cull Off
      ZTest [_ZTest]
      ZWrite Off
      ColorMask 0

      Stencil {
        Ref 1
        Pass Replace
      }

      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

      struct Attributes {
        float4 positionOS   : POSITION;
      };

      struct Varyings {
        float4 positionCS   : SV_POSITION;
      };

      Varyings vert(Attributes input) {
        Varyings output;
        output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
        return output;
      }

      float4 frag(Varyings input) : SV_Target {
        return 0;
      }
      ENDHLSL
    }
  }
}
