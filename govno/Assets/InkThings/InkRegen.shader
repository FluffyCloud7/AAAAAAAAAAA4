Shader "Hidden/InkRegen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RegenSpeed ("Speed", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _RegenSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Шаг смещения к соседям (радиус затекания)
                // Умножаем на 1.5, чтобы чернила "текли" быстрее и увереннее
                float2 texel = _MainTex_TexelSize.xy * 1.5;

                // Считываем текущий пиксель и 4-х соседей
                float current = tex2D(_MainTex, i.uv).r;
                float up      = tex2D(_MainTex, i.uv + float2(0, texel.y)).r;
                float down    = tex2D(_MainTex, i.uv + float2(0, -texel.y)).r;
                float left    = tex2D(_MainTex, i.uv + float2(-texel.x, 0)).r;
                float right   = tex2D(_MainTex, i.uv + float2(texel.x, 0)).r;

                // Берем максимальное значение среди соседей
                float maxNeighbor = max(max(up, down), max(left, right));

                // Логика затекания: пиксель стремится к более заполненному соседу
                float result = current + (maxNeighbor - current) * _RegenSpeed;

                // ФИКС ЗАВИСАНИЯ: Если пиксель хоть немного закрашен чернилами, 
                // мы пинком подталкиваем его вверх к 1.0. Это не дает краям оставаться размытыми.
                if (result > 0.01)
                {
                    // Чем ближе к единице, тем сильнее уплотняется цвет
                    result += _RegenSpeed * 0.5; 
                }

                // Общая фоновая подпитка (гарантия закрытия центра больших дыр)
                result += _RegenSpeed * 0.05;

                fixed4 col = fixed4(0, 0, 0, 1);
                col.r = min(1.0, result);
                return col;
            }
            ENDCG
        }
    }
}