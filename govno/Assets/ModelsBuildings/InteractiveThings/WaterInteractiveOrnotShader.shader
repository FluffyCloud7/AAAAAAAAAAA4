Shader "Custom/StylizedCartoonWater"
{
    Properties
    {
        [MainColor] _BaseColor("Water Base Color", Color) = (0.0, 0.4, 0.8, 0.6)
        
        [Header(Ripples Settings)]
        _RipplesColor("Ripples Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _RippleSpeed("Ripple Speed", Float) = 1.0
        _RippleScale("Ripple Scale", Float) = 15.0
        _RippleDissolve("Ripple Dissolve (Power)", Range(1.0, 10.0)) = 5.0
        
        [Header(Normal Distortion)]
        _NormalStrength("Water Normal Strength", Range(0.0, 2.0)) = 0.5
        _NoiseScale("Noise Scale", Float) = 24.0
        _NoiseSpeed("Noise Speed (XY)", Vector) = (0.05, -0.01, 0, 0)

        [Header(Foam Settings)]
        _FoamColor("Foam Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _FoamOffset("Foam Offset", Range(0.0, 5.0)) = 0.5
        _WaterTransparency("Water Transparency", Range(0.0, 1.0)) = 0.8
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

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Обязательно для чтения глубины (эффект пены у берегов)
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 screenPos    : TEXCOORD1;
                float3 normalWS     : TEXTURE2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RipplesColor;
                float _RippleSpeed;
                float _RippleScale;
                float _RippleDissolve;
                float _NormalStrength;
                float _NoiseScale;
                float4 _NoiseSpeed;
                float4 _FoamColor;
                float _FoamOffset;
                float _WaterTransparency;
            CBUFFER_END

            // Хелпер для генерации процедурного шума Вороного (Voronoi) из Shader Graph
            inline float2 voronoi_noise_randomVector (float2 UV, float offset)
            {
                float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
                UV = frac(sin(mul(UV, m)) * 46839.32);
                return float2(sin(UV.y * offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
            }

            float voronoi_noise(float2 UV, float AngleOffset, float CellDensity)
            {
                float2 g = floor(UV * CellDensity);
                float2 f = frac(UV * CellDensity);
                float t = 8.0;
                float3 res = float3(8.0, 0.0, 0.0);

                for(int y=-1; y<=1; y++)
                {
                    for(int x=-1; x<=1; x++)
                    {
                        float2 d = float2(float(x), float(y));
                        float2 r = d - f + voronoi_noise_randomVector(g + d, AngleOffset);
                        float d2 = dot(r, r);
                        if(d2 < res.x)
                        {
                            res = float3(d2, r.x, r.y);
                        }
                    }
                }
                return res.x;
            }

            // Хелпер для градиентного шума (Gradient Noise)
            float2 gradient_noise_dir(float2 p)
            {
                p = p % 289.0;
                float x = (34.0 * p.x + 1.0) * p.x % 289.0 + p.y;
                x = (34.0 * x + 1.0) * x % 289.0;
                x = frac(x / 41.0) * 2.0 - 1.0;
                return normalize(float2(frac(x * 4.3) * 2.0 - 1.0, x));
            }

            float gradient_noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(dot(gradient_noise_dir(i + float2(0.0,0.0)), f - float2(0.0,0.0)),
                                 dot(gradient_noise_dir(i + float2(1.0,0.0)), f - float2(1.0,0.0)), u.x),
                            lerp(dot(gradient_noise_dir(i + float2(0.0,1.0)), f - float2(0.0,1.0)),
                                 dot(gradient_noise_dir(i + float2(1.0,1.0)), f - float2(1.0,1.0)), u.x), u.y) + 0.5;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(vertexInput.positionCS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Искажение координат (Radial Shear + Noise)
                float time = _Time.y;
                float2 noiseUV = input.uv + _NoiseSpeed.xy * time;
                float movingNoise = gradient_noise(noiseUV * _NoiseScale);
                
                // Легкое сферическое искажение для ряби Вороного
                float2 distortedUV = input.uv + (movingNoise * _NormalStrength * 0.1);

                // 2. Генерация ряби (Voronoi + Power для толщины линий)
                float voronoiVal = voronoi_noise(distortedUV, time * _RippleSpeed, _RippleScale);
                float ripples = pow(voronoiVal, _RippleDissolve);

                // 3. Расчет пены у берегов через Scene Depth
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneLinearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float screenLinearDepth = input.screenPos.w;
                
                // Разница глубин между водой и объектами под ней
                float depthDiff = sceneLinearDepth - screenLinearDepth;
                float foamValue = 1.0 - saturate(depthDiff / _FoamOffset);

                // 4. Сборка финального цвета и прозрачности
                half4 waterColor = _BaseColor;
                
                // Добавляем цвет движущейся ряби
                waterColor.rgb += ripples * _RipplesColor.rgb * _RipplesColor.a;
                
                // Смешиваем с пеной у берегов
                waterColor.rgb = lerp(waterColor.rgb, _FoamColor.rgb * _FoamColor.a, foamValue);
                
                // Прозрачность зависит от базовых настроек, узора Вороного и плотности пены
                waterColor.a = saturate(waterColor.a * _WaterTransparency + ripples + foamValue);

                return waterColor;
            }
            ENDHLSL
        }
    }
}