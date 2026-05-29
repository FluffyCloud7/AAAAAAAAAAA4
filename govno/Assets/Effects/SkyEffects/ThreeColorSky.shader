Shader "Custom/SkyboxWithHorizonClouds"
{
    Properties
    {
        [Header(Sky Gradient)]
        _SkyColor ("Sky Color", Color) = (0.3, 0.4, 0.6, 1)
        _HorizonColor ("Horizon Color", Color) = (0.6, 0.7, 0.8, 1)
        _GroundColor ("Ground Color", Color) = (0.4, 0.3, 0.2, 1)
        _SkyBlur ("Sky Blur (Exponent)", Range(0.1, 10)) = 1.0
        _GroundBlur ("Ground Blur (Exponent)", Range(0.1, 10)) = 1.0

        [Header(Clouds Settings)]
        _CloudTex ("Clouds Texture (2D Wrapped)", 2D) = "white" {}
        _CloudColor ("Cloud Tint", Color) = (1, 1, 1, 1)
        _Cutoff ("Alpha Cutoff (Sharp Edge)", Range(0, 1)) = 0.1
        _RotationSpeed ("Rotation Speed", Float) = 0.02
        
        _CloudHeight ("Cloud Height Shift", Range(-1, 1)) = 0.0
        _CloudThickness ("Cloud Thickness (Scale)", Range(0.1, 10)) = 1.0

        [Header(Color Correction)]
        _CloudIntensity ("Cloud Intensity (Boost)", Range(1, 5)) = 1.0
        _CloudSaturation ("Cloud Saturation", Range(0, 2)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 viewDir      : TEXCOORD0;
            };

            float4 _SkyColor;
            float4 _HorizonColor;
            float4 _GroundColor;
            float _SkyBlur;
            float _GroundBlur;

            Texture2D _CloudTex;
            SamplerState sampler_CloudTex;
            float4 _CloudColor;
            float _Cutoff;
            float _RotationSpeed;
            float _CloudHeight;
            float _CloudThickness;
            float _CloudIntensity;
            float _CloudSaturation;

            #define PI 3.14159265359

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.viewDir = input.positionOS.xyz;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.viewDir);
                float y = dir.y;

                // 1. Градиент неба
                float3 skyGradient;
                if (y > 0.0)
                {
                    skyGradient = lerp(_HorizonColor.rgb, _SkyColor.rgb, pow(y, _SkyBlur));
                }
                else
                {
                    skyGradient = lerp(_HorizonColor.rgb, _GroundColor.rgb, pow(-y, _GroundBlur));
                }

                // 2. UV координаты облаков
                float angle = atan2(dir.z, dir.x);
                float u = (angle + PI) / (2.0 * PI);
                u += _Time.y * _RotationSpeed;

                float v = (y * _CloudThickness) - _CloudHeight + 0.5;

                float3 finalColor = skyGradient;

                if (v >= 0.0 && v <= 1.0)
                {
                    float4 cloudSample = _CloudTex.Sample(sampler_CloudTex, float2(u, v));
                    float alpha = cloudSample.a * _CloudColor.a;

                    if (alpha >= _Cutoff)
                    {
                        float3 cColor = cloudSample.rgb * _CloudColor.rgb;

                        // Коррекция сочности (насыщенности)
                        float luma = dot(cColor, float3(0.2126, 0.7152, 0.0722));
                        cColor = lerp(float3(luma, luma, luma), cColor, _CloudSaturation);

                        // Прожигаем блеклость через интенсивность
                        cColor *= _CloudIntensity;

                        finalColor = lerp(finalColor, cColor, alpha);
                    }
                }

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}