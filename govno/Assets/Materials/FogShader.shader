Shader "Custom/TrueSoftVolumeFog"
{
    Properties
    {
        // Один единственный ползунок, который отвечает за ВСЮ мягкость
        _FogSoftness ("Fog Softness (Distance)", Float) = 10.0
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent+99" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "VolumeFogPass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 screenPos    : TEXCOORD0;
            };

            float _FogSoftness;

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                
                output.positionCS = vertexInput.positionCS;
                output.screenPos = ComputeScreenPos(vertexInput.positionCS);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.screenPos.xy / input.screenPos.w;
                
                // 1. Хватаем цвет того, что ЗА кубом (небо, колонны)
                float3 backgroundColor = SampleSceneColor(uv);
                
                // 2. Считаем глубину сцены (расстояние до колонны/земли)
                float rawDepth = SampleSceneDepth(uv);
                float sceneLinearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                
                // 3. Считаем глубину ПЕРЕДНЕГО полигона куба (где луч вошел в куб)
                float currentDepth = input.screenPos.w;
                
                // 4. Главная магия: считаем толщину тумана в этой точке экрана
                // Разница между тем, где луч вошел в куб, и где он ударился о колонну/задний план
                float fogThickness = sceneLinearDepth - currentDepth;
                
                // Если сзади бесконечное небо, толщина тумана — это просто расстояние до передней грани
                #if UNITY_REVERSED_Z
                    if (rawDepth == 0.0) fogThickness = currentDepth;
                #else
                    if (rawDepth == 1.0) fogThickness = currentDepth;
                #endif

                // Плавное угасание тумана в зависимости от его толщины
                float fogFactor = saturate(fogThickness / max(0.001, _FogSoftness));
                
                // Делаем переход супер-воздушным без резких углов
                fogFactor = smoothstep(0.0, 1.0, fogFactor);

                // Возвращаем цвет заднего плана, размытый по толщине куба
                return float4(backgroundColor, fogFactor);
            }
            ENDHLSL
        }
    }
}