using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class ReloadRegister : MonoBehaviour
{
    [System.Serializable]
    public struct TeamToMat
    {
        public Material material;
        public string teamName;
    }

    public List<TeamToMat> teamMaterials;
    public TeamData.Team myTeam;

    public int meshCircleResolution = 30;
    public int meshHeightResolution = 10;

    [Tooltip("What game object's materials need to be set with the bases team")]
    public List<GameObject> materialSettingObjects;

    public Material friendlyReloadMaterial;
    public Material reloadRadiusMaterial;

    Mesh reloadFieldMesh;

    private float reloadRadius;

    private void Start()
    {
        string myTeamName = TeamData.TeamToString(myTeam);
        foreach (TeamToMat ttm in teamMaterials)
        {
            if (myTeamName == ttm.teamName)
            {
                foreach (GameObject go in materialSettingObjects)
                {
                    go.GetComponent<Renderer>().material = ttm.material;
                }
                break;
            }
        }
        reloadRadius = GetComponent<SphereCollider>().radius * transform.localScale.x;
        Invoke(nameof(Setup), 1f);
    }

    //Doesnt work right now
    void Setup()
    {
        if (Blackboard.playerPhotonHub.myTeam == myTeam)
        {
            Debug.Log("Making the mesh");
            CreateDisplayMesh();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ship") && other.gameObject.GetComponent<PlayerController>().myTeam == myTeam)
        {
            other.gameObject.GetComponent<PlayerController>().resupply = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ship") && other.gameObject.GetComponent<PlayerController>().myTeam == myTeam)
        {
            other.gameObject.GetComponent<PlayerController>().resupply = false;
        }
    }

    void AddVertCircle(float displacement, List<Vector3> verts, List<Vector3> norms)
    {
        Vector3 center = transform.position + new Vector3(0f, displacement, 0f);
        float r = Mathf.Sqrt((reloadRadius * reloadRadius) - (displacement * displacement));
        float degreesPerPoint = 360.0f / meshCircleResolution;

        for(int i = 0; i < meshCircleResolution; i++)
        {
            verts.Add(center + r * (Quaternion.Euler(0f, degreesPerPoint * i, 0f) * Vector3.forward));
            norms.Add((verts[i] - transform.position).normalized);
        }
    }

    void CreateDisplayMesh()
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        float heightChunk = reloadRadius / meshHeightResolution;
        for(int i = meshHeightResolution * -1; i < (meshHeightResolution + 2); i += 2)
        {
            AddVertCircle(i * heightChunk, verts, norms);
        }

        List<int> tris = new List<int>();
        Debug.Log("Making " + verts.Count + " spheres");
        for(int i = 0; i < verts.Count - (meshCircleResolution + 1); i++)
        {
            tris.Add(i);
            tris.Add(i + 1);
            tris.Add(i + meshCircleResolution);

            tris.Add(i + 1);
            tris.Add(i + meshCircleResolution);
            tris.Add(i + meshCircleResolution + 1);

            tris.Add(i + meshCircleResolution);
            tris.Add(i + 1);
            tris.Add(i);

            tris.Add(i + meshCircleResolution + 1);
            tris.Add(i + meshCircleResolution);
            tris.Add(i + 1);

            Instantiate(Resources.Load("DebugSphere"), verts[i], Quaternion.identity);
        }

        Vector3[] v = new Vector3[verts.Count];
        Vector3[] n = new Vector3[norms.Count];
        int[] t = new int[tris.Count];

        verts.CopyTo(v);
        norms.CopyTo(n);
        tris.CopyTo(t);

        Mesh myMesh = new Mesh();
        myMesh.vertices = v;
        myMesh.normals = n;
        myMesh.triangles = t;

        GameObject shellRenderer = new GameObject();
        MeshFilter mf = shellRenderer.AddComponent<MeshFilter>();
        MeshRenderer mr = shellRenderer.AddComponent<MeshRenderer>();

        mf.sharedMesh = myMesh;
        mr.material = reloadRadiusMaterial;
        Color teamColour = TeamData.TeamToColour(myTeam);
        //mr.material.SetVector("Colour", new Vector4(teamColour.r, teamColour.g, teamColour.b, 0.6f));
        
        //Just set it to false for now
        //mr.enabled = false;
    }
}
