using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GenerateNoiseTexture : EditorWindow
{
    public static int width = 128;
    public static int height = 128;
    public static int depth = 128;
    public static int octaves = 5;
    public float scale = 1.0f;

    private ComputeShader falloffGenShader;
    public float falloff_sharpness = 32.0f;
    public float falloff_start = 0.65f;

    public string name;
    private static Color[] colours = new Color[width * height * depth];
    private static Color[] falloff_colours = new Color[width * height * depth];

    private Texture3D texture;
    private Texture3D falloff_texture;
    public RenderTexture falloff_renderTexture;



    [MenuItem("Tools/Generate Noise Texture")]
    public static void ShowWindow()
    {
        GetWindow(typeof(GenerateNoiseTexture));
    }

    private void GenerateFalloff()
    {

        falloffGenShader = Resources.Load<ComputeShader>("FalloffGenShader");
        falloff_renderTexture = new RenderTexture(width, height, 0);
        falloff_renderTexture.enableRandomWrite = true;
        falloff_renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        falloff_renderTexture.volumeDepth = depth;
        falloff_renderTexture.Create();
        Debug.Log(width);
        falloffGenShader.SetTexture(0, "Result", falloff_renderTexture);
        falloffGenShader.SetInt("width", width);
        falloffGenShader.SetInt("height", height);
        falloffGenShader.SetInt("depth", depth);
        falloffGenShader.SetFloat("falloff_sharpness", falloff_sharpness);
        falloffGenShader.SetFloat("falloff_start", falloff_start);

        falloffGenShader.Dispatch(0, width / 8, height / 8, depth / 8);
        Debug.Log("Dispatched");

        Save();


        // for (int z = 0; z < depth; z++)
        // {
        //     for (int y = 0; y < height; y++)
        //     {
        //         for (int x = 0; x < width; x++)
        //         {

        //         }
        //     }
        // }

        // TextureFormat format = TextureFormat.RGBA32;
        // TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        // falloff_texture = new Texture3D(width, height, depth, format, false);
        // falloff_texture.wrapMode = wrapMode;

        // int centreX = width / 2;
        // int centreY = height / 2;
        // int centreZ = depth / 2;

        // for (int z = 0; z < depth; z++)
        // {
        //     for (int y = 0; y < height; y++)
        //     {
        //         for (int x = 0; x < width; x++)
        //         {
        //             int index = x + width * (y + height * z);

        //             int dx = Math.Abs(x - centreX);
        //             int dy = Math.Abs(y - centreY);
        //             int dz = Math.Abs(z - centreZ);
        //             double d = Math.Sqrt(dx * dx + dy * dy + dz * dz);

        //             // f(x) = (1/e^a) * (x - b)^3
        //             double fd = (1 / (Math.Pow(2.718, falloff_sharpness))) * Math.Pow(x - falloff_start, 3);
        //             double falloff = Math.Min(Math.Max(fd, 0), 1); //bound between 0 and 1
        //             Debug.Log(falloff);
        //             falloff_colours[index] = new Color((float)falloff, (float)falloff, (float)falloff);
        //         }
        //     }
        // }

        // falloff_texture.SetPixels(colours);
        // falloff_texture.Apply();

        // name = "falloff_" + width + "x" + height + "x" + falloff_start + "_" + falloff_sharpness;
        // AssetDatabase.CreateAsset(falloff_texture, "Assets/" + name + ".asset");
        // Debug.Log(AssetDatabase.GetAssetPath(falloff_texture));
    }

    private void Generate()
    {
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        texture = new Texture3D(width, height, depth, format, false);
        texture.wrapMode = wrapMode;

        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = x + width * (y + height * z);
                    double _x = (double)x / width;
                    double _y = (double)y / height;
                    double _z = (double)z / depth;

                    NoiseS3D.octaves = octaves;

                    double noiseValue = NoiseS3D.NoiseCombinedOctaves(_x, _y, _z);

                    // Color c = new Color((float)noiseValue, (float)noiseValue, (float)noiseValue, noiseValue <= 0.1f ? 0 : 1);
                    Color c = new Color((float)noiseValue, (float)noiseValue, (float)noiseValue);
                    colours[index] = c;
                }
            }
        }

        texture.SetPixels(colours);
        texture.Apply();

    }

    private void SaveAsAsset()
    {
        if (texture)
        {
            name = width + "x" + height + "x" + depth + "_" + octaves;
            AssetDatabase.CreateAsset(texture, "Assets/" + name + ".asset");
            Debug.Log(AssetDatabase.GetAssetPath(texture));
        }
    }

    public void OnGUI()
    {
        width = EditorGUILayout.IntField("Width", width);
        depth = EditorGUILayout.IntField("Depth", depth);
        height = EditorGUILayout.IntField("Height", height);
        octaves = EditorGUILayout.IntField("Octaves", octaves);
        falloff_sharpness = EditorGUILayout.Slider("Falloff Sharpness", falloff_sharpness, 0.0f, 100.0f);
        falloff_start = EditorGUILayout.FloatField("Falloff Start", falloff_start);

        if (GUILayout.Button("Generate Noise Texture"))
        {
            Generate();

            if (texture)
            {
                EditorGUI.PrefixLabel(new Rect(25, 45, 100, 15), 0, new GUIContent("Preview:"));
                EditorGUI.DrawPreviewTexture(new Rect(25, 60, 100, 100), texture);
            }

            SaveAsAsset();
            Debug.Log("Saved");
        }

        if (GUILayout.Button("Generate Falloff"))
        {
            GenerateFalloff();
        }

    }


    //Stolen from: https://answers.unity.com/questions/840983/how-do-i-copy-a-3d-rendertexture-isvolume-true-to.html?childToView=1243556#answer-1243556
    private RenderTexture Copy3DSliceToRenderTexture(RenderTexture source, int layer)
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

    private Texture2D ConvertFromRenderTexture(RenderTexture rt)
    {
        Texture2D output = new Texture2D(width, height);
        RenderTexture.active = rt;
        output.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        output.Apply();
        return output;
    }

    private void Save()
    {
        Texture3D export = new Texture3D(width, height, depth, TextureFormat.ARGB32, false);
        RenderTexture selectedRenderTexture = falloff_renderTexture;
        // if (useA)
        //     selectedRenderTexture = renderA;
        // else
        //     selectedRenderTexture = renderB;

        RenderTexture[] layers = new RenderTexture[depth];
        for (int i = 0; i < depth; i++)
            layers[i] = Copy3DSliceToRenderTexture(selectedRenderTexture, i);

        Texture2D[] finalSlices = new Texture2D[depth];
        for (int i = 0; i < depth; i++)
            finalSlices[i] = ConvertFromRenderTexture(layers[i]);

        Texture3D output = new Texture3D(width, height, depth, TextureFormat.ARGB32, true);
        output.filterMode = FilterMode.Trilinear;
        Color[] outputPixels = output.GetPixels();

        for (int k = 0; k < depth; k++)
        {
            Color[] layerPixels = finalSlices[k].GetPixels();
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    outputPixels[i + j * depth + k * width * height] = layerPixels[i + j * height];
                }
        }

        output.SetPixels(outputPixels);
        output.Apply();

        AssetDatabase.CreateAsset(output, "Assets/" + "falloff_" + width + "x" + height + "x" + depth + "_" + falloff_sharpness + "x" + falloff_start + ".asset");
    }
}

