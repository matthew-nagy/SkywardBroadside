using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudGen2 : MonoBehaviour
{
    private static int width = 64, height = 64, depth = 64;
    private Material material;
    private Texture3D noise;
    private Texture3D falloff;
    private GameObject[,,] spheres = new GameObject[width, height, depth];

    void Start()
    {
        material = Resources.Load("Materials/Cloud", typeof(Material)) as Material;
        noise = Resources.Load("noise64", typeof(Texture3D)) as Texture3D;
        falloff = Resources.Load("falloff64", typeof(Texture3D)) as Texture3D;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    spheres[i, j, k] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    spheres[i, j, k].GetComponent<MeshRenderer>().material = material;
                    spheres[i, j, k].transform.SetParent(gameObject.transform);
                    spheres[i, j, k].transform.position = new Vector3(i, j, k);

                    float size = noise.GetPixel(i, j, k).r - falloff.GetPixel(i, j, k).r;
                    if (size < 0.1f)
                    {
                        spheres[i, j, k].SetActive(false);
                    }
                    else
                    {
                        size *= 10;
                        spheres[i, j, k].transform.localScale = new Vector3(size, size, size);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
