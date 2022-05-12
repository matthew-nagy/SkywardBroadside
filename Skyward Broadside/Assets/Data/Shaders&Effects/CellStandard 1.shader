Shader "Unlit/CellAutumn"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Colour("Colour", Color) = (1, 1, 1, 1)
        _AmbientLevel("Ambient light level", Range(0,1)) = 0.3
    }
    SubShader
    {
        Pass
        {

            Tags {"Queue" = "Geometry" "RenderType" = "Geometry" "LightMode" = "ForwardBase" }
            LOD 100
            Cull[_Cull]
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0      //Get access to screen space pixels

            #include "UnityCG.cginc"

            //Now some stuff to get shadows to work properly
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 pos : SV_POSITION;
                float3 worldPosition : TEXCOORD2;

                SHADOW_COORDS(1)
                fixed3 ambient : COLOR0;

            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _AmbientLevel;
            float4 _Colour;
            float _ShaderAlpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.worldPosition = v.vertex.xyz;


                UNITY_TRANSFER_DEPTH(o.depth);  //Put in depth so our outline shader will work on it 
                o.ambient = ShadeSH9(half4(o.normal, 1));  //Now handle the shadows
                TRANSFER_SHADOW(o)

                return o;
            }

            //UNITY_VPOS_TYPE screenPos : VPOS
            fixed4 frag(v2f i) : SV_Target
            {
                //Phong lighting equations
                float3 cameraToVertexUnit = normalize(i.worldPosition - _WorldSpaceCameraPos);
                float3 lightingDirection = normalize(_WorldSpaceLightPos0.xyz) * -1.0;
                float3 specularHalfVector = (cameraToVertexUnit + lightingDirection) / 2.0;

                float diffuse = dot(lightingDirection, i.normal * -1.0);
                float specular = dot(specularHalfVector, i.normal * -1.0) * 2.0;
                fixed shadow = SHADOW_ATTENUATION(i);
                float shadowDetail = length(shadow * i.ambient);

                float lighting = ((diffuse + specular) / 1.5) * shadowDetail;
                
                //Now we have our full lighting
                lighting = max(lighting, _AmbientLevel);

                //Clamp the lighting into bounds for a cell shading effect
                if (lighting <= 0.3) {
                    lighting = 0.4;
                }
                else if (lighting <= 0.8) {
                    lighting = 0.6;
                }
                else {
                    lighting = 1.0;
                }

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb = lighting * col * _Colour;
                col.a = _ShaderAlpha;
                //Set the red to 1. Makes for a lovely autumn colour.
                col.r = 1.0;
                return col;
            }
            ENDCG
        }
                UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
            Fallback "Diffuse"
}
