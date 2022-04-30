Shader "Unlit/TransparentWave"
{
    Properties
    {
        _Colour("Colour", Vector) = (1.0, 1.0, 1.0, 1.0)
        _DrawLimit("DrawLimit", Range(0, 1)) = 0.9
        _RepeatFactor("Repeat factor", float) = 3.0
        _ScrollSpeed("Scroll speed", float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 worldY : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Colour;
            float _DrawLimit;
            float _RepeatFactor;
            float _ScrollSpeed;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldY = v.vertex.y;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Colour;
                float time = _Time.y * _ScrollSpeed;

                float a = sin(i.worldY*_RepeatFactor + time);
                if (a < 0) {
                    a *= -1.0;
                }

                if (a > _DrawLimit) {
                    a = 1.0;
                }
                else {
                    a = 0.0;
                }

                col.a = a;

                return col;
            }
            ENDCG
        }
    }
}
