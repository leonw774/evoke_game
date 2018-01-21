using UnityEngine;
using System.Collections.Generic;
using TileTypeDefine;

public class Tile
{
    public int h;
    public int w;
    public int estimatedTotalCost; // estimated cost form here to goal + cost of form start to here; -1 == not yet calculated

    public Tile()
    {
        h = -1;
        w = -1;
    }

    public Tile(Tile other)
    {
        this.h = other.h;
        this.w = other.w;
        this.estimatedTotalCost = other.estimatedTotalCost;
    }

    public Tile(int _i, int _j)
    {
        h = _i;
        w = _j;
        estimatedTotalCost = -1;
    }

    public Tile getNeighbor(int neighborNumber)
    {
        // make neighbor
        switch (neighborNumber)
        {
            case 0: // top
                return new Tile(h - 1, w);
            case 1: // left
                return new Tile(h, w - 1);
            case 2: // down
                return new Tile(h + 1, w);
            case 3: // right
                return new Tile(h, w + 1);
            default:
                return new Tile(-1, -1);
        }
    }

    public bool IsEqualTile(Tile other)
    {
        return (other.h == h && other.w == w);
    }
}

public class TileComparer : IComparer<Tile>
{
    public int Compare(Tile x, Tile y)
    {
        if (x.estimatedTotalCost < y.estimatedTotalCost)
            return -1;
        if (x.estimatedTotalCost == y.estimatedTotalCost)
            return 0;
        return 1;
    }
}

public class Astar {

    private enum PATH_TILE_TYPE : int { WALKABLE = 0, WALL = 1, OBSTACLE = 2 };
    private int height, width;
    private int[,] GeoMap;
    private int[,] CameFromMap;
    private int[,] CostMap; // cost of form start to here; -1 == not yet calculated
    private int[,] EstimatedTotalCostMap; // estimated cost form here to goal + cost of form start to here; -1 == not yet calculated
    private List<Tile> OpenList; // Tile pending to examine, sorting increasingly by estimated score
    private List<Tile> ClosedList; // tiles done examining, sorting increasingly by estimated score
    private Tile StartTile;
    private Tile GoalTile;

    public Astar(TILE_TYPE[,] tiles, int h, int w, List<int> obstacleList, int[] start, int[] goal)
    {
        height = h;
        width = w;
        if (height < 0)
        {
            Debug.Log("map.block is empty");
            return;
        }
        // Tiles
        StartTile = new Tile(start[0], start[1]);
        GoalTile = new Tile(goal[0], goal[1]);
        // List
        OpenList = new List<Tile>();
        ClosedList = new List<Tile>();
        OpenList.Clear();
        ClosedList.Clear();
        OpenList.Add(StartTile);
        if (obstacleList.Count == 0)
            Debug.Log("obsList empty"); 
        InitializeMaps(tiles, obstacleList);
    }
    
    private void InitializeMaps(TILE_TYPE[,] tiles, List<int> obstaclePostionList)
    {
        // Map
        GeoMap = new int[height, width];
        CostMap = new int[height, width];
        EstimatedTotalCostMap = new int[height, width];
        CameFromMap = new int[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                CostMap[i, j] = 2048;
                CameFromMap[i, j] = -1;
                EstimatedTotalCostMap[i, j] = 2048;
                GeoMap[i, j] = (int)tiles[i, j] + ((obstaclePostionList.IndexOf(i * width + j) >= 0) ? 2 : 0);
            }
        }
        CostMap[StartTile.h, StartTile.w] = 0;
        EstimatedTotalCostMap[StartTile.h, StartTile.w] = EstimateCost(StartTile);
    }

    public void Refresh()
    {
        OpenList.Clear();
        ClosedList.Clear();
        OpenList.Add(StartTile);
        CostMap = new int[height, width];
        EstimatedTotalCostMap = new int[height, width];
        CostMap[StartTile.h, StartTile.w] = 0;
        EstimatedTotalCostMap[StartTile.h, StartTile.w] = EstimateCost(StartTile);
    }

    public int FindPathLength(bool ignoreObs, bool canBreakThroughObs, bool recordPath) // retrun -1 means failure
    {
        Tile curTile;
        Tile nbTile;
        while (OpenList.Count > 0)
        {
            curTile = OpenList[0];
            // the end
            if (curTile.IsEqualTile(GoalTile))
            {
                int result = CostMap[GoalTile.h, GoalTile.w];
                Refresh();
                return result;
            }

            // upadte lists
            OpenList.RemoveAt(0);
            ClosedList.Add(curTile);

            // examine to neighbors 
            int nbNum = 0;
            while (nbNum < 4)
            {
                nbTile = curTile.getNeighbor(nbNum);
                nbNum++;
                //Debug.Log("looking at " + nbTile.h + ", " + nbTile.w);
                // if already examined
                if (ClosedList.Exists(x => x.IsEqualTile(nbTile)))
                    continue;

                // if is wall then continue, but finishTile is actually a wall so:
                if (!nbTile.IsEqualTile(GoalTile) && (GeoMap[nbTile.h, nbTile.w] == (int)PATH_TILE_TYPE.WALL))
                    continue;

                // calculate cost form start to here
                int nbCostScore = CostMap[curTile.h, curTile.w] + 1;
                // treatment for obstacles
                if (!ignoreObs)
                {
                    if (GeoMap[nbTile.h, nbTile.w] == (int)PATH_TILE_TYPE.OBSTACLE)
                    {
                        // yes: add random steps for this obs 
                        if (canBreakThroughObs)
                            nbCostScore += ((Random.Range(0, 2) > 0) ? 1 : 0);
                        // no: then it function as a wall
                        else
                            continue;
                    }
                }

                // check if it is a newly discovered block
                if (!OpenList.Exists(x => x.IsEqualTile(nbTile)))
                {
                    TileComparer nc = new TileComparer();
                    nbTile.estimatedTotalCost = EstimateCost(nbTile) + nbCostScore;
                    OpenList.Add(nbTile);
                    OpenList.Sort(nc);
                }
                // else if there is better way to get to this block
                else if (nbCostScore >= CostMap[nbTile.h, nbTile.w])
                    continue;

                // now, this is a better way to get to this block
                // updat the came-from-map with this cost
                if (recordPath) CameFromMap[nbTile.h, nbTile.w] = (nbNum - 1); // nbNum - 1 because it is now the next neighbor
                CostMap[nbTile.h, nbTile.w] = nbCostScore;
                EstimatedTotalCostMap[nbTile.h, nbTile.w] = nbCostScore + EstimateCost(nbTile);
            } // end of while: nbTile < 4
        } // end of while: OpenList.Count > 0
        Refresh();
        return -1;
    }

    private int EstimateCost(Tile t)
    {
        return System.Math.Abs(t.h - GoalTile.h) + System.Math.Abs(t.w - GoalTile.w);
    }

    // return {-1} if there is no path
    public List<int> GetPath()
    {
        List<int> pathList;
        int count = 0;
        int[] currentTile = new int[2] { GoalTile.h, GoalTile.w };
        pathList = new List<int>();
        // creat it as {goal -> start} order
        do
        {
            pathList.Add(CameFromMap[currentTile[0], currentTile[1]]);
            switch (CameFromMap[currentTile[0], currentTile[1]])
            {
                case 0: // it came from down
                    currentTile[0]++; break;
                case 1: // it came from right
                    currentTile[1]++; break;
                case 2: // it came from up
                    currentTile[0]--; break;
                case 3: // it came from left
                    currentTile[1]--; break;
            }
            if(count++ > 50) break;
        } while (CameFromMap[currentTile[0], currentTile[1]] != -1);
        pathList.Reverse(); // reverse to {start -> goal} order
        return pathList;
    }

    public void PrintPath() // debug
    {
        List<Vector3> pathList;
        int count = 0;
        int[] currentTile = new int[2] { GoalTile.h, GoalTile.w };
        pathList = new List<Vector3>();
        // creat it as {goal -> start} order
        //Debug.Log("Astar: PrintPath()");
        do
        {
            pathList.Add(new Vector3(currentTile[0], currentTile[1], CameFromMap[currentTile[0], currentTile[1]]));
            switch (CameFromMap[currentTile[0], currentTile[1]])
            {
                case 0: // it came from down
                    currentTile[0]++; break;
                case 1: // it came from right
                    currentTile[1]++; break;
                case 2: // it came from up
                    currentTile[0]--; break;
                case 3: // it came from left
                    currentTile[1]--; break;
            }
            if(count++ > 50) break;
        }  while (CameFromMap[currentTile[0], currentTile[1]] != -1);
        pathList.Reverse(); // reverse to {start -> goal} order
        foreach (Vector3 v in pathList)
        {
            Debug.Log(v.x + ", " + v.y + ": " + v.z);
        }
    }
}
