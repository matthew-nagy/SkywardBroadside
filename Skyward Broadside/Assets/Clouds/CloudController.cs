using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{

    List<int>[,,] bubbleScales;
    Mesh mesh;
    Vector3Int gridRes;
    Color[] colours;
    bool changeToColourThisFrame = false;
    BoxCollider boxCollider;

    List<GameObject> trackingObjects;
    HashSet<List<int>> zeroSizeBubbleSet;
    List<List<int>> zeroSizeBubbleList;
    List<float> secondsTillbubbleBack;

    float cellsPerX;
    float cellsPerY;
    float cellsPerZ;

    static float s_bubbleRegenTime = 6000.0f;

    bool InBox(Vector3 coords)
    {
        if(coords.x < 0 || coords.y < 0 || coords.z < 0)
        {
            return false;
        }
        else if((int)coords.x >= gridRes.x || (int)coords.y >= gridRes.y || (int)coords.z >= gridRes.z)
        {
            return false;
        }
        return true;
    }

    bool AddZeroSizeBubble(List<int> bubble)
    {
        if (!zeroSizeBubbleSet.Contains(bubble))
        {
            zeroSizeBubbleSet.Add(bubble);
            zeroSizeBubbleList.Add(bubble);
            secondsTillbubbleBack.Add(s_bubbleRegenTime);
            foreach(int i in bubble)
            {
                colours[i].a = 0.0f;
            }
            //Make sure to update the mesh afterwards
            changeToColourThisFrame = true;
            return true;
        }
        return false;
    }
    void UpdateTrackedObjects()
    {
        foreach(GameObject go in trackingObjects)
        {
            //Get the local position of the game object
            Vector3 localPos = transform.worldToLocalMatrix.MultiplyPoint3x4(go.transform.position);
     
            Vector3 indices = new Vector3(localPos.x / cellsPerX, localPos.y / cellsPerY, localPos.x / cellsPerZ);
            if (InBox(indices))
            {
                if (AddZeroSizeBubble(getBubble((int)indices.x, (int)indices.y, (int)indices.z)))
                {
                    Debug.Log("Zeroing " + (int)indices.x + "," + (int)indices.y + "," + (int)indices.z);
                }
            }

        }
    }
    void UpdateZeroSizedBubbles()
    {
        int i = 0;
        while(i < zeroSizeBubbleList.Count)
        {
            secondsTillbubbleBack[i] -= Time.deltaTime;
            if(secondsTillbubbleBack[i] <= 0.0f)
            {
                List<int> restoreBubble = zeroSizeBubbleList[i];
                zeroSizeBubbleList.RemoveAt(i);
                zeroSizeBubbleSet.Remove(restoreBubble);
                secondsTillbubbleBack.RemoveAt(i);

                foreach(int cindex in restoreBubble)
                {
                    colours[cindex].a = 1.0f;
                }
                changeToColourThisFrame = true;
            }
            else
            {
                i += 1;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        changeToColourThisFrame = false;
        UpdateZeroSizedBubbles();
        UpdateTrackedObjects();
        if (changeToColourThisFrame)
        {
            mesh.colors = colours;
        }
    }





    //Add the collider to the tracking list
    private void OnCollisionEnter(Collision collision)
    {
        trackingObjects.Add(collision.gameObject);
        var turbulancePoints = collision.gameObject.GetComponentsInChildren<TurbulancePoint>();
        foreach(TurbulancePoint tp in turbulancePoints)
        {
            trackingObjects.Add(tp.gameObject);
        }
    }

    //Remove the collider from the tracking list
    private void OnCollisionExit(Collision collision)
    {
        trackingObjects.Remove(collision.gameObject); var turbulancePoints = collision.gameObject.GetComponentsInChildren<TurbulancePoint>();
        foreach (TurbulancePoint tp in turbulancePoints)
        {
            trackingObjects.Remove(tp.gameObject);
        }
    }

    List<int> getBubble(int x, int y, int z)
    {
        return bubbleScales[z, y, x];
    }

    void InitBubbles()
    {
        bubbleScales = new List<int>[gridRes.z, gridRes.y, gridRes.x];
        for(int z = 0; z < gridRes.z; z++)
        {
            for(int y = 0; y < gridRes.y; y++)
            {
                for(int x = 0; x < gridRes.x; x++)
                {
                    bubbleScales[z, y, x] = new List<int>();
                }
            }
        }
    }

    void InitCollisionSystem(int cloudWidth, int cloudHeight, int cloudDepth)
    {
        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(cloudWidth, cloudHeight, cloudDepth);
        boxCollider.center = boxCollider.size / 2.0f;

        trackingObjects = new List<GameObject>();

        zeroSizeBubbleSet = new HashSet<List<int>>();
        zeroSizeBubbleList = new List<List<int>>();
        secondsTillbubbleBack = new List<float>();
    }

    public void Init(Vector3Int inGridDimensions, Mesh theMesh, Color[] inColours, int cloudWidth, int cloudHeight, int cloudDepth)
    {
        gridRes = inGridDimensions;
        InitBubbles();
        mesh = theMesh;
        colours = inColours;

        InitCollisionSystem(cloudWidth, cloudHeight, cloudDepth);

        cellsPerX = (float)cloudWidth / (float)gridRes.x;
        cellsPerY = (float)cloudHeight / (float)gridRes.y;
        cellsPerZ = (float)cloudDepth / (float)gridRes.z;

        int colIndex = 0;

        for(int x = 0; x < cloudWidth; x++)
        {
            for(int y = 0; y < cloudHeight; y++)
            {
                for(int z = 0; z < cloudDepth; z++)
                {
                    List<int> colList = getBubble((int)(x / cellsPerX),(int)(y / cellsPerY), (int)(z / cellsPerZ));
                    for(int i= 0; i < 4; i++)
                    {
                        colList.Add(colIndex + i);
                    }
                    colIndex += 4;
                }
            }
        }
    }
}
