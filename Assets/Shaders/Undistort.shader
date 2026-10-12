Shader "Custom/Undistort"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        // Cull Off ZWrite Off ZTest Always
        // Blend SrcAlpha OneMinusSrcAlpha

        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Cull Off
        ZWrite On
        ZTest LEqual
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float _FxN;
            float _FyN;
            float _CxN;
            float _CyN;
            float _K1;
            float _K2;
            float _K3;
            float _P1;
            float _P2;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float xn = (i.uv.x - _CxN) / _FxN;
                float yn = (i.uv.y - _CyN) / _FyN;

                float r2 = xn * xn + yn * yn;
                float r4 = r2 * r2;
                float r6 = r4 * r2;

                float radial = 1.0 + _K1 * r2 + _K2 * r4 + _K3 * r6;
                float xd = xn * radial + 2.0 * _P1 * xn * yn + _P2 * (r2 + 2.0 * xn * xn);
                float yd = yn * radial + _P1 * (r2 + 2.0 * yn * yn) + 2.0 * _P2 * xn * yn;

                float2 distUV = float2(xd * _FxN + _CxN, yd * _FyN + _CyN);

                if (distUV.x < 0 || distUV.x > 1 || distUV.y < 0 || distUV.y > 1)
                    return fixed4(0, 0, 0, 1);

                
                fixed4 col = tex2D(_MainTex, distUV);
                col.a = 1;
                return col;

                // return tex2D(_MainTex, distUV);

            }
            ENDCG
        }
    }
}