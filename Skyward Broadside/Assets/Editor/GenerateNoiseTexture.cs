#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GenerateNoiseTexture : EditorWindow
{
    public static int width = 5;
    public static int height = 5;
    public static int depth = 5;
    public static int octaves = 5;

    public int editorNoiseSeed = 12345;

    public float falloff_sharpness = 32.0f;
    public float falloff_start = 0.65f;

    private static Color[] colours = new Color[width * height * depth];

    private Texture3D texture;
    private Texture3D falloff_texture;



    [MenuItem("Tools/Generate Noise Texture")]
    public static void ShowWindow()
    {
        GetWindow(typeof(GenerateNoiseTexture));
    }


    public void OnGUI()
    {
        width = EditorGUILayout.IntField("Width", width);
        depth = EditorGUILayout.IntField("Depth", depth);
        height = EditorGUILayout.IntField("Height", height);
        editorNoiseSeed = EditorGUILayout.IntField("Noise Seed", editorNoiseSeed);

        octaves = EditorGUILayout.IntField("Octaves", octaves);
        falloff_sharpness = EditorGUILayout.Slider("Falloff Sharpness", falloff_sharpness, 0.0f, 100.0f);
        falloff_start = EditorGUILayout.FloatField("Falloff Start", falloff_start);

        if (GUILayout.Button("Generate Noise Texture"))
        {
            texture = CloudUtils.Generate(width, height, depth, octaves, editorNoiseSeed);
            CloudUtils.SaveAsAsset(texture, width + "x" + height + "x" + depth + "_" + octaves + "Seed_" + editorNoiseSeed);
            Debug.Log("Saved");
        }

        if (GUILayout.Button("Generate Falloff"))
        {
            falloff_texture = CloudUtils.GenerateFalloff(width, height, depth, falloff_sharpness, falloff_start);
            CloudUtils.SaveAsAsset(falloff_texture, "Assets/" + "falloff_" + width + "x" + height + "x" + depth + "_" + falloff_sharpness + "x" + falloff_start);
        }

    }
}

#endif
