Shader "Hidden/Custom/CellShade"
{
    HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

    float _BlackCuttof;
    float _WhiteCuttof;
    float _MinRange;
    float _BlendRange;
    float _HighLuminanceVal;
    float _LowLuminanceVal;

    float4 Frag(VaryingsDefault i) : SV_Target
    {
        float4 originalColour = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
        float3 colourRGB = originalColour.rgb;
        
        float3x3 YcRcRToRGB = { 1.0, 0.0, 1.402,
                                1.0, -0.344136, -0.714136,
                                1.0, 1.772, 0.0 };
        //Inverse of the above from matrix.reshish.com
        float3x3 RGBToYcRcB = { 0.29899994434761785177 , 0.58700012599191222428 , 0.11399992966046992394,
                                - 0.1687358602413193295 , -0.3312641794536750702 , 0.5000000396949943996,
                                0.5000000396949943996, -0.4186876790241884624, 	-0.081312360670805937191 };

        //Get luminance in YcRcB colour space
        float3 YcRcB = mul(colourRGB,RGBToYcRcB);
        if (YcRcB.y > _MinRange) {
            YcRcB.y = _HighLuminanceVal;
        }
        else {
            YcRcB.y = _LowLuminanceVal;
        }
        float3 finalColour = mul(YcRcB,YcRcRToRGB);
        float4 colour = originalColour;
        colour.r = finalColour.r;
        colour.g = finalColour.g;
        colour.b = finalColour.b;

        return colour;
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