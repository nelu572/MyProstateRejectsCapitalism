// ACT 2 product-only ceramic response. Does not use or modify scene lights,
// architecture materials, reflection probes, URP configuration, or post effects.
Shader "Prologue/ExhibitionProductPorcelain"
{
    Properties
    {
        _BaseColor ("Product colour", Color) = (.94,.945,.935,1)
        _Glaze ("Soft glaze", Range(0,.12)) = .045
        _Roughness ("Glaze roughness", Range(.3,1)) = .65
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
                half _Glaze;
                half _Roughness;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; half4 tint : COLOR; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half4 tint : COLOR;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.tint = input.tint;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half3 n = normalize(input.normalWS);
                half3 v = normalize(GetCameraPositionWS() - input.positionWS);
                half3 key = normalize(half3(-.45h,.75h,-.5h));
                half form = .70h + .18h * saturate(n.y * .5h + .5h)
                    + .12h * saturate(dot(n,key));
                half nh = saturate(dot(n,normalize(key+v)));
                half broad = pow(nh,lerp(36.0h,10.0h,_Roughness));
                half edge = pow(1.0h-saturate(dot(n,v)),3.0h);
                half glaze = _Glaze * (.8h*broad + .2h*edge);
                return half4(saturate((_BaseColor.rgb * form + glaze) * input.tint.rgb),1);
            }
            ENDHLSL
        }
    }
}
