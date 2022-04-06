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
        reloadRadius = GetComponent<SphereCollider>().radius;
        Invoke(nameof(Setup), 1f);
    }

    //Doesnt work right now
    void Setup()
    {
        if (Blackboard.playerPhotonHub.myTeam == myTeam || true)
        {
            Debug.Log("Making the mesh");
            CreateDisplayMesh();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ship" && other.gameObject.GetComponent<PlayerController>().myTeam == myTeam)
        {
            other.gameObject.GetComponent<PlayerController>().resupply = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Ship" && other.gameObject.GetComponent<PlayerController>().myTeam == myTeam)
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
        }
    }

    void CreateDisplayMesh()
    {
        reloadFieldMesh = new Mesh();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        float heightChunk = reloadRadius / meshHeightResolution;
        for(int i = meshHeightResolution * -1; i < meshHeightResolution; i += 2)
        {
            AddVertCircle(i * heightChunk, verts, norms);
        }

        List<int> tris;
        Debug.Log("Making " + verts.Count + " spheres");
        for(int i = 0; i < verts.Count; i++)
        {
            Instantiate(Resources.Load("DebugSphere"));
        }
    }
}
