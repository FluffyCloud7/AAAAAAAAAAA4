Shader "Custom/PaperOutside"
{
    Properties
    {
        _BaseColor("Paper Color", Color) = (0.9, 0.85, 0.8, 1)
        _DirtColor("Fiber Color", Color) = (0.8, 0.75, 0.7, 1)
        _NoiseScale("Noise Scale", Float) = 20.0
        _BumpScale("Bump Scale", Float) = 400.0
        _BumpStrength("Bump Strength", Range(0, 0.05)) = 0.002
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionOS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
            };

            float4 _BaseColor;
            float4 _DirtColor;
            float _NoiseScale;
            float _BumpScale;
            float _BumpStrength;

            // Простой генератор шума Градиента
            float hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123); }
            float noise(float2 p) {
                float2 i = floor(p); float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i + float2(0,0)), hash(i + float2(1,0)), u.x),
                            lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), u.x), u.y);
            }

            // Трипланарный шум
            float getTriplanarNoise(float3 pos, float3 normal, float scale) {
                float3 blend = abs(normal);
                blend /= (blend.x + blend.y + blend.z + 0.0001);
                float nX = noise(pos.yz * scale);
                float nY = noise(pos.xz * scale);
                float nZ = noise(pos.xy * scale);
                return nX * blend.x + nY * blend.y + nZ * blend.z;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalOS = TransformWorldToObjectNormal(input.normalWS);
                
                // Считаем шум для цвета
                float nColor = getTriplanarNoise(input.positionOS, normalOS, _NoiseScale);
                half4 finalColor = lerp(_BaseColor, _DirtColor, nColor);

                // Генерируем рельеф нормалей на лету (Bump)
                float p = getTriplanarNoise(input.positionOS, normalOS, _BumpScale);
                float3 dX = ddx(input.positionOS) * _BumpStrength * p;
                float3 dY = ddy(input.positionOS) * _BumpStrength * p;
                float3 modifiedNormal = normalize(input.normalWS + cross(dX, dY));

                // Минимальный матовый расчет света (Unlit-подобный матовый диффуз)
                InputData inputData = (InputData)0;
                inputData.normalWS = modifiedNormal;
                Light mainLight = GetMainLight();
                half3 lighting = half3(1,1,1) * (dot(modifiedNormal, mainLight.direction) * 0.5 + 0.5); // Soft lighting

                return half4(finalColor.rgb * lighting, 1.0);
            }
            ENDHLSL
        }
    }
}