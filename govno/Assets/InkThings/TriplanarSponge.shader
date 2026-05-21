Shader "Custom/TriplanarSponge"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Tiling ("Tiling", Float) = 4
        _NormalStrength ("Normal Strength", Range(0,2)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NormalMap;

            float _Tiling;
            float _NormalStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            float3 TriplanarBlend(float3 normal)
            {
                float3 blend = abs(normal);
                blend = normalize(max(blend, 0.00001));
                blend /= (blend.x + blend.y + blend.z);
                return blend;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 blend = TriplanarBlend(i.worldNormal);

                float2 xUV = i.worldPos.yz * _Tiling;
                float2 yUV = i.worldPos.xz * _Tiling;
                float2 zUV = i.worldPos.xy * _Tiling;

                fixed4 xTex = tex2D(_MainTex, xUV);
                fixed4 yTex = tex2D(_MainTex, yUV);
                fixed4 zTex = tex2D(_MainTex, zUV);

                fixed4 albedo =
                    xTex * blend.x +
                    yTex * blend.y +
                    zTex * blend.z;

                float3 xNormal = UnpackNormal(tex2D(_NormalMap, xUV));
                float3 yNormal = UnpackNormal(tex2D(_NormalMap, yUV));
                float3 zNormal = UnpackNormal(tex2D(_NormalMap, zUV));

                float3 normal =
                    xNormal * blend.x +
                    yNormal * blend.y +
                    zNormal * blend.z;

                normal.xy *= _NormalStrength;
                normal = normalize(normal);

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

                float NdotL = saturate(dot(normalize(i.worldNormal + normal), lightDir));

                float3 finalColor = albedo.rgb * (0.2 + NdotL);

                return float4(finalColor, 1);
            }

            ENDHLSL
        }
    }
}