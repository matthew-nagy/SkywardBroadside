#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CloudUtils
{

    private static ComputeShader falloffGenShader;

    public static Texture3D GenerateFalloff(int w, int h, int d, float sharpness, float start)
    {
        RenderTexture falloff_renderTexture;

        falloffGenShader = Resources.Load<ComputeShader>("FalloffGenShader");
        falloff_renderTexture = new RenderTexture(w, h, 0);
        falloff_renderTexture.enableRandomWrite = true;
        falloff_renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        falloff_renderTexture.volumeDepth = d;
        falloff_renderTexture.Create();
        falloffGenShader.SetTexture(0, "Result", falloff_renderTexture);
        falloffGenShader.SetInt("width", w);
        falloffGenShader.SetInt("height", h);
        falloffGenShader.SetInt("depth", d);
        falloffGenShader.SetFloat("falloff_sharpness", sharpness);
        falloffGenShader.SetFloat("falloff_start", start);

        int shaderCountX = (int)Math.Ceiling((float)w / 8.0f);
        int shaderCountY = (int)Math.Ceiling((float)h / 8.0f);
        int shaderCountZ = (int)Math.Ceiling((float)d / 8.0f);

        falloffGenShader.Dispatch(0, shaderCountX, shaderCountY, shaderCountZ);
        Debug.Log("Dispatched");
        Texture3D falloff_tex = ConvertRenderTextureToTexture3D(falloff_renderTexture, w, h, d);

        return falloff_tex;
    }


    // Generate the 3D noise texture using NoiseS3D library for the actual noise function (just because its faster than Math.noise)
    public static Texture3D Generate(int w, int h, int d, int octaves, int noiseSeed)
    {
        Color[] colours = new Color[w * h * d];
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        Texture3D _texture = new Texture3D(w, h, d, format, false);
        _texture.wrapMode = wrapMode;

        NoiseS3D.seed = noiseSeed;

        for (int z = 0; z < d; z++)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int index = x + (y * w) + (z * w * h);

                    // Needs to be a value between 0 and 1 for noise to work, so normalise with width/height/depth
                    double _x = (double)x / w;
                    double _y = (double)y / h;
                    double _z = (double)z / d;

                    NoiseS3D.octaves = octaves;

                    double noiseValue = NoiseS3D.NoiseCombinedOctaves(_x, _y, _z);

                    Color c = new Color((float)noiseValue, (float)noiseValue, (float)noiseValue, (float)noiseValue);
                    colours[index] = c;
                }
            }
        }

        // Save the texture

        _texture.SetPixels(colours);
        _texture.Apply();
        return _texture;
    }

    public static void SaveAsAsset(Texture3D tex, string name)
    {
        if (tex)
        {
            AssetDatabase.CreateAsset(tex, "Assets/Resources/CloudTextures/" + name + ".asset");
            Debug.Log(AssetDatabase.GetAssetPath(tex));
        }
    }

    /*
        A render texture cant be converted to a  Texture3D, but we need to pass that to the shader, so we convert it slice by slice then combine the slices
        The unity API is just straight-up missing this feature for some reason.... it supports converting 2d tex to 2d rendertex, but not 3d to 3d 

        so it has to be done manually
    */

    //Adapted from: https://answers.unity.com/questions/840983/how-do-i-copy-a-3d-rendertexture-isvolume-true-to.html?childToView=1243556#answer-1243556
    private static RenderTexture Copy3DSliceToRenderTexture(RenderTexture source, int layer, int width, int height)
    {

        RenderTexture render = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        render.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        render.enableRandomWrite = true;
        render.wrapMode = TextureWrapMode.Clamp;
        render.Create();

        ComputeShader slicer = Resources.Load<ComputeShader>("Slicer");
        int kernelIndex = slicer.FindKernel("CSMain");
        slicer.SetTexture(kernelIndex, "voxels", source);
        slicer.SetInt("layer", layer);
        slicer.SetTexture(kernelIndex, "Result", render);
        slicer.Dispatch(kernelIndex, width, height, 1);

        return render;
    }

    private static Texture2D ConvertFromRenderTexture(RenderTexture rt, int width, int height)
    {
        Texture2D output = new Texture2D(width, height);
        RenderTexture.active = rt;
        output.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        output.Apply();
        return output;
    }

    private static Texture3D ConvertRenderTextureToTexture3D(RenderTexture input, int w, int h, int d)
    {
        Texture3D export = new Texture3D(w, h, d, TextureFormat.ARGB32, false);
        RenderTexture selectedRenderTexture = input;

        RenderTexture[] layers = new RenderTexture[d];
        for (int i = 0; i < d; i++)
            layers[i] = Copy3DSliceToRenderTexture(selectedRenderTexture, i, w, h);

        Texture2D[] finalSlices = new Texture2D[d];
        for (int i = 0; i < d; i++)
            finalSlices[i] = ConvertFromRenderTexture(layers[i], w, h);

        Texture3D output = new Texture3D(w, h, d, TextureFormat.ARGB32, true);
        output.filterMode = FilterMode.Trilinear;
        Color[] outputPixels = output.GetPixels();

        for (int k = 0; k < d; k++)
        {
            Color[] layerPixels = finalSlices[k].GetPixels();

            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    Color c = layerPixels[i + (j * w)];
                    int index = i + (j * w) + (k * w * h);
                    outputPixels[index] = c;
                }
            }
        }

        output.SetPixels(outputPixels);
        output.Apply();

        return output;
    }

}

#endif