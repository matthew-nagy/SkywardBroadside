using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PerlinNoise : MonoBehaviour
{
    public static float scale = 1.0f;
    public static int height = 10;
    public static int width = 10;
    public static int depth = 10;

    static Color[] colours = new Color[width * height * depth];

    [MenuItem("MyScriptStuff/GenPerlin3DAsset")]
    static void CreateTexture3D()
    {
        GenerateTexture();
    }

    static void GenerateTexture()
    {
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        Texture3D texture = new Texture3D(width, height, depth, format, false);
        texture.wrapMode = wrapMode;

        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = x + width * (y + height * z);
                    colours[index] = getNoiseColour(x, y, z);
                }
            }
        }

        texture.SetPixels(colours);
        texture.Apply();
        print("Made an asset!");
        AssetDatabase.CreateAsset(texture, "Assets/Noise.asset");
    }

    static Color getNoiseColour(int x, int y, int z)
    {
        float _x = (float)x / width * scale;
        float _y = (float)y / height * scale;
        float _z = (float)z / depth * scale;

        float sample = PerlinNoise3D(_x, _y, _z);
        return new Color(sample, sample, sample, 1.0f);
    }
    static float PerlinNoise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);

        return (xy + xz + yz + yx + zx + zy) / 6;
    }
}
