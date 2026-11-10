Shader "MyProstateRejectsCapitalism/UI/Failure Pressure Veil"
{
    Properties
    {
        [PerRendererData] _MainTex ("UI texture", 2D) = "white" {}
        _Invasion ("Radial invasion", Range(0,1)) = 0
        _Veil ("Veil opacity", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend DstColor Zero, Zero One

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            float _Invasion, _Veil;

            struct Vertex
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Vertex vertex)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(vertex.position);
                output.uv = vertex.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float radius = length((input.uv - 0.5) * float2(1.72, 1.48));
                // Move the entire soft front past the center at peak, including its trailing edge.
                float front = lerp(0.64, -0.57, _Invasion);
                float coverage = smoothstep(front, front + 0.56, radius);
                // URP already tints the edges. Counterbalance that with a slightly stronger center tint.
                float rim = smoothstep(0.12, 0.74, radius);
                float strength = lerp(0.99, 0.94, rim) * coverage * _Veil;
                // Filter the rendered scene's channels rather than blending in a brighter color.
                half3 transmission = lerp(half3(1.0, 1.0, 1.0), half3(1.0, 0.89, 0.025), strength);
                return half4(transmission, 1.0);
            }
            ENDCG
        }
    }
}
