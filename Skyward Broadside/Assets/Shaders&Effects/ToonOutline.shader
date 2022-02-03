Shader "Hidden/Custom/ToonOutline"
{
    HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
        float4 _MainTex_TexelSize;
        float _Scale;
        float _EdgeScaler;
        float _DepthThreshold;

        float GetDepthAt(float2 pos)
        {
            return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, pos).r;
        }

        float4 Frag(VaryingsDefault i) : SV_Target
        {
            float2 tc = i.texcoord;
            float2 texSize = float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y);
            float halfScale = _Scale * 0.5;
            float2 bottomLeft = tc - texSize * halfScale;
            float2 topLeft = tc + float2(texSize.x * halfScale * -1, texSize.y * halfScale);
            float2 topRight = tc + texSize * halfScale;
            float2 bottomRight = tc + float2(texSize.x * halfScale, texSize.y * halfScale * -1);

            float blDepth = GetDepthAt(bottomLeft);
            float tlDepth = GetDepthAt(topLeft);
            float trDepth = GetDepthAt(topRight);
            float brDepth = GetDepthAt(bottomRight);


            float crossDepth1 = abs(tlDepth - brDepth);
            float crossDepth2 = abs(trDepth - blDepth);
            float fullDepth = sqrt(pow(crossDepth1,2) + pow(crossDepth2,2)) * _EdgeScaler;

            if (fullDepth < _DepthThreshold) {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            }
            else {
                return float4(0, 0, 0, 1);
            }
        }

        ENDHLSL

        SubShader
    {
        Cull Off ZWrite Off ZTest Always

            Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }
    }
}