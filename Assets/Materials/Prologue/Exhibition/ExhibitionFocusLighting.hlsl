#ifndef EXHIBITION_FOCUS_LIGHTING_INCLUDED
#define EXHIBITION_FOCUS_LIGHTING_INCLUDED

half3 ExhibitionFocusKey(float3 positionWS, half3 normal, half3 view, float4 source, half weight)
{
    float3 toKey = source.xyz - positionWS;
    float distanceSquared = dot(toKey, toKey);
    half falloff = 1.0h - smoothstep(.14h, 1.0h, distanceSquared * source.w);
    half3 direction = toKey * rsqrt(max(distanceSquared, .0001));
    half diffuse = saturate(dot(normal, direction));
    half sheen = pow(saturate(dot(normal, normalize(direction + view))), 17.0h);
    half3 response = (.18h * diffuse + .037h * sheen) * half3(1.0h, .91h, .75h)
        - .041h * (1.0h - diffuse);
    return weight * falloff * response;
}

#endif
