Shader "Hidden/InkErase"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ErasePos ("Erase", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _ErasePos;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                float d = distance(i.uv, _ErasePos.xy);

                if (d < _ErasePos.z)
                    col.r = 0;

                return col;
            }
            ENDCG
        }
    }
}