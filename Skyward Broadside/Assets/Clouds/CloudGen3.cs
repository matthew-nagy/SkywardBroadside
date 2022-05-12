using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CloudGen3 : MonoBehaviour
{
    public int cloud_width = 10;
    public int cloud_height = 10;
    public int cloud_depth = 10;

    public Color cloud_colour = new Color(1f, 0.9f, 0.9f, 0.7f);
    private int maxDimension = 10;
    private Vector3 dimensionRatios;


    private float cloud_scale = 10;
    public float bubble_size = 1.0f;

    private float phase = 0.0f;

    private float phaseAdvanceFreq = 0.1f;
    public float phaseAdvanceAmount = 0.01f;

    private float quad_width = 1.0f;
    private float quad_height = 1.0f;

    public Vector3 shaderScale;

    private Material myMat;

    MeshRenderer meshRenderer;
    Mesh mesh;

    // Generates a quad to be used in the clud mesh
    void MakeCloudQuad(int x, int y, int z, int vertexCount, int triangleCount, int normalCount, int uvCount, int colorCount, Vector3[] vertices, int[] triangles, Vector3[] normals, Vector2[] uv, Color[] colors)
    {
        // Calculate posions of the corners of the quad relative to the quad position
        vertices[vertexCount + 0] = new Vector3(x, y, z);
        vertices[vertexCount + 1] = new Vector3(x + quad_width, y, z);
        vertices[vertexCount + 2] = new Vector3(x, y + quad_height, z);
        vertices[vertexCount + 3] = new Vector3(x + quad_width, y + quad_height, z);


        // Quads are made of two triangles
        // lower left triangle
        triangles[triangleCount + 0] = vertexCount + 0;
        triangles[triangleCount + 1] = vertexCount + 2;
        triangles[triangleCount + 2] = vertexCount + 1;
        // upper right triangle
        triangles[triangleCount + 3] = vertexCount + 2;
        triangles[triangleCount + 4] = vertexCount + 3;
        triangles[triangleCount + 5] = vertexCount + 1;

        // direction quad will face
        normals[normalCount + 0] = -Vector3.forward;
        normals[normalCount + 1] = -Vector3.forward;
        normals[normalCount + 2] = -Vector3.forward;
        normals[normalCount + 3] = -Vector3.forward;

        uv[uvCount + 0] = new Vector2(0, 0);
        uv[uvCount + 1] = new Vector2(1, 0);
        uv[uvCount + 2] = new Vector2(0, 1);
        uv[uvCount + 3] = new Vector2(1, 1);

        // Colours encode position inside the quad, which is why they are scaled from 0 to 1 inside the mesh
        colors[colorCount + 0] = new Color((float)x / (float)cloud_width, (float)y / cloud_height, (float)z / cloud_depth, 1.0f);
        colors[colorCount + 1] = new Color((float)x / (float)cloud_width, (float)y / cloud_height, (float)z / cloud_depth, 1.0f);
        colors[colorCount + 2] = new Color((float)x / (float)cloud_width, (float)y / cloud_height, (float)z / cloud_depth, 1.0f);
        colors[colorCount + 3] = new Color((float)x / (float)cloud_width, (float)y / cloud_height, (float)z / cloud_depth, 1.0f);
    }

    void Start()
    {
        string id = cloud_width + "_" + cloud_height + "_" + cloud_depth;

        //For deployment make sure cloud shader is in the always loaded shaders list
        myMat = new Material(Shader.Find("Unlit/CloudShader"));

        Texture3D noiseTexture = Resources.Load<Texture3D>("CloudTextures/" + id + "_noise");
        Texture3D falloffTexture = Resources.Load<Texture3D>("CloudTextures/" + id + "_falloff");

#if UNITY_EDITOR //Only try to generate assets in editor mode - never in deployment (they should already be there)
        if (!noiseTexture)
        {
            //If none found, generate the gloud texture
            noiseTexture = CloudUtils.Generate(cloud_width, cloud_height, cloud_depth, 5, 12345);
            CloudUtils.SaveAsAsset(noiseTexture, id + "_noise");
        }

        if (!falloffTexture)
        {
            //If none found generate the falloff map
            falloffTexture = CloudUtils.GenerateFalloff(cloud_width, cloud_height, cloud_depth, 32, 0.65f);
            CloudUtils.SaveAsAsset(falloffTexture, id + "_falloff");
        }
#endif

        if (!noiseTexture || !falloffTexture)
        {
            Debug.LogWarning("CLOUDS MISSING TEXTURES AH BALLS");
        }

        ComputeMaxDimension();
        ComputeDimensionRatios();

        cloud_scale = maxDimension;
        myMat.SetTexture("_Noisemap", noiseTexture);
        myMat.SetTexture("_Falloffmap", falloffTexture);
        myMat.SetFloat("_Scale", cloud_scale);
        myMat.SetFloat("_BubbleSize", bubble_size);
        myMat.SetVector("_DimensionRatios", dimensionRatios);
        myMat.SetVector("_CloudPos", gameObject.transform.position);
        myMat.SetVector("_CloudColour", cloud_colour);
        InvokeRepeating("AdvancePhase", 0.0f, phaseAdvanceFreq); //Phase makes the quads of the clouds float around, looks nice up close but imperceptible in game so unused

        if (cloud_width * cloud_height * cloud_depth * 4 > 65535)
        {
            Debug.LogWarning("Cloud is bigger than vertex limit!!!");
        }

        // Make the cloud smaller until it fits in the vertex limit
        while (cloud_width * cloud_height * cloud_depth * 4 > 65535)
        {
            cloud_width = (int)((float)(cloud_width) * 0.95);
            cloud_height = (int)((float)(cloud_height) * 0.95);
            cloud_depth = (int)((float)(cloud_depth) * 0.95);
        }

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = myMat;

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        mesh = new Mesh();

        //initialise containers for all the mesh attributes
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

        // This generates the cloud mesh which is then shaped by shaders
        for (int x = 0; x < cloud_width; x++)
        {
            for (int y = 0; y < cloud_height; y++)
            {
                for (int z = 0; z < cloud_depth; z++)
                {

                    MakeCloudQuad(x, y, z, vertexCount, triangleCount, normalCount, uvCount, colorCount, vertices, triangles, normals, uv, colors);

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
        shaderScale = new Vector3(cloud_width, cloud_height, cloud_depth);
    }

    // Update is called once per frame
    void Update()
    {

        ComputeMaxDimension();
        ComputeDimensionRatios();

        cloud_scale = maxDimension;

        // Set values again in case anything has changed
        myMat.SetFloat("_Scale", cloud_scale);
        myMat.SetFloat("_BubbleSize", bubble_size);
        myMat.SetVector("_DimensionRatios", dimensionRatios);
        myMat.SetVector("_CloudPos", gameObject.transform.position);
        myMat.SetFloat("_Phase", phase);
        myMat.SetVector("_CloudColour", cloud_colour);

    }

    //Gives the largest size in any dimention
    private void ComputeMaxDimension()
    {
        maxDimension = Math.Max(cloud_width, Math.Max(cloud_height, cloud_depth));
    }

    //Dimension rations are used so that the cloud scales properly in all dimensions. 
    private void ComputeDimensionRatios()
    {
        dimensionRatios = new Vector3((float)cloud_width / maxDimension, (float)cloud_height / maxDimension, (float)cloud_depth / maxDimension);
    }

    //Phase makes the quads of the clouds float around, looks nice up close but imperceptible in game so unused
    private void AdvancePhase()
    {
        phase += phaseAdvanceAmount;
    }
}
