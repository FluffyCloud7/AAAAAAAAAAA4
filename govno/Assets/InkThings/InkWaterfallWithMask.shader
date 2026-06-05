Shader "Custom/InkWaterfallWithMask"
{
    Properties
    {
        [MainTexture] _BaseMap("Seamless Ink Texture (PNG)", 2D) = "white" {}
        [MainColor] _BaseColor("Ink Color", Color) = (0.0, 0.7, 0.8, 1.0)
        
        _Mask("Ink Mask (From Script)", 2D) = "white" {} // ТО САМОЕ СВОЙСТВО ДЛЯ СКРИПТА
        
        _ScrollSpeedX("Scroll Speed X", Float) = 0.0
        _ScrollSpeedY("Scroll Speed Y", Float) = -1.0
        
        _FadePower("Fade Sharpness", Range(0.1, 5.0)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }
        
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off 

        Pass
        {
            Name "ForwardLit"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float2 rawUV        : TEXCOORD1; 
            };

            Texture2D _BaseMap;
            SamplerState sampler_BaseMap;
            
            Texture2D _Mask;
            SamplerState sampler_Mask;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _ScrollSpeedX;
                float _ScrollSpeedY;
                float _FadePower;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.rawUV = input.uv; // Чистые UV для маски и затухания
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Сэмплируем маску стирания по чистым UV (она НЕ должна двигаться)
                half4 maskColor = _Mask.Sample(sampler_Mask, input.rawUV);
                float eraseMask = maskColor.r; // В вашей системе r > 0.1f означает наличие чернил

                // 2. Анимация движения основного узора чернил
                float2 scrollOffset = float2(_ScrollSpeedX, _ScrollSpeedY) * _Time.y;
                float2 scrolledUV = input.uv + scrollOffset;
                half4 texColor = _BaseMap.Sample(sampler_BaseMap, scrolledUV);

                // 3. Расчет вертикального затухания к низу меша
                float verticalFade = pow(saturate(input.rawUV.y), _FadePower);

                // 4. Финальная сборка цвета
                half4 finalColor = texColor * _BaseColor;
                
                // Прозрачность зависит от узора, вертикального растворения И маски стирания губки!
                finalColor.a *= verticalFade * eraseMask;

                return finalColor;
            }
            ENDHLSL
        }
    }
}