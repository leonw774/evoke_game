using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockNode
{
    public int h;
    public int w;
    public int estimatedTotalCost; // estimated cost form here to goal + cost of form start to here; -1 == not yet calculated

    public BlockNode(int _i, int _j)
    {
        h = _i;
        w = _j;
        estimatedTotalCost = -1;
    }

    public BlockNode getNeighbor(int neighborNumber)
    {
        // make neighbor
        switch (neighborNumber)
        {
            case 0: // top
                return new BlockNode(h - 1, w);
            case 1: // left
                return new BlockNode(h, w - 1);
            case 2: // down
                return new BlockNode(h + 1, w);
            case 3: // right
                return new BlockNode(h, w + 1);
            default:
                return new BlockNode(-1, -1);
        }
    }

    public bool IsEqualBlock(BlockNode other)
    {
        return (other.h == h && other.w == w);
    }
}

public class NodeComparer : IComparer<BlockNode>
{
    public int Compare(BlockNode x, BlockNode y)
    {
        if (x.estimatedTotalCost < y.estimatedTotalCost)
            return -1;
        if (x.estimatedTotalCost == y.estimatedTotalCost)
            return 0;
        return 1;
    }
}

public class Astar {

    private enum PATH_BLOCK_TYPE : int { WALKABLE = 0, WALL = 1, OBSTACLE = 2 };
    private int height, width;
    private int[,] GeoMap;
    private int[,] CameFromMap;
    private int[,] CostMap; // cost of form start to here; -1 == not yet calculated
    private int[,] EstimatedTotalCostMap; // estimated cost form here to goal + cost of form start to here; -1 == not yet calculated
    private List<BlockNode> OpenList; // blocks pending to examine, sorting increasingly by estimated score
    private List<BlockNode> ClosedList; // blocks done examining, sorting increasingly by estimated score
    private BlockNode StartBlock;
    private BlockNode GoalBlock;

    public Astar(int[,] blocks, int h, int w, List<int> obstacleList, int[] start, int[] goal)
    {
        height = h;
        width = w;
        if (height < 0)
        {
            Debug.Log("map.block is empty");
            return;
        }
        // Blocks
        StartBlock = new BlockNode(start[0], start[1]);
        GoalBlock = new BlockNode(goal[0], goal[1]);
        // List
        OpenList = new List<BlockNode>();
        ClosedList = new List<BlockNode>();
        OpenList.Clear();
        ClosedList.Clear();
        OpenList.Add(StartBlock);
        if (obstacleList.Count == 0)
            Debug.Log("obsList empty"); 
        InitializeMaps(blocks, obstacleList);
    }
    
    private void InitializeMaps(int[,] blocks, List<int> obstaclePostionList)
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
                GeoMap[i, j] = blocks[i, j] + ((obstaclePostionList.IndexOf(i * width + j) >= 0) ? 2 : 0);
            }
        }
        CostMap[StartBlock.h, StartBlock.w] = 0;
        EstimatedTotalCostMap[StartBlock.h, StartBlock.w] = EstimateCost(StartBlock);
    }

    public void Refresh()
    {
        OpenList.Clear();
        ClosedList.Clear();
        OpenList.Add(StartBlock);
        CostMap = new int[height, width];
        EstimatedTotalCostMap = new int[height, width];
        CostMap[StartBlock.h, StartBlock.w] = 0;
        EstimatedTotalCostMap[StartBlock.h, StartBlock.w] = EstimateCost(StartBlock);
    }

    public int FindPathLength(bool canBreakThroughObs, bool recordPath) // retrun -1 means failure
    {
        while(OpenList.Count > 0)
        {
            BlockNode curBlock;
            curBlock = OpenList[0];
            // the end
            if (curBlock.IsEqualBlock(GoalBlock))
            {
                int result = CostMap[GoalBlock.h, GoalBlock.w];
                Refresh();
                return result;
            }

            // upadte lists
            OpenList.RemoveAt(0);
            ClosedList.Add(curBlock);

            // examine to neighbors 
            int nbNum = 0;
            while(nbNum < 4)
            {
                BlockNode nbBlock = curBlock.getNeighbor(nbNum);
                nbNum++;
                //Debug.Log("looking at " + nbBlock.h + ", " + nbBlock.w);
                // if already examined
                if (ClosedList.Exists(x => x.IsEqualBlock(nbBlock)))
                    continue;

                // if is wall then continue, but finishBlock is actually a wall so:
                if (!nbBlock.IsEqualBlock(GoalBlock) && (GeoMap[nbBlock.h, nbBlock.w] == (int)PATH_BLOCK_TYPE.WALL))
                    continue;

                // calculate cost form start to here
                int nbCostScore = CostMap[curBlock.h, curBlock.w] + 1;
                if (GeoMap[nbBlock.h, nbBlock.w] == (int)PATH_BLOCK_TYPE.OBSTACLE)
                {
                    if (canBreakThroughObs)
                    { // yes: add random steps for this obs
                        int rn = Random.Range(0, 2);
                        nbCostScore += ((Random.Range(0, 1) == 0) ? 1 : rn);
                    }
                    else
                    { // no: then it function as a wall
                        continue;
                    }
                }
                // check if it is a newly discovered block
                if (!OpenList.Exists(x => x.IsEqualBlock(nbBlock)))
                {
                    NodeComparer nc = new NodeComparer();
                    nbBlock.estimatedTotalCost = EstimateCost(nbBlock) + nbCostScore;
                    OpenList.Add(nbBlock);
                    OpenList.Sort(nc);
                }
                // else if there is better way to get to this block
                else if (nbCostScore >= CostMap[nbBlock.h, nbBlock.w])
                    continue;

                // now, this is a better way to get to this block
                if (recordPath) CameFromMap[nbBlock.h, nbBlock.w] = (nbNum - 1); // nbNum - 1 because it is now the next neighbor
                CostMap[nbBlock.h, nbBlock.w] = nbCostScore;
                EstimatedTotalCostMap[nbBlock.h, nbBlock.w] = nbCostScore + EstimateCost(nbBlock);
            }
        }
        Refresh();
        return -1;
    }

    private int EstimateCost(BlockNode bn)
    {
        return System.Math.Abs(bn.h - GoalBlock.h) + System.Math.Abs(bn.w - GoalBlock.w);
    }

    // return {-1} if there is no path
    public List<int> GetPath()
    {
        List<int> pathList;
        int count = 0;
        int[] currentBlock = new int[2] { GoalBlock.h, GoalBlock.w };
        pathList = new List<int>();
        pathList.Add(CameFromMap[currentBlock[0], currentBlock[1]]);

        while (CameFromMap[currentBlock[0], currentBlock[1]] != -1)
        {
            //Debug.Log(currentBlock[0] + "," + currentBlock[1]);
            switch (CameFromMap[currentBlock[0], currentBlock[1]])
            {
                case 0: // down
                    currentBlock[0]++; break;
                case 1: // right
                    currentBlock[1]++; break;
                case 2: // up
                    currentBlock[0]--; break;
                case 3: // left
                    currentBlock[1]--; break;
            }
            pathList.Add(CameFromMap[currentBlock[0], currentBlock[1]]);
            if(count++ > 50)
                break;
        }
        return pathList;
    }
}
