Shader "Hidden/PencilShadowFullscreen"
{
    Properties
    {
        _PencilTex ("Pencil Texture", 2D) = "white" {}
        _PencilScale ("Pencil Scale", Float) = 4
        _PencilSpeed ("Pencil Speed", Float) = 0.2
        _ShadowStrength ("Shadow Strength", Range(0,1)) = 0.8
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Pencil Shadow Fullscreen"

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            TEXTURE2D(_PencilTex);
            SAMPLER(sampler_PencilTex);

            float _PencilScale;
            float _PencilSpeed;
            float _ShadowStrength;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 Frag (Varyings IN) : SV_Target
            {
                float3 sceneColor =
                    SAMPLE_TEXTURE2D(
                        _CameraOpaqueTexture,
                        sampler_CameraOpaqueTexture,
                        IN.uv
                    ).rgb;

                // Маска тени по яркости (пост-эффект)
                float luminance = dot(sceneColor, float3(0.3, 0.59, 0.11));
                float shadowMask = saturate(1 - luminance);

                float2 pencilUV = IN.uv * _PencilScale;
                pencilUV += _Time.y * _PencilSpeed;

                float pencil =
                    SAMPLE_TEXTURE2D(
                        _PencilTex,
                        sampler_PencilTex,
                        pencilUV
                    ).r;

                float shadowEffect =
                    lerp(1.0, pencil, shadowMask * _ShadowStrength);

                return float4(sceneColor * shadowEffect, 1);
            }
            ENDHLSL
        }
    }
}
