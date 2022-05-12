using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

//Script used by reload balloons to tell ships it can reload
public class ReloadRegister : MonoBehaviour
{
    //Materials for different teams
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

    //Nothing works but this may
    public Material friendlyReloadMaterial;
    public Material reloadRadiusMaterial;

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

        if (myTeam == TeamData.Team.Purple)
        {
            Blackboard.purpleReloadObjects.Add(gameObject);
        }
        else if (myTeam == TeamData.Team.Yellow)
        {
            Blackboard.yellowReloadObjects.Add(gameObject);
        }
        reloadRadius = GetComponent<SphereCollider>().radius * transform.localScale.x;
        //Invoke(nameof(Setup), 1f);
    }

    //Called after some photon stuff has been set up
    public void Setup()
    {
        CreateDisplayMesh();
    }

    //Tell the player they can resuply if they are on the right team in the trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ship") && other.gameObject.GetComponent<PlayerController>().myTeam == myTeam)
        {
            other.gameObject.GetComponent<PlayerController>().resupply = true;
        }
    }

    //When you leave the trigger, stop reloading
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ship") && other.gameObject.GetComponent<PlayerController>().myTeam == myTeam)
        {
            other.gameObject.GetComponent<PlayerController>().resupply = false;
        }
    }

    #region HullSetup
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
        List<int> trisInterior = new List<int>();
        for(int i = 0; i < verts.Count - (meshCircleResolution + 1); i++)
        {
            tris.Add(i);
            tris.Add(i + 1);
            tris.Add(i + meshCircleResolution);

            tris.Add(i + 1);
            tris.Add(i + meshCircleResolution);
            tris.Add(i + meshCircleResolution + 1);

            trisInterior.Add(i + meshCircleResolution);
            trisInterior.Add(i + 1);
            trisInterior.Add(i);

            trisInterior.Add(i + meshCircleResolution + 1);
            trisInterior.Add(i + meshCircleResolution);
            trisInterior.Add(i + 1);
        }

        Vector3[] v = new Vector3[verts.Count];
        Vector3[] n = new Vector3[norms.Count];
        int[] t = new int[tris.Count];
        int[] ti = new int[tris.Count];

        verts.CopyTo(v);
        norms.CopyTo(n);
        tris.CopyTo(t);
        trisInterior.CopyTo(ti);

        Mesh myMesh = new Mesh();
        myMesh.vertices = v;
        myMesh.normals = n;
        myMesh.triangles = t;

        Mesh myMesh2 = new Mesh();
        myMesh2.vertices = v;
        myMesh2.normals = n;
        myMesh2.triangles = t;
        
        GameObject shellRenderer = new GameObject();
        GameObject shellRendererInside = new GameObject();

        Color teamColour = TeamData.TeamToColour(myTeam);
        Vector4 colourAsVector = new Vector4(teamColour.r, teamColour.g, teamColour.b, teamColour.a);
        Material myMaterial = reloadRadiusMaterial;
        myMaterial.SetVector("_Colour", colourAsVector);

        MeshFilter mf = shellRenderer.AddComponent<MeshFilter>();
        MeshRenderer mr = shellRenderer.AddComponent<MeshRenderer>();
        mf.sharedMesh = myMesh;
        mr.material = myMaterial;

        MeshFilter mf2 = shellRendererInside.AddComponent<MeshFilter>();
        MeshRenderer mr2 = shellRendererInside.AddComponent<MeshRenderer>();
        mf2.sharedMesh = myMesh2;
        mr2.material = myMaterial;
        mr2.material.renderQueue = mr2.material.renderQueue + 100;
    }
    #endregion
}
