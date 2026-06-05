Shader "Custom/InkWaterfall"
{
    Properties
    {
        [MainTexture] _BaseMap("Ink Texture (PNG)", 2D) = "white" {}
        [MainColor] _BaseColor("Ink Color", Color) = (0.0, 0.7, 0.8, 1.0)
        
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
        Cull Off // Видно обе стороны меша, полезно для плоских водопадов

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
                float2 rawUV        : TEXCOORD1; // Сохраняем чистые UV для градиента
            };

            Texture2D _BaseMap;
            SamplerState sampler_BaseMap;

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
                
                // Трансформация вершин в пространство экрана
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                
                // Базовые UV с учетом Tiling и Offset из инспектора
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                // Передаем исходные UV без изменений (нужны для затухания)
                output.rawUV = input.uv;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Анимация движения (Scrolling)
                float2 scrollOffset = float2(_ScrollSpeedX, _ScrollSpeedY) * _Time.y;
                float2 scrolledUV = input.uv + scrollOffset;

                // 2. Семплирование бесшовных чернил
                half4 texColor = _BaseMap.Sample(sampler_BaseMap, scrolledUV);

                // 3. Расчет вертикального затухания (Fade)
                // Предполагается, что 1.0 — верх водопада, а 0.0 — низ, где всё исчезает
                float verticalFade = input.rawUV.y; 
                
                // Возводим в степень для управления жесткостью перехода
                verticalFade = pow(saturate(verticalFade), _FadePower);

                // 4. Итоговый цвет и прозрачность
                half4 finalColor = texColor * _BaseColor;
                
                // Умножаем прозрачность текстуры на наш вертикальный градиент
                finalColor.a *= verticalFade;

                return finalColor;
            }
            ENDHLSL
        }
    }
}