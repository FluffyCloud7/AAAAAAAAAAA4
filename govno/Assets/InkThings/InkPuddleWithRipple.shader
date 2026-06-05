Shader "Custom/InkPuddleWithRipple"
{
    Properties
    {
        [MainTexture] _BaseMap("Seamless Ink Texture (PNG)", 2D) = "white" {}
        [MainColor] _BaseColor("Ink Color", Color) = (0.0, 0.7, 0.8, 1.0)
        
        _Mask("Ink Mask (From Script)", 2D) = "white" {} // Для вашей системы стирания
        
        [Header(Ripple Settings)]
        _RippleSpeed("Ripple Speed", Float) = 1.5
        _RippleFrequency("Ripple Frequency", Float) = 10.0
        _RippleAmplitude("Ripple Amplitude", Range(0.0, 0.05)) = 0.01
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
                float _RippleSpeed;
                float _RippleFrequency;
                float _RippleAmplitude;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.rawUV = input.uv; // Чистые UV для маски стирания
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Сэмплируем маску стирания по чистым UV (она строго неподвижна)
                half4 maskColor = _Mask.Sample(sampler_Mask, input.rawUV);
                float eraseMask = maskColor.r; 

                // 2. Создаем легкую рябь (деформацию UV)
                // Используем синус и косинус от времени и исходных координат, чтобы узор слегка «дышал»
                float timeFactor = _Time.y * _RippleSpeed;
                
                float waveX = sin(input.uv.y * _RippleFrequency + timeFactor) * _RippleAmplitude;
                float waveY = cos(input.uv.x * _RippleFrequency + timeFactor) * _RippleAmplitude;
                
                // Добавляем искажение к основным UV-координатам текстуры
                float2 distortedUV = input.uv + float2(waveX, waveY);

                // 3. Сэмплируем текстуру чернил по искаженным координатам
                half4 texColor = _BaseMap.Sample(sampler_BaseMap, distortedUV);

                // 4. Финальный цвет
                half4 finalColor = texColor * _BaseColor;
                
                // Прозрачность зависит от текстуры и маски стирания
                finalColor.a *= eraseMask;

                return finalColor;
            }
            ENDHLSL
        }
    }
}