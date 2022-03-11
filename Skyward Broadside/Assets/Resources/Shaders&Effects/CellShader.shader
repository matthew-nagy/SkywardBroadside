Shader "Unlit/CellShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Colour("Colour", Color) = (1, 1, 1, 1)
        _ScriptAlpha("Script alpha", float) = 1.0
        _AmbientLevel("Ambient light level", Range(0,1)) = 0.2
    }
        SubShader
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Tags {"RenderType" = "Opaque" "LightMode" = "ForwardBase" }// "RenderType" = "Transparent" }
            LOD 100
            Cull[_Cull]
            ZWrite On

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"
                #include "Lighting.cginc"

                #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
                #include "AutoLight.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    SHADOW_COORDS(1)
                    fixed3 ambient : COLOR0;
                    float4 pos : SV_POSITION;
                    float3 worldNormal : NORMAL;
                    half4 worldPosition : TEXCOORD2;
                    float3 viewDir : TEXCOORD3;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Colour;
                float _AmbientLevel;
                float _ScriptAlpha;

                float getAOI(float3 normalVec, float3 lightVec)
                {
                    float incidence = dot(normalize(normalVec), normalize(lightVec));
                    incidence = max(0.0, incidence);
                    return incidence;
                }

                float getSpecular(float3 normalVec, float3 viewDirection, float3 lightVec)
                {
                    float3 surfaceToCamera = float3(0, 0, 0) - viewDirection;

                    float3 h = mul(normalize(surfaceToCamera) + normalize(lightVec), 0.5);
                    float spec = dot(h, normalize(normalVec));
                    return max(0.0, spec);
                }

                float getFresnel(float3 normalVec, float3 viewDirection)
                {
                    return saturate(1 - dot(normalize(normalVec), viewDirection));
                }

                float CellShade(float3 normalVec, float3 viewDirection, float3 lightVec, float shadowDetail)
                {
                    float aoi = getAOI(normalVec, lightVec);
                    //float fresnel = getFresnel(normalVec, viewDirection);
                    float lighting = aoi + getSpecular(normalVec, viewDirection, lightVec);
                    lighting *= 0.5;
                    lighting += _AmbientLevel;
                    lighting *= shadowDetail;
                    lighting = min(1.0, lighting);

                    if (lighting < 0.4) {
                        lighting = 0.35;
                    }
                    else if (lighting < 0.6) {
                        lighting = 0.55;
                    }
                    else if (lighting < 0.9) {
                        lighting = 0.7;
                    }
                    else {
                        lighting = 1.0;
                    }
                    return lighting;
                    //return fresnel;
                }

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.worldNormal = UnityObjectToWorldNormal(v.normal);
                    o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
                    o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPosition));
                    //Put in depth so our outline shader will work on it
                    UNITY_TRANSFER_DEPTH(o.depth);
                    //Now handle the shadows
                    o.ambient = ShadeSH9(half4(o.worldNormal, 1));
                    TRANSFER_SHADOW(o)
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // sample the texture
                    fixed4 col = tex2D(_MainTex, i.uv) * _Colour;
                    fixed shadow = SHADOW_ATTENUATION(i);
                    float cellShade = CellShade(i.worldNormal, i.viewDir, _WorldSpaceLightPos0.xyz, length(shadow * i.ambient));
                    col *= cellShade;
                    col.a = _ScriptAlpha;
                    return col;
                }
                ENDCG
            }
                UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
        }
            Fallback "Diffuse"
}
