#ifndef MAIN_LIGHT_SHADOW_INCLUDED
#define MAIN_LIGHT_SHADOW_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void MainLightShadow_float(float3 WorldPos, out float Shadow)
{
#ifdef SHADERGRAPH_PREVIEW
    Shadow = 1;
#else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    Light mainLight = GetMainLight(shadowCoord);
    Shadow = mainLight.shadowAttenuation;
#endif
}

#endif
