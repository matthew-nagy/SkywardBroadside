Shader "Unlit/CloudShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Noisemap("Noise Map", 3D) = "" {}
        _Falloffmap("Falloff Map", 3D) = "" {}
        _Scale("Cloud_scale", float) = 1
        _BubbleSize("Bubble size", float) = 1
        _DimensionRatios("Dimension Ratios", Vector) = (1,1,1)
        _CloudPos("Cloud Position",Vector) = (0,0,0)
        _CloudColour("Cloud Colour",Vector) = (1,1,1,0.7)
        _Phase("Phase",float) = 0
    }
        SubShader
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
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
                sampler3D _Noisemap;
                sampler3D _Falloffmap;

                float4 _MainTex_ST;
                float _Scale;
                float _BubbleSize;
                float _Phase;
                Vector _DimensionRatios;
                Vector _CloudPos;
                Vector _CloudColour;


            v2f vert (appdata v)
            {
                
                //Handle all noise based values
                float4 base_noise = tex3Dlod (_Noisemap, v.colour); // Raw noise before combined with falloff, colour encodes position in texture
                float4 falloff =  tex3Dlod (_Falloffmap, v.colour); // colour channels used to encode position
                float noise = base_noise.x * (1 - falloff.x); //Noise combined with falloff to create cloud shape
                float3 offset = (falloff.gba - 0.5) / 3; //position offset amount is encoded in falloff map. -0.5 to centre it
                offset += float3(0, sin(2 * 3.14 * _Phase + falloff.g) / 100,0); //Add some sin waves to the positions to pulsate indivitual quads
                
                v2f o;

                // View transforms
                float4 world_origin = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1));
                float4 view_origin = float4(UnityObjectToViewPos(float3(0, 0, 0)), 1);

                //Get the position encoded in the colour channels
                float4 colourPos = float4((v.colour.rgb + offset) * _Scale * _DimensionRatios, v.vertex.w);

                //Convert that position to view space, in front of the camera
                
                float4 colourPos = float4((v.colour.rgb + offset) * _Scale * _DimensionRatios, v.vertex.w);
                float4 world_pos = mul(UNITY_MATRIX_M, colourPos);
                float4 view_pos = mul(UNITY_MATRIX_V, world_pos);

                //If the alpha is set to 0, there is no bubble
                float bubbleScale = _BubbleSize * noise * v.colour.a;
                //Move x,y based on the uv positions of the 
                view_pos += float4(v.uv * bubbleScale, 0.0, 0.0);

                // Make sure the clip pos doesnt go offscreen while some of the cloud is on sceen - or unity will make it dissapear!
                float4 clip_pos = mul(UNITY_MATRIX_P, view_pos);

                o.vertex = clip_pos;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }


            // This colours the cloud (actually colour, not colour encoding positon this time) based on the colour set in the editor
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _CloudColour; //float4(1, 0.9, 0.8, 0.5);; //tex2D(_MainTex, i.uv);
            
                // col.r = 1.0;
                return col;
            }

            ENDCG
        }
    }
}

