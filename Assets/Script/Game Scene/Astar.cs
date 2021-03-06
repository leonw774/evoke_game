﻿using UnityEngine;
using System.Collections.Generic;

public class Tile
{
    public int h;
    public int w;
    public float estTotalCost; // estimated cost form here to goal + cost of form start to here; -1 == not yet calculated

    public Tile()
    {
        h = -1;
        w = -1;
    }

    public Tile(Tile other)
    {
        this.h = other.h;
        this.w = other.w;
        this.estTotalCost = other.estTotalCost;
    }

    public Tile(int _i, int _j)
    {
        h = _i;
        w = _j;
        estTotalCost = -1;
    }

    public Tile getNeighbor(FACETO f)
    {
        // make neighbor
        switch (f)
        {
            case FACETO.UP:
                return new Tile(h - 1, w);
            case FACETO.LEFT:
                return new Tile(h, w - 1);
            case FACETO.DOWN:
                return new Tile(h + 1, w);
            case FACETO.RIGHT:
                return new Tile(h, w + 1);
            default:
                return new Tile();
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
        if (x.estTotalCost < y.estTotalCost)
            return -1;
        if (x.estTotalCost == y.estTotalCost)
            return 0;
        return 1;
    }
}

public class Astar {

    private enum PATH_TILE_TYPE : int { WALKABLE = 0, WALL = 1, OBSTACLE = 2 };
    private int height, width;
    private PATH_TILE_TYPE[,] GeoMap;
    private int[,] CameFromMap;
    private float[,] CostMap; // cost of form start to here; -1 == not yet calculated
    private float[,] EstimatedTotalCostMap; // estimated cost form here to goal + cost of form start to here; -1 == not yet calculated
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
        GeoMap = new PATH_TILE_TYPE[height, width];
        CostMap = new float[height, width];
        EstimatedTotalCostMap = new float[height, width];
        CameFromMap = new int[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                CostMap[i, j] = 2048;
                CameFromMap[i, j] = -1;
                EstimatedTotalCostMap[i, j] = 2048;
                GeoMap[i, j] = (PATH_TILE_TYPE)((int)tiles[i, j] + ((obstaclePostionList.IndexOf(i * width + j) >= 0) ? 2 : 0));
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
        CostMap = new float[height, width];
        EstimatedTotalCostMap = new float[height, width];
        CostMap[StartTile.h, StartTile.w] = 0;
        EstimatedTotalCostMap[StartTile.h, StartTile.w] = EstimateCost(StartTile);
    }

    public float FindPath(bool ignoreObs, bool canBreakThroughObs, bool recordPath, float obstaclesCost = 1.2f)
    // will return cost of shortest path
    // return -1 means failure
    {
        TileComparer nc = new TileComparer();
        Tile curTile;
        Tile nbTile;
        while (OpenList.Count > 0)
        {
            curTile = OpenList[0];
            // the end
            if (curTile.IsEqualTile(GoalTile))
            {
                float result = CostMap[GoalTile.h, GoalTile.w];
                Refresh();
                return result;
            }

            // update lists
            OpenList.RemoveAt(0);
            ClosedList.Add(curTile);

            // examine to neighbors
            for (int nbNum = 0; nbNum < 4; nbNum++)
            {
                nbTile = curTile.getNeighbor((FACETO)nbNum);
                
                //Debug.Log("looking at " + nbTile.h + ", " + nbTile.w);
                // if already examined
                if (ClosedList.Exists(x => x.IsEqualTile(nbTile)))
                    continue;

                // if is wall then continue, but finishTile is actually a wall so:
                if (!nbTile.IsEqualTile(GoalTile) && GeoMap[nbTile.h, nbTile.w] == PATH_TILE_TYPE.WALL)
                    continue;

                // calculate cost form start to here
                float nbCostScore = CostMap[curTile.h, curTile.w] + 1;
                // treatment for obstacles
                if (!ignoreObs)
                {
                    if (GeoMap[nbTile.h, nbTile.w] == PATH_TILE_TYPE.OBSTACLE)
                    {
                        // yes: add random steps for this obs 
                        if (canBreakThroughObs)
                            nbCostScore += obstaclesCost;
                        // no: then it function as a wall
                        else
                            continue;
                    }
                }

                // check if it is a newly discovered block
                if (!OpenList.Exists(x => x.IsEqualTile(nbTile)))
                {
                    nbTile.estTotalCost = EstimateCost(nbTile) + nbCostScore;
                    OpenList.Add(nbTile);
                    OpenList.Sort(nc);
                }
                // else if there is better way to get to this block
                else if (nbCostScore >= CostMap[nbTile.h, nbTile.w])
                    continue;

                // now, this is a better way to get to this block
                // update the came-from-map with this cost
                if (recordPath) CameFromMap[nbTile.h, nbTile.w] = nbNum;
                CostMap[nbTile.h, nbTile.w] = nbCostScore;
                EstimatedTotalCostMap[nbTile.h, nbTile.w] = nbCostScore + EstimateCost(nbTile);
            } // end of while: nbTile < 4
        } // end of while: OpenList.Count > 0
        Refresh();
        return -1;
    }

    private float EstimateCost(Tile t)
    {
        return System.Math.Abs(t.h - GoalTile.h) + System.Math.Abs(t.w - GoalTile.w);
    }

    // return {-1} if there is no path
    public List<int> GetPath()
    {
        List<int> pathList = new List<int>();
        int count = 0;
        int[] curTile = new int[2] { GoalTile.h, GoalTile.w };
        // create it as {goal -> start} order
        do
        {
            pathList.Add(CameFromMap[curTile[0], curTile[1]]);
            switch (CameFromMap[curTile[0], curTile[1]])
            {
                case 0: // it came from down
                    curTile[0]++; break;
                case 1: // it came from right
                    curTile[1]++; break;
                case 2: // it came from up
                    curTile[0]--; break;
                case 3: // it came from left
                    curTile[1]--; break;
            }
            if(count++ > 50) break;
        } while (CameFromMap[curTile[0], curTile[1]] != -1);
        pathList.Reverse(); // reverse to {start -> goal} order
        return pathList;
    }
    /*
    public void PrintPath() // debug
    {
        List<Vector3> pathList;
        int count = 0;
        int[] curTile = new int[2] { GoalTile.h, GoalTile.w };
        pathList = new List<Vector3>();
        // creat it as {goal -> start} order
        //Debug.Log("Astar: PrintPath()");
        do
        {
            pathList.Add(new Vector3(curTile[0], curTile[1], CameFromMap[curTile[0], curTile[1]]));
            switch (CameFromMap[curTile[0], curTile[1]])
            {
                case 0: // it came from down
                    curTile[0]++; break;
                case 1: // it came from right
                    curTile[1]++; break;
                case 2: // it came from up
                    curTile[0]--; break;
                case 3: // it came from left
                    curTile[1]--; break;
            }
            if(count++ > 50) break;
        }  while (CameFromMap[curTile[0], curTile[1]] != -1);
        pathList.Reverse(); // reverse to {start -> goal} order
        foreach (Vector3 v in pathList)
        {
            Debug.Log(v.x + ", " + v.y + ": " + v.z);
        }
    }
    */
}
