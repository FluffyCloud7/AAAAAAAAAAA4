Shader "Custom/InkWaterfallWithCustomFade"
{
    Properties
    {
        [MainTexture] _BaseMap("Seamless Ink Texture (PNG)", 2D) = "white" {}
        [MainColor] _BaseColor("Ink Color", Color) = (0.0, 0.7, 0.8, 1.0)
        
        _Mask("Ink Mask (From Script)", 2D) = "white" {} 
        
        _ScrollSpeedX("Scroll Speed X", Float) = 0.0
        _ScrollSpeedY("Scroll Speed Y", Float) = -1.0
        
        [Header(Fade Settings)]
        _FadePower("Fade Sharpness (Резкость края)", Range(0.1, 10.0)) = 2.0
        _FadeOffset("Fade Offset (Плотная часть)", Range(0.0, 1.0)) = 0.6
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
                float _FadeOffset;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.rawUV = input.uv; 
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Сэмплируем маску стирания губки
                half4 maskColor = _Mask.Sample(sampler_Mask, input.rawUV);
                float eraseMask = maskColor.r; 

                // 2. Движение чернил
                float2 scrollOffset = float2(_ScrollSpeedX, _ScrollSpeedY) * _Time.y;
                float2 scrolledUV = input.uv + scrollOffset;
                half4 texColor = _BaseMap.Sample(sampler_BaseMap, scrolledUV);

                // 3. НОВАЯ ЛОГИКА ЗАТУХАНИЯ:
                // Перенаправляем градиент так, чтобы верхняя часть контролировалась ползунком
                float remapUV = saturate(input.rawUV.y / _FadeOffset);
                float verticalFade = pow(remapUV, _FadePower);

                // 4. Сборка
                half4 finalColor = texColor * _BaseColor;
                finalColor.a *= verticalFade * eraseMask;

                return finalColor;
            }
            ENDHLSL
        }
    }
}