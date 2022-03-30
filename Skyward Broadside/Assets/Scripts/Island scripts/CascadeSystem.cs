using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class CascadeCell
{
    const float shatterProportion = 0.6f;
    const float shatterMinimumWaitSeconds = 0.1f;
    const float shatterMaximumWaitSeconds = 0.5f;

    List<Breakable> containedBreakables;
    int brokenBreakables;

    CascadeCell[] neighbours;
    int nextNeighbourIndex;

    bool buoyancyPoint;

    public void DebugColour()
    {
        Color dCol = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        foreach (Breakable b in containedBreakables)
        {
            Renderer r = b.GetComponent<Renderer>();
            r.material.color = dCol;
        }
        if(containedBreakables.Count > 0)
        {
            Debug.Log("Contains breakable");
        }
    }

    public void Init()
    {
        containedBreakables = new List<Breakable>();
        brokenBreakables = 0;

        neighbours = new CascadeCell[6];
        nextNeighbourIndex = 0;
        
        buoyancyPoint = false;
    }

    public List<Breakable> GetContainedBreakables()
    {
        return containedBreakables;
    }
    public void AddBreakable(Breakable b)
    {
        containedBreakables.Add(b);
    }
    public int Size()
    {
        return containedBreakables.Count;
    }

    public void InformOfBreak()
    {
        brokenBreakables += 1;
    }
    public bool ShouldShatter()
    {
        return ((float)brokenBreakables / (float)containedBreakables.Count) >= shatterProportion;
    }
    //You may not always want the overhead of neighbour removal, especially if an entire section of cells are broken
    public IEnumerator Shatter(bool removeNeighbours)
    {
        if (removeNeighbours)
        {
            for (int i = 0; i < nextNeighbourIndex; i++)
            {
                neighbours[i].RemoveNeighbour(this);
            }
        }

        float waitPeriod = Random.Range(shatterMinimumWaitSeconds, shatterMaximumWaitSeconds);
        yield return new WaitForSeconds(waitPeriod);

        foreach(Breakable b in containedBreakables)
        {
            if(b != null)//Could have been deleted
            {
                b.GamePlayBreakCommand();
            }
        }
    }
   
    //You can only pathfind on a neighbour if it actually has any breakables in it
    public void AddNeighbour(CascadeCell n)
    {
        if (n.Size() > 0)
        {
            neighbours[nextNeighbourIndex] = n;
            nextNeighbourIndex += 1;
        }
    }
    void RemoveNeighbour(CascadeCell cell)
    {
        for(int i = 0; i < nextNeighbourIndex; i++)
        {
            if(neighbours[i] == cell)
            {
                for(int j = i + 1; j < nextNeighbourIndex; j++)
                {
                    neighbours[j - 1] = neighbours[j];
                }
                nextNeighbourIndex -= 1;
                return;
            }
        }
    }
    public CascadeCell[] Neighbours()
    {
        return neighbours;
    }
    public int GetNeighbourNumber()
    {
        return nextNeighbourIndex;
    }

    public void MakeBuoyant()
    {
        buoyancyPoint = true;
    }
    public bool IsBuoyant()
    {
        return buoyancyPoint;
    }
}

struct CascadeSearchInfo
{
    HashSet<Vector3Int> safeIndices;
    HashSet<Vector3Int> brokenIndices;
}

public class CascadeSystem : MonoBehaviour
{
    //The two points to make a cube the island will sit in
    public Transform topGuard;
    public Transform bottomGuard;
    
    //Points the islands float off 
    public List<GameObject> buoyancyPoints;
    HashSet<CascadeCell> buoyanceyCells;

    //The dimensions the system will have. Row, columns and depth of the cell grid
    public Vector3Int cellResolution = new Vector3Int(10, 10, 10);
    Vector3 cellDimensions;

    CascadeCell[,,] grid;



    Vector3Int GetIndexFromPosition(Vector3 position)
    {
        Vector3 normalizedPosition = position - topGuard.localPosition;
        Vector3Int index = new Vector3Int(
            (int)(normalizedPosition.x / cellDimensions.x),
            (int)(normalizedPosition.y / cellDimensions.y),
            (int)(normalizedPosition.z / cellDimensions.z)
        );
        return index;
    }


    bool IndexInvalid(Vector3Int index)
    {
        if(index.x < 0 || index.y < 0 || index.z < 0)
        {
            //Debug.LogError("Index has values less than 0");
            return true;
        }
        else if(index.x >= cellResolution.x || index.y >= cellResolution.y || index.z >= cellResolution.z)
        {
            //Debug.LogError("Index greater than cell resolution");
            return true;
        }
        return false;
    }

    // Only called by the BreakMaster if this is the master photon client
    public void Init(List<Breakable> allBreakables)
    {
        SortGuards();       //make sure the top guard has the lower of all values, the bottom has the heighest
        InitGrid();
        Instantiate(Resources.Load("DebugSphere"), topGuard.position, Quaternion.identity);
        Instantiate(Resources.Load("DebugSphere"), bottomGuard.position, Quaternion.identity);

        Vector3 dimensions = bottomGuard.localPosition - topGuard.localPosition;
        cellDimensions = new Vector3(dimensions.x / (float)cellResolution.x, dimensions.y / (float)cellResolution.y, dimensions.z / (float)cellResolution.z);

        buoyanceyCells = new HashSet<CascadeCell>();


        foreach (Breakable b in allBreakables)
        {
            Vector3Int index = GetIndexFromPosition(b.transform.localPosition);
            grid[index.z, index.y, index.x].AddBreakable(b);
        }
        foreach (GameObject bp in buoyancyPoints)
        {
            Vector3Int index = GetIndexFromPosition(bp.transform.localPosition);
            CascadeCell cell = grid[index.z, index.y, index.x];
            buoyanceyCells.Add(cell);
            cell.MakeBuoyant();
        }

        

        AddCellNeighbours();
    }

    #region InitFunctions

    void SortGuards()
    {
        Vector3 lesser = topGuard.localPosition;
        Vector3 greater = bottomGuard.localPosition;
        float temp;
        if (lesser.x > greater.x)
        {
            temp = lesser.x;
            lesser.x = greater.x;
            greater.x = temp;
        }
        if (lesser.y > greater.y)
        {
            temp = lesser.y;
            lesser.y = greater.y;
            greater.y = temp;
        }
        if (lesser.z > greater.z)
        {
            temp = lesser.z;
            lesser.z = greater.z;
            greater.z = temp;
        }

        topGuard.localPosition = lesser;
        bottomGuard.localPosition = greater;
    }

    void InitGrid()
    {
        grid = new CascadeCell[cellResolution.z, cellResolution.y, cellResolution.x];
        for (int z = 0; z < cellResolution.z; z++)
        {
            for (int y = 0; y < cellResolution.y; y++)
            {
                for (int x = 0; x < cellResolution.x; x++)
                {
                    grid[z, y, x] = new CascadeCell();
                    grid[z, y, x].Init();
                }
            }
        }
    }

    void AddCellNeighbours()
    {
        for (int z = 0; z < cellResolution.z; z++)
        {
            for (int y = 0; y < cellResolution.y; y++)
            {
                for (int x = 0; x < cellResolution.x; x++)
                {
                    AddCellNeighbours(x, y, z);
                    grid[z, y, x].DebugColour();
                }
            }
        }
    }
    void AddCellNeighbours(int x, int y, int z)
    {
        CascadeCell cell = grid[z, y, x];
        if (x > 0)
        {
            cell.AddNeighbour(grid[z, y, x - 1]);
        }
        if (x + 1 < cellResolution.x)
        {
            cell.AddNeighbour(grid[z, y, x + 1]);
        }

        if (y > 0)
        {
            cell.AddNeighbour(grid[z, y - 1, x]);
        }
        if (y + 1 < cellResolution.y)
        {
            cell.AddNeighbour(grid[z, y + 1, x]);
        }

        if (z > 0)
        {
            cell.AddNeighbour(grid[z - 1, y, x]);
        }
        if (z + 1 < cellResolution.z)
        {
            cell.AddNeighbour(grid[z + 1, y, x]);
        }
    }

    #endregion

    public void InformOfBreak(Breakable b)
    {
        Vector3Int index = GetIndexFromPosition(b.transform.position);
        CascadeCell cell = grid[index.z, index.y, index.x];
        cell.InformOfBreak();

        if (cell.ShouldShatter())
        {
            StartCoroutine(cell.Shatter(true));

            HashSet<CascadeCell> safeCells = new HashSet<CascadeCell>(buoyanceyCells);  //All buoyant cells are certainly safe
            HashSet<CascadeCell> brokenCells = new HashSet<CascadeCell>();  //Empty, none are broken yet

            for(int i = 0; i < cell.GetNeighbourNumber(); i++)
            {
                System.Tuple<HashSet<CascadeCell>, bool> result = BreakSearch(safeCells, brokenCells, cell);
                if (result.Item2)
                {
                    brokenCells.UnionWith(result.Item1);
                }
                else
                {
                    safeCells.UnionWith(result.Item1);
                }
            }

            foreach(CascadeCell bCell in brokenCells)   //A cell can only be broken when the search was exhaustive, so they are all certainly broken
            {
                StartCoroutine(bCell.Shatter(false));
            }
        }

    }


    System.Tuple<HashSet<CascadeCell>, bool> BreakSearch(HashSet<CascadeCell> safeCells, HashSet<CascadeCell> brokenCells, CascadeCell startCell)
    {
        HashSet<CascadeCell> closedSet = new HashSet<CascadeCell>();
        HashSet<CascadeCell> openSet = new HashSet<CascadeCell>();
        List<CascadeCell> openList = new List<CascadeCell>();
        int openListIndex = 0;
        bool broken = false;

        openSet.Add(startCell);
        openList.Add(startCell);
        openListIndex += 1;
        if (safeCells.Contains(startCell))   //Finish now, because we know we are safe
        {
            return System.Tuple.Create(openSet, false);
        }
        else if (brokenCells.Contains(startCell))    //Finish now because we know we are not safe
        {
            return System.Tuple.Create(openSet, true);
        }


        bool searching = true;
        CascadeCell currentCell;
        while (openSet.Count > 0 && searching)
        {
            currentCell = openList[openListIndex];
            openListIndex++;
            openSet.Remove(currentCell);
            closedSet.Add(currentCell);

            CascadeCell[] neighbours = currentCell.Neighbours();
            int neighbourNum = currentCell.GetNeighbourNumber();

            for(int i = 0; i < neighbourNum; i++)
            {
                CascadeCell cell = neighbours[i];
                if(!openSet.Contains(cell) && !closedSet.Contains(cell))    //Ignore this cell if we have looked at it, or will look at it later
                {
                    if (brokenCells.Contains(cell))
                    {
                        closedSet.Add(cell);
                        searching = false;
                        broken = true;
                    }
                    else if(safeCells.Contains(cell)){
                        closedSet.Add(cell);
                        searching = false;
                        broken = false;
                    }
                    else
                    {
                        openList.Add(cell);
                        openSet.Add(cell);
                    }
                }
            }
        }


        return System.Tuple.Create<HashSet<CascadeCell>, bool>(closedSet, broken);
    }
}
