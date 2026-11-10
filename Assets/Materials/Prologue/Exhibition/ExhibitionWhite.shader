Shader "Prologue/ExhibitionWhite"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _ShapeShading ("Exhibit shape shading", Range(0,1)) = 0
        [HideInInspector] _InspectionKey ("Local inspection key", Vector) = (0,0,0,0)
        [HideInInspector] _InspectionKeyWeight ("Local inspection key weight", Float) = 0
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
            #include "Assets/Materials/Prologue/Exhibition/ExhibitionFocusLighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _ShapeShading;
                float4 _InspectionKey;
                half _InspectionKeyWeight;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                // Architecture uses zero shading: no lights, fog, AO, reflection or bloom emission.
                // Only exhibit materials use a fixed, broad hemisphere to retain their curved shape.
                half3 normal = normalize(input.normalWS);
                half shape = 0.64h + 0.22h * saturate(normal.y * 0.5h + 0.5h)
                    + 0.14h * saturate(dot(normal, normalize(half3(-0.4h, 0.8h, -0.25h))));
                half3 color = _BaseColor.rgb * lerp(1.0h, shape, _ShapeShading);
                UNITY_BRANCH if (_InspectionKeyWeight > 0)
                {
                    half3 view = normalize(GetCameraPositionWS() - input.positionWS);
                    color = saturate(color + ExhibitionFocusKey(input.positionWS, normal, view,
                        _InspectionKey, _InspectionKeyWeight));
                }
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}
