Shader "Unlit/ScreenSpaceCuttof"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LeftDraw("Draw on the Left", Range(0,1)) = 1
        _CuttoffProp("Cuttoff proportion", Range(0,1)) = 0.5
        _Colour("Colour", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            bool _LeftDraw;
            float _CuttoffProp;
            fixed4 _Colour;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPosition = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float2 screenPosition = (i.screenPosition.xy / i.screenPosition.w);

                bool sideCheck = screenPosition > _CuttoffProp;
                if (sideCheck == _LeftDraw) {
                    return float4(0.0, 0.0, 0.0, 0.0);
                }

                return col * _Colour;
            }
            ENDCG
        }
    }
}
