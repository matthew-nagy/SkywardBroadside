Shader "Unlit/NewUnlitShader"
{
    //See stagnant water shader for more documentation
    Properties
    {
        _WaterTexture ("Texture", 2D) = "white" {}
        _DisplacementTexture("Texture", 2D) = "white" {}
        _DisplacementStrength("Displacement strength", float) = 0.05
        _ScrollVector1("Scroll direction 1", Vector) = (0.25, 0.0, 0.0, 0.0)
        _ScrollVector2("Scroll direction 2", Vector) = (-0.1, 0.15, 0.0, 0.0)
        _ScrollSpeedFactor("Scroll speed factor", float) = 0.3

        _FlowSpeed("Water flow", Vector) = (0.1, 0.05, 0.0, 0.0)
        _Scale("Water scale", float) = 1.0
        _Alpha("Transparency", Range(0,1)) = 1.0

        _WaterRoughness("Water roughness", float ) = 3.0
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _WaterTexture;
            float4 _WaterTexture_ST;
            sampler2D _DisplacementTexture;
            float4 _DisplacementTexture_ST;
            float _DisplacementStrength;
            float4 _ScrollVector1;
            float4 _ScrollVector2;
            float _ScrollSpeedFactor;

            float4 _FlowSpeed;
            float _Scale;
            float _Alpha;

            float _WaterRoughness;


            v2f vert (appdata v)
            {
                v2f o;

                float x = v.vertex.x + _Time.x;
                float z = v.vertex.z + _Time.x;
                
                float4 vertToTransform = v.vertex;

                vertToTransform.y += (sin(x) + sin(2.7 * z + 2.3) + sin(1.9 * x - 0.7) + sin(5.3 * z + 6.6)) * (_WaterRoughness / 4.0);

                o.vertex = UnityObjectToClipPos(vertToTransform);
                o.uv = TRANSFORM_TEX(v.uv, _WaterTexture);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 samplePosition = i.uv;
                //Now scale it
                samplePosition *= _Scale;

                //Add flow
                samplePosition += _FlowSpeed * _Time.x;

                //Add displacement maps
                float2 dispUV1 = i.uv * _Scale + _ScrollVector1.xy * _Time.x * _ScrollSpeedFactor;
                float2 dispUV2 = i.uv * _Scale + _ScrollVector2.xy * _Time.x * _ScrollSpeedFactor;
                float displacementFactor = tex2D(_DisplacementTexture, dispUV1).r + tex2D(_DisplacementTexture, dispUV2);
                //We now have a value -1 <-> 1
                displacementFactor -= 1;
                samplePosition += float2(1.0, 1.0) * displacementFactor * _DisplacementStrength;

                fixed4 col = tex2D(_WaterTexture, samplePosition);
                col.a *= _Alpha;
                return col;
            }
            ENDCG
        }
    }
}
