Shader "Prologue/ExhibitionInspectionOutline"
{
    Properties
    {
        _Ink ("Ink", Color) = (0.18, 0.15, 0.15, 1)
        _WidthPx ("Width in render pixels", Float) = 3
        _OffsetPx ("Registration offset in render pixels", Vector) = (0,0,0,0)
        _Opacity ("Opacity", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-100" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZTest LEqual
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Ink;
                float4 _OffsetPx;
                float _WidthPx;
                half _Opacity;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float4 position = TransformObjectToHClip(input.positionOS.xyz);
                float3 normalVS = TransformWorldToViewDir(TransformObjectToWorldNormal(input.normalOS));
                float4 direction = mul(UNITY_MATRIX_P, float4(normalVS, 0.0));
                // Differentiate clip XY / W so the width is constant even on a close, oblique surface.
                float2 size = GetScaledScreenParams().xy;
                float2 tangent = (direction.xy * position.w - position.xy * direction.w) * size;
                float2 normalPx = tangent * rsqrt(max(dot(tangent, tangent), .00001));
                position.xy += (normalPx * _WidthPx + _OffsetPx.xy) * (2.0 / size) * position.w;
                output.positionCS = position;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(_Ink.rgb, _Ink.a * _Opacity);
            }
            ENDHLSL
        }
    }
}
