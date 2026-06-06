Shader "Custom/PortalShaderURP"
{
    Properties
    {
        [HDR] _Color ("Portal Color", Color) = (1, 0.5, 1, 1)
        _Mask ("Mask Texture (Alpha)", 2D) = "white" {}
        _TwirlStrength ("Twirl Strength", Float) = 10.0
        _Scale ("Voronoi Scale", Float) = 2.5
        _Speed ("Rotation Speed", Float) = 0.5
        _DissolveAmount ("Dissolve Amount", Range(0.1, 10.0)) = 1.0

        // Настройки смешивания для прозрачности
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
        }
        
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            // Настройки для прозрачного двухстороннего рендеринга (как в видео)
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma target 2.0
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
            };

            // Текстуры и Самплеры
            Texture2D _Mask;
            SamplerState sampler_Mask;

            // Константный буфер свойств
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _Mask_ST;
                float _TwirlStrength;
                float _Scale;
                float _Speed;
                float _DissolveAmount;
            CBUFFER_END

            // Вспомогательные функции для генерации шума Вороного (Voronoi)
            inline float2 voronoi_noise_randomVector(float2 UV, float offset)
            {
                float2 x = float2(dot(UV, float2(127.1, 311.7)), dot(UV, float2(269.5, 183.3)));
                return frac(sin(x) * 43758.5453);
            }

            float unity_voronoi(float2 UV, float AngleOffset, float CellDensity)
            {
                float2 g = floor(UV * CellDensity);
                float2 f = frac(UV * CellDensity);
                float t = 8.0;
                float3 res = float3(8.0, 0.0, 0.0);

                for(int y=-1; y<=1; y++)
                {
                    for(int x=-1; x<=1; x++)
                    {
                        float2 lattice = float2(x, y);
                        float2 offset = voronoi_noise_randomVector(lattice + g, AngleOffset);
                        float2 r = lattice - f + (sin(AngleOffset + offset * 6.2831) * 0.5 + 0.5);
                        float d = dot(r, r);
                        if(d < res.x)
                        {
                            res = float3(d, offset.x, offset.y);
                        }
                    }
                }
                return res.x; // Возвращает расстояние Вороного
            }

            // Функция закручивания UV координат (Twirl)
            float2 unity_twirl(float2 UV, float2 Center, float Strength, float2 Offset)
            {
                float2 delta = UV - Center;
                float radius = length(delta);
                float angle = Strength * radius + (Offset.x + Offset.y);
                float x = cos(angle) * delta.x - sin(angle) * delta.y;
                float y = sin(angle) * delta.x + cos(angle) * delta.y;
                return float2(x + Center.x, y + Center.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                // Трансформация координат в соответствии с требованиями URP/Unity 6
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _Mask);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // 1. Анимация времени
                float timeOffset = _Time.y * _Speed;

                // 2. Нода Twirl (закручивание вокруг центра UV [0.5, 0.5])
                float2 twirledUV = unity_twirl(input.uv, float2(0.5, 0.5), _TwirlStrength, float2(timeOffset, 0.0));

                // 3. Генерация шума Voronoi на закрученных UV
                // Передаем 0 в качестве AngleOffset, так как вращение уже задано через Twirl
                float voronoiTex = unity_voronoi(twirledUV, 0.0, _Scale);

                // 4. Эффект Dissolve (Нода Power)
                // Смягчаем или обостряем переходы шума
                float dissolve = pow(voronoiTex, _DissolveAmount);

                // 5. Текстурная маска (чтобы портал оставался круглым по краям)
                float4 maskColor = _Mask.Sample(sampler_Mask, input.uv);
                
                // Комбинируем маску и эффект Вороного (Нода Multiply)
                float finalAlpha = dissolve * maskColor.a;

                // 6. Итоговый цвет с учетом HDR-интенсивности
                float3 finalRGB = _Color.rgb * finalAlpha;

                return float4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Invisible"
}