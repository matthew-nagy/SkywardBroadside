Shader "Unlit/CloudShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Scale("Cloud_scale", float) = 128.0
        _BubbleSize("Bubble size", float) = 1.0
    }
        Category{

       ZWrite Off
       Cull Off
       Blend SrcAlpha OneMinusSrcAlpha
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
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
                    fixed4 colour : COLOR;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _Scale;
                float _BubbleSize;

            v2f vert(appdata v)
            {
                v2f o;

                float4 world_origin = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1));
                float4 view_origin = float4(UnityObjectToViewPos(float3(0, 0, 0)), 1);

                float4 colourPos = float4(v.colour.rgb * _Scale, v.vertex.w);

                float4 world_pos = mul(UNITY_MATRIX_M, colourPos);
                //float4 world_pos = mul(UNITY_MATRIX_M, v.vertex);

                float4 view_pos = mul(UNITY_MATRIX_V, world_pos);

                //Scale herebased on noise at this point
                view_pos += float4(v.uv * _BubbleSize, 0.0, 0.0);

                float4 clip_pos = mul(UNITY_MATRIX_P, view_pos);

                o.vertex = clip_pos;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col.a *= 0.75;
                return col;
            }
            ENDCG
        }
    }
        }
}