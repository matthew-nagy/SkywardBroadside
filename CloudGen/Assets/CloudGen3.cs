using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudGen3 : MonoBehaviour
{
    public int cloud_width = 10;
    public int cloud_height = 10;
    public int cloud_depth = 10;

    public float quad_width = 0.5f;
    public float quad_height = 0.5f;

    MeshRenderer meshRenderer;
    Mesh mesh;

    void Start()
    {
        if (cloud_width * cloud_height * cloud_depth * 4 > 65535)
        {
            Debug.LogWarning("Cloud is bigger than vertex limit!!!");
        }
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        mesh = new Mesh();

        Vector3[] vertices = new Vector3[cloud_width * cloud_height * cloud_depth * 4];
        int[] triangles = new int[cloud_width * cloud_height * cloud_depth * 6];
        Vector3[] normals = new Vector3[cloud_width * cloud_height * cloud_depth * 4];
        Vector2[] uv = new Vector2[cloud_width * cloud_height * cloud_depth * 4];
        Color[] colors = new Color[cloud_width * cloud_height * cloud_depth * 4];

        int vertexCount = 0;
        int triangleCount = 0;
        int normalCount = 0;
        int uvCount = 0;
        int colorCount = 0;

        for (int x = 0; x < cloud_width; x++)
        {
            for (int y = 0; y < cloud_height; y++)
            {
                for (int z = 0; z < cloud_depth; z++)
                {

                    vertices[vertexCount + 0] = new Vector3(x, y, z);
                    vertices[vertexCount + 1] = new Vector3(x + quad_width, y, z);
                    vertices[vertexCount + 2] = new Vector3(x, y + quad_height, z);
                    vertices[vertexCount + 3] = new Vector3(x + quad_width, y + quad_height, z);

                    // lower left triangle
                    triangles[triangleCount + 0] = vertexCount + 0;
                    triangles[triangleCount + 1] = vertexCount + 2;
                    triangles[triangleCount + 2] = vertexCount + 1;
                    // upper right triangle
                    triangles[triangleCount + 3] = vertexCount + 2;
                    triangles[triangleCount + 4] = vertexCount + 3;
                    triangles[triangleCount + 5] = vertexCount + 1;


                    normals[normalCount + 0] = -Vector3.forward;
                    normals[normalCount + 1] = -Vector3.forward;
                    normals[normalCount + 2] = -Vector3.forward;
                    normals[normalCount + 3] = -Vector3.forward;

                    uv[uvCount + 0] = new Vector2(0, 0);
                    uv[uvCount + 1] = new Vector2(1, 0);
                    uv[uvCount + 2] = new Vector2(0, 1);
                    uv[uvCount + 3] = new Vector2(1, 1);

                    colors[colorCount + 0] = new Color((float)x / (float)cloud_width, (float)y / cloud_height, (float)z / cloud_depth);
                    colors[colorCount + 1] = new Color((float)x / (float)cloud_width, (float)y / cloud_height, (float)z / cloud_depth);
                    colors[colorCount + 2] = new Color((float)x / (float)cloud_width, (float)y / cloud_height, (float)z / cloud_depth);
                    colors[colorCount + 3] = new Color((float)x / (float)cloud_width, (float)y / cloud_height, (float)z / cloud_depth);

                    vertexCount += 4;
                    triangleCount += 6;
                    normalCount += 4;
                    uvCount += 4;
                    colorCount += 4;

                }
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.colors = colors;
        // mesh.SetColors(colors);

        meshFilter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
