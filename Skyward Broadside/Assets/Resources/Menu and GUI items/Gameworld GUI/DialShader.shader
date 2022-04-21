Shader "Unlit/DialShader"
{
    Properties
    {
        _Colour("Colour", Vector) = (1.0, 0.0, 0.0, 5.0)
        _Center("Center uv", Vector) = (0.5, 0.5, 0.0, 0.0)
        _AngleRange("Range of angles", float) = 0.7
        _FullProportion("Proportion full", Range(0, 1)) = 1.0
        _Radius("UV Radius", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
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
                float4 vertex : SV_POSITION;
            };

            float4 _Colour;
            float4 _Center;
            float _AngleRange;
            float _FullProportion;
            float _Radius;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Colour;

            float2 uvPos = i.uv.xy;
            float2 centerPos = _Center.xy;

            float propFull = _FullProportion;

            float maxAngle = _AngleRange * propFull;
            float2 vec = uvPos - centerPos;

            float myRadius = length(vec);
            float myAngle = acos( dot(vec, float2(0.0,1.0)) / myRadius);
            if (i.uv.x < 0.5) {
                myAngle += 3.141592;
            }
            else {
                myAngle = 3.141592 - myAngle;
            }

            if (myAngle > maxAngle || myRadius  > _Radius) {
                col.a = 0.0;
            }

                return col;
            }
            ENDCG
        }
    }
}
