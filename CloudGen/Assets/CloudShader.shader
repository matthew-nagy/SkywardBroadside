Shader "Unlit/CloudShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;

                float3 camera_right = float3(1.0, 0.0, 0.0);
                float3 camera_up = float3(0.0, 1.0, 0.0);

                float4 world_origin = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1));
                float4 view_origin = float4(UnityObjectToViewPos(float3(0, 0, 0)), 1);

                float4 world_pos = mul(UNITY_MATRIX_M, v.vertex);
                //float4 view_pos = mul(UNITY_MATRIX_V, world_pos);

                float4 view_pos = world_pos - world_origin + view_origin + float4(float3(10, 0, 0), 0);

                float4 clip_pos = mul(UNITY_MATRIX_P, view_pos);

                o.vertex = clip_pos;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
