using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{

    List<int>[,,] bubbleScales;
    Mesh mesh;
    Vector3Int gridRes;
    Color[] colours;

    // Update is called once per frame
    void Update()
    {
        
    }

    List<int> getBubble(int x, int y, int z)
    {
        if (x >= gridRes.x)
        {
            x = gridRes.x - 1;
        }
        else if(x < 0)
        {
            x = 0;
        }
        if (y >= gridRes.y)
        {
            y = gridRes.y - 1;
        }
        else if (y < 0)
        {
            y = 0;
        }
        if (z >= gridRes.z)
        {
            z = gridRes.z - 1;
        }
        else if (z < 0)
        {
            z = 0;
        }

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

    public void Init(Vector3Int inGridDimensions, Mesh theMesh, Color[] inColours, int cloudWidth, int cloudHeight, int cloudDepth)
    {
        gridRes = inGridDimensions;
        InitBubbles();
        mesh = theMesh;
        colours = inColours;

        float cellsPerX = (float)cloudWidth / (float)gridRes.x;
        float cellsPerY = (float)cloudHeight / (float)gridRes.y;
        float cellsPerZ = (float)cloudDepth / (float)gridRes.z;

        int colIndex = 0;

        for(int x = 0; x < cloudWidth; x++)
        {
            for(int y = 0; y < cloudHeight; y++)
            {
                for(int z = 0; z < cloudDepth; z++)
                {
                    List<int> colList = getBubble((int)(x / cellsPerX),(int)(y / cellsPerY), (int)(z / cellsPerZ));
                    Debug.Log((int)(x / cellsPerX) + " " + (int)(y / cellsPerY) + " " + (int)(z / cellsPerZ));
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
