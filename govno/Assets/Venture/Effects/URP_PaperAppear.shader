Shader "Custom/URP_PaperAppear"
{
    Properties
    {
        [MainTexture] _MainTex("Base Texture", 2D) = "white" {}
        [MainColor]   _BaseColor("Base Color", Color) = (1,1,1,1)
        
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        
        _AppearAmount("Appear Amount (0->1)", Range(0.0, 1.0)) = 0.0
        _EdgeWidth("Edge Width", Range(0.0, 0.2)) = 0.05
        [HDR] _EdgeColor("Edge Color", Color) = (1, 0.5, 0, 1)
        
        [Header(Height Settings)]
        _MinY("Min Local Y (Feet)", Float) = 0.0
        _MaxY("Max Local Y (Head)", Float) = 2.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 localPos     : TEXCOORD1;
                half3  normalWS     : TEXCOORD3;
                half4  tangentWS    : TEXCOORD4;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _BaseColor;
                half _AppearAmount;
                half _EdgeWidth;
                half4 _EdgeColor;
                float _MinY;
                float _MaxY;
            CBUFFER_END

            float2 hash(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            float GradientNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(dot(hash(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)), dot(hash(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
                            lerp(dot(hash(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)), dot(hash(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.localPos = input.positionOS.xyz;
                
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = half4(normalInput.tangentWS, input.tangentOS.w);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float height = saturate((input.localPos.y - _MinY) / (_MaxY - _MinY));
                float noise = GradientNoise(input.uv * 1.0) * 0.5 + 0.5;
                float combinedMap = height * 0.5 + noise * 0.5;

                // Ћќ√» ј ѕќя¬Ћ≈Ќ»я: при _AppearAmount = 0 всЄ стЄрто. ѕри 1 Ч всЄ про€влено.
                float clipVal = _AppearAmount - combinedMap;
                clip(clipVal);

                // ћаска свет€щегос€ кра€, который бежит впереди про€вл€ющихс€ кусков бумаги
                half edgeMask = step(clipVal, _EdgeWidth) * step(0.0, clipVal) * step(0.01, _AppearAmount);

                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv));
                half3 bitangentWS = cross(input.normalWS, input.tangentWS.xyz) * input.tangentWS.w;
                half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangentWS, input.normalWS);
                half3 normalWS = NormalizeNormalPerPixel(mul(normalTS, tangentToWorld));

                Light mainLight = GetMainLight();
                half3 diffuseLighting = LightingLambert(mainLight.color, mainLight.direction, normalWS);
                half3 ambientLighting = SampleSH(normalWS);

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _BaseColor;
                texColor.rgb *= (diffuseLighting + ambientLighting);

                half3 finalEmission = _EdgeColor.rgb * edgeMask;
                texColor.rgb += finalEmission;

                return texColor;
            }
            ENDHLSL
        }
    }
}