Shader "Prologue/ExhibitionWhite"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _ShapeShading ("Exhibit shape shading", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _ShapeShading;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; half3 normalWS : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                // Architecture uses zero shading: no lights, fog, AO, reflection or bloom emission.
                // Only exhibit materials use a fixed, broad hemisphere to retain their curved shape.
                half3 normal = normalize(input.normalWS);
                half shape = 0.64h + 0.22h * saturate(normal.y * 0.5h + 0.5h)
                    + 0.14h * saturate(dot(normal, normalize(half3(-0.4h, 0.8h, -0.25h))));
                return half4(_BaseColor.rgb * lerp(1.0h, shape, _ShapeShading), 1.0h);
            }
            ENDHLSL
        }
    }
}
