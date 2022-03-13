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

    public float reloadRadius = 60f;

    public List<TeamToMat> teamMaterials;
    public string myTeam;

    public int meshCircleResolution = 30;

    [Tooltip("What game object's materials need to be set with the bases team")]
    public List<GameObject> materialSettingObjects;

    public Material friendlyReloadMaterial;


    Mesh reloadFieldMesh;

    // Update is called once per frame
    void Update()
    {
        PlayerPhotonHub pph = GameObject.Find("Game Manager").GetComponent<GameManager>().serverPlayerPhotonHub;
        if(pph == null || PhotonNetwork.LocalPlayer.GetPhotonTeam() == null)
        {   //Photon hasn't finished loading in yet, do this next turn
            return;
        }

        ReloadStation myStation = new ReloadStation();
        myStation.reloadRadius = reloadRadius;
        myStation.gameObject = gameObject;

        if(myTeam == "Red")
        {
            pph.redReloadStations.Add(myStation);
        }
        else if(myTeam == "Blue")
        {
            pph.blueReloadStations.Add(myStation);
        }
        else
        {
            Debug.LogError("Cannot find a team with name '" + myTeam + ".");
        }

        foreach(TeamToMat ttm in teamMaterials)
        {
            if(myTeam == ttm.teamName)
            {
                foreach(GameObject go in materialSettingObjects)
                {
                    go.GetComponent<Renderer>().material = ttm.material;
                }
                break;
            }
        }

        if(myTeam == PhotonNetwork.LocalPlayer.GetPhotonTeam().Name)
        {
            CreateDisplayMesh();
        }

        Destroy(this);
    }

    void CreateDisplayMesh()
    {
        reloadFieldMesh = new Mesh();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        Renderer renderer = gameObject.AddComponent<MeshRenderer>();

        Vector3[] vertices = new Vector3[meshCircleResolution * 2];
        int[] tris = new int[meshCircleResolution * 6];//bc each point connects to the pair to the right, with two tris. So one tri per vert, 3 ints per tri
        //Create a circle both far above and far below the reload point, with a radius based off the reload radius
        for(int i = 0; i < meshCircleResolution; i++)
        {
            Vector3 offset = Quaternion.Euler(0, 360.0f / (float)meshCircleResolution, 0) * (Vector3.forward * reloadRadius);
            vertices[i] = transform.position + offset + (Vector3.up * 1000.0f);
            vertices[meshCircleResolution + i] = transform.position + offset + (Vector3.down * 1000.0f);
        }

        //Set all circles clockwise
        for(int i = 0; i < meshCircleResolution - 1; i++)
        {
            int triBase = i * 6;
            tris[triBase]       = i;       //Current on the top
            tris[triBase + 1]   = i + 1;   //Current on top + 1 to the right
            tris[triBase + 2]   = i + meshCircleResolution + 1; //On bottom, one to the right

            tris[triBase + 3] = i + meshCircleResolution + 1; //On bottom, one to the right
            tris[triBase + 4] = i + meshCircleResolution; //Current on bottom
            tris[triBase + 5] = i;  //Current one again
        }

        reloadFieldMesh.vertices = vertices;
        reloadFieldMesh.triangles = tris;


        meshFilter.mesh = reloadFieldMesh;
        renderer.material = friendlyReloadMaterial;
    }
}
