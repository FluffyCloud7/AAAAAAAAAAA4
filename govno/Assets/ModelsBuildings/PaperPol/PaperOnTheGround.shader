Shader "Custom/PaperDecalShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Paper Texture (RGBA)", 2D) = "white" {}
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline"
        }

        // Настройки блендинга: включаем честную прозрачность
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "DecalPass"

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
            };

            Texture2D _BaseMap;
            SamplerState sampler_BaseMap;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                // Базовая трансформация координат для URP
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Читаем текстуру
                float4 color = _BaseMap.Sample(sampler_BaseMap, input.uv);
                
                // Если альфа нулевая или около того — полностью отсекаем пиксель из рендера
                if (color.a < 0.01)
                {
                    discard;
                }

                return color;
            }
            ENDHLSL
        }
    }
}