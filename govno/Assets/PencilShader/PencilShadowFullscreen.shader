Shader "Custom/PencilShadowFullscreen"
{
    Properties
    {
        _HatchJitterSpeed("Hatch Jitter Speed", Float) = 5
        _HatchJitterStrength("Hatch Jitter Strength", Float) = 0.01

        _HatchSpeed("Hatch Speed", Float) = 0.1

        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _HatchColor("Hatch Line Color", Color) = (0, 0, 0, 1) // Новый параметр цвета
        _HatchOpacity("Hatch Opacity", Range(0, 1)) = 1.0     // Новый параметр прозрачности
        _HatchContrast("Hatch Contrast", Range(0.1, 5)) = 1

        _HatchTex ("Hatching Texture", 2D) = "white" {}
        _HatchScale ("Hatch Scale", Float) = 10               // Вернули твой дефолтный масштаб
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            TEXTURE2D(_HatchTex);
            SAMPLER(sampler_HatchTex);

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float _HatchJitterSpeed;
                float _HatchJitterStrength;
                half4 _BaseColor;
                half4 _HatchColor;
                float _HatchOpacity;
                float _HatchContrast;
                float _HatchSpeed;
                float _HatchScale; // Теперь переменная строго в CBUFFER, как положено в URP
            CBUFFER_END

            Varyings vert(uint vertexID : SV_VertexID)
            {
                Varyings OUT;

                // Fullscreen triangle (Твой исходный рабочий вариант)
                float2 pos = float2(
                    (vertexID == 2) ? 3.0 : -1.0,
                    (vertexID == 1) ? 3.0 : -1.0
                );

                OUT.positionHCS = float4(pos, 0, 1);
                OUT.uv = pos * 0.5 + 0.5;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                
                // Настройка размера штриховки через _HatchScale (как ты и просила)
                float2 hatchUV = uv * _HatchScale;

                float t = _Time.y * _HatchJitterSpeed;

                float2 jitter = float2(
                    sin(t * 1.7),
                    cos(t * 1.3)
                ) * _HatchJitterStrength;

                hatchUV += jitter;

                // СТРОГО КАК В ТВОЕМ КОДЕ: переворот UV идет строго после расчета hatchUV
                #if UNITY_UV_STARTS_AT_TOP
                uv.y = 1.0 - uv.y;
                #endif

                half4 sceneColor = SAMPLE_TEXTURE2D(
                    _CameraOpaqueTexture,
                    sampler_CameraOpaqueTexture,
                    uv
                );

                half hatch = SAMPLE_TEXTURE2D(
                    _HatchTex,
                    sampler_HatchTex,
                    hatchUV
                ).r;

                hatch = 1.0 - hatch;

                // Контраст
                hatch = pow(hatch, _HatchContrast);

                float rawDepth = SAMPLE_DEPTH_TEXTURE(
                    _CameraDepthTexture,
                    sampler_CameraDepthTexture,
                    uv
                );

                float3 positionWS = ComputeWorldSpacePosition(
                    uv,
                    rawDepth,
                    UNITY_MATRIX_I_VP
                );

                // Shadow coord
                float4 shadowCoord = TransformWorldToShadowCoord(positionWS);

                // Реальная тень
                float shadow = MainLightRealtimeShadow(shadowCoord);

                // Корректное наложение штриховки только на тени с учетом прозрачности и цвета:
                half hatchMask = hatch * (1.0 - shadow) * _HatchOpacity;
                
                // Смешиваем оригинальный цвет сцены с кастомным цветом штриховки
                half3 finalColor = lerp(sceneColor.rgb, _HatchColor.rgb * sceneColor.rgb, hatchMask);

                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }
}