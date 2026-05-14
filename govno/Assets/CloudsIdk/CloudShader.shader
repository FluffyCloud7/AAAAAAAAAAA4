Shader "Custom/CloudVolume"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _NoiseScale("Noise Scale", Float) = 0.02
        _Density("Density", Range(0,1)) = 0.5
        _Speed("Speed", Float) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _BaseColor;
            float _NoiseScale;
            float _Density;
            float _Speed;

            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float noise(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);

                f = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(
                        lerp(hash(i + float3(0,0,0)),
                             hash(i + float3(1,0,0)), f.x),

                        lerp(hash(i + float3(0,1,0)),
                             hash(i + float3(1,1,0)), f.x),

                        f.y),

                    lerp(
                        lerp(hash(i + float3(0,0,1)),
                             hash(i + float3(1,0,1)), f.x),

                        lerp(hash(i + float3(0,1,1)),
                             hash(i + float3(1,1,1)), f.x),

                        f.y),

                    f.z);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs =
                    GetVertexPositionInputs(IN.positionOS.xyz);

                OUT.positionHCS = posInputs.positionCS;
                OUT.worldPos = posInputs.positionWS;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 p = IN.worldPos;

                p.x += _Time.y * _Speed;

                float n = noise(p * _NoiseScale);

                n += noise(p * _NoiseScale * 2.0) * 0.5;
                n += noise(p * _NoiseScale * 4.0) * 0.25;

                float alpha = smoothstep(
                    0.4,
                    0.7,
                    n * _Density
                );

                return half4(_BaseColor.rgb, alpha);
            }

            ENDHLSL
        }
    }
}