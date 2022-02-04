using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudGen : MonoBehaviour
{

    public float scale = 1.0f;
    public float noiseThreshold = 0.5f;
    public int height = 10;
    public int width = 10;
    public int depth = 10;
    public int octaves = 3;
    public float density = 0.5f;

    public float floatSpeed = 0.01f;
    private Vector3 direction;
    private Material cloudMaterial;

    void Start()
    {
        cloudMaterial = Resources.Load("Materials/Cloud", typeof(Material)) as Material;
        direction = new Vector3(1, 0, 0);
        GenerateCloud();
    }

    void Update()
    {
        foreach (Transform child in gameObject.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        gameObject.transform.position += direction * floatSpeed;
        GenerateCloud();
    }

    public void GenerateCloud()
    {
        float initialZ = gameObject.transform.position[2];
        float initialY = gameObject.transform.position[1];
        float initialX = gameObject.transform.position[0];

        for (float z = initialZ; z < depth + initialZ; z += density)
        {
            for (float y = initialY; y < height + initialY; y += density)
            {
                for (float x = initialX; x < width + initialX; x += density)
                {
                    double _x = (double)x / width * scale;
                    double _y = (double)y / height * scale;
                    double _z = (double)z / depth * scale;

                    NoiseS3D.octaves = octaves;

                    double noiseValue = NoiseS3D.NoiseCombinedOctaves(_x, _y, _z);

                    if (noiseValue < noiseThreshold) continue;

                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.GetComponent<MeshRenderer>().material = cloudMaterial;
                    sphere.transform.SetParent(gameObject.transform);
                    sphere.transform.position = new Vector3(x, y, z);
                    sphere.transform.localScale = new Vector3((float)noiseValue, (float)noiseValue, (float)noiseValue);
                }
            }
        }
    }
}
