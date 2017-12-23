﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour {

    public List<int> positionList = null;  // store obstacle position in as integer(h * width + w)
    private Level_Map levelMap;
    private GameObject prototype;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Obstacles.Start()");
    }
    
    public void Initialize()
    {
        positionList = new List<int>();
        levelMap = gameObject.GetComponent<Level_Map>();
        DestroyAllObstacles(); // deatroy previous obstacles
        prototype = GameObject.Find("Prototype Obstacle Sprite");
    }

    public void Construct()
    {
        int walkableTilesNum = levelMap.tiles.Length - levelMap.wallsNumber;
        do
        {
            if (positionList.Count > 0) DestroyAllObstacles();
            Generate();
            Adjust();
        } while (positionList.Count < (int) (walkableTilesNum * 0.3) || positionList.Count > (int)(walkableTilesNum * 0.7));

        Debug.Log("There are " + positionList.Count + " obs in map");
    }

    public void ObsUpdate(int pos)
    {
        ObsUpdate(pos / levelMap.width, pos % levelMap.width);
    }

    public void ObsUpdate(int h, int w)
    {
        int pos = h * levelMap.width + w;
        // check if there is already a obstacle on this block
        if (positionList.IndexOf(pos) != -1)
        {
            ObsDestroy(pos);
        }
        // if there is not obs & block is walkable then
        else if (levelMap.tiles[h, w] == 0)
        {
            levelMap.theMonsters.TryKillMonsterByPos(pos);
            ObsCreate(h, w);
        }
    }

    public void ObsCreate(int h, int w)
    {
        // Debug.Log("obs creation happened");
        int pos = h * levelMap.width + w;
        Vector3 trans = new Vector3((w - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - h - 0.5f), 0);
        GameObject created = Instantiate(prototype);
        positionList.Add(pos);
        positionList.Sort();
        created.name = "Obstacle Sprite" + pos.ToString();
        created.tag = "Obstacle";
        created.transform.parent = GameObject.Find("Game Panel").transform;
        created.transform.position = trans;
    }

    public void ObsDestroy(int pos)
    {
        //Debug.Log("obs destruction happened");
        GameObject[] obs = GameObject.FindGameObjectsWithTag("Obstacle");
        for (int k = 0; k < obs.Length; ++k)
        {
            if (obs[k].name == "Obstacle Sprite" + pos.ToString())
            {
                Destroy(obs[k]);
                obs[k] = null;
            }
        }
        positionList.Remove(pos);
    }

    public void Generate()
    {
        // make horizonal obs
        for (int i = 1; i < levelMap.height - 1; ++i) // for every height
        {
            bool putObs = Random.Range(-5, 5) > 0;
            int obslength = 0;
            for (int j = 1; j < levelMap.width - 1; ++j)
            {
                if (putObs && levelMap.tiles[i, j] == (int)Level_Map.TILE_TYPE.WALKABLE)
                {
                    // STATE: PUT OBS
                    ObsUpdate(i, j);
                    obslength++;
                    if (Random.Range(-5, 5) < 0 || obslength > Random.Range(8, 10)) // possibility to change state
                    {
                        putObs = !putObs;
                        obslength = 0;
                    }
                }
                else
                {
                    // STATE: DONT PUT OBS
                    if (Random.Range(-5, 9) < 0) // possibility to change state 
                        putObs = !putObs;
                }
            }
        }
        // make vertical obs
        for (int j = 1; j < levelMap.width - 1; ++j)
        {
            bool putObs = Random.Range(-5, 5) > 0;
            for (int i = levelMap.height - 2; i > 0; --i)
            {
                if (putObs && levelMap.tiles[i, j] == (int)Level_Map.TILE_TYPE.WALKABLE)
                {
                    ObsUpdate(i, j);
                    if (Random.Range(-5, 5) < 0)
                        putObs = !putObs;
                }
                else
                {
                    if (Random.Range(-5, 9) < 0)
                        putObs = !putObs;
                }
            }
        }
    }

    public void Adjust()
    {
        int count = 0;
        bool find_something_to_adjust = true;

        do
        {
            find_something_to_adjust = DistributeAdjust();
            CorridorAdjust();
            count++;
        } while (find_something_to_adjust && count < 4);
        //DistributeAdjust();
        Debug.Log("Obstacles Adjusted");
        /*
        DistributeAdjust();
        CorridorAdjust();
        */
        // in opening, too much obstacles should not neighbor or be on same block of the player and finish
        int playerPosition = levelMap.playerStartTile[0] * levelMap.width + levelMap.playerStartTile[1];
        //Debug.Log("playerPosition:" + playerPosition);
        if (positionList.Exists(x => x == playerPosition)) // same block
            ObsUpdate(playerPosition);
        if (positionList.Exists(x => x == playerPosition + 1)) // right
            ObsUpdate(playerPosition + 1);
        if (positionList.Exists(x => x == playerPosition - 1)) // left
            ObsUpdate(playerPosition - 1);
        if (positionList.Exists(x => x == playerPosition + levelMap.width)) // up
            ObsUpdate(playerPosition + levelMap.width);
        if (positionList.Exists(x => x == playerPosition - levelMap.width)) // down
            ObsUpdate(playerPosition - levelMap.width);
    }

    private bool DistributeAdjust()
    {
        bool some_adjustment_are_done = false;
        // adjust too crowded and too loosen obstacles
        for (int i = 1; i < levelMap.height - 1; ++i)
        {
            for (int j = 1; j < levelMap.width - 1; ++j)
            {
                int thisTilePos = i * levelMap.width + j;
                if (levelMap.tiles[i, j] != (int)Level_Map.TILE_TYPE.WALKABLE)
                    continue;
                // else
                bool this_is_obs = (positionList.Exists(x => x == thisTilePos));
                int chi_of_this = 4;
                int di = -1, dj = -1, sameNeighborCount = 0;
                while (di <= 1)
                {
                    if (di == 0 & dj == 0) continue;
                    int neighborTilePos = (i + di) * levelMap.width + (j + dj);
                    // no matter the neighbor block is really a obstacle or a wall, it all count as obstacle
                    bool neighbor_is_obs = (positionList.Exists(x => x == neighborTilePos)) || (levelMap.tiles[i + di, j + dj] != (int)Level_Map.TILE_TYPE.WALKABLE);
                    // check if neighbor is same with this block
                    if (this_is_obs == neighbor_is_obs)
                    {
                        sameNeighborCount++;
                        if( di == 0 || dj == 0)
                            chi_of_this--;
                    }
                    // upadte neighbor tiles ij
                    if (dj == 1)
                    {
                        di++;
                        dj = -1;
                    }
                    else if (di == 0 & dj == -1) dj = 1;
                    else dj++;
                }
                if (sameNeighborCount >= (this_is_obs ? (7 - ((chi_of_this < 1) ? 1 : 0)) : 5))
                {
                    some_adjustment_are_done = true;
                    ObsUpdate(i, j);
                    if (sameNeighborCount == 8)
                    {
                        ObsUpdate(i + Random.Range(-1, 2), j);
                        ObsUpdate(i, j + Random.Range(-1, 2));
                    }
                }
            } // end of for: j
        } // end of for: i
        return some_adjustment_are_done;
    }

    private void CorridorAdjust()
    {
        // after previous adjustion, there could be some "corridor shape" obstacles
        // we want to "close" those corridor
        for (int i = 1; i < levelMap.height - 1; ++i)
        {
            for (int j = 1; j < levelMap.width - 1; ++j)
            {
                if (levelMap.tiles[i, j] == (int)Level_Map.TILE_TYPE.WALKABLE)
                {
                    int pos = i * levelMap.width + j, mw = levelMap.width, d = -1;
                    bool is_up_all_obs = true, is_down_all_obs = true,
                        is_left_all_obs = true, is_right_all_obs = true,
                        is_middle_all_walkable = true;
                    // check vertical corridor
                    while (d <= 1)
                    {
                        // Note: Map.TILE_TYPE.WALL is 1
                        is_up_all_obs = is_up_all_obs && (positionList.Exists(x => x == pos - mw + d) || levelMap.tiles[i - 1, j + d] == 1);
                        is_down_all_obs = is_down_all_obs && (positionList.Exists(x => x == pos + mw + d) || levelMap.tiles[i + 1, j + d] == 1);
                        is_middle_all_walkable = is_middle_all_walkable && !(positionList.Exists(x => x == pos + d) || levelMap.tiles[i, j + d] == 1);
                        d++;
                    }
                    if (is_middle_all_walkable && is_up_all_obs && is_down_all_obs && Random.Range(0, 6) > 0)
                    {
                        ObsUpdate(i, j); // add an obs in the walk way
                        ObsUpdate(i + ((Random.Range(0, 2) == 0) ? 1 : -1), j);// then randomly delete a obs
                        return;
                    }
                    // check horizontal corridor
                    d = -1;
                    is_middle_all_walkable = true;
                    while (d <= 1)
                    {
                        is_left_all_obs = is_left_all_obs && (positionList.Exists(x => x == pos + d * mw - 1) || levelMap.tiles[i + d, j - 1] == 1);
                        is_right_all_obs = is_right_all_obs && (positionList.Exists(x => x == pos + d * mw + 1) || levelMap.tiles[i + d, j + 1] == 1);
                        is_middle_all_walkable = is_middle_all_walkable && !(positionList.Exists(x => x == pos + d * mw) || levelMap.tiles[i + d, j] == 1);
                        d++;
                    }
                    if (is_middle_all_walkable && is_left_all_obs && is_right_all_obs && Random.Range(0, 6) > 0)
                    {
                        ObsUpdate(i, j); // add an obs in the walk way
                        ObsUpdate(i, j + ((Random.Range(0, 2) == 0) ? 1 : -1)); // then randomly delete a obs
                        return;
                    }
                }
            } // end of for: j
        } // end of for: i
    }

    public void DestroyAllObstacles()
    {
        GameObject[] obs = GameObject.FindGameObjectsWithTag("Obstacle");
        int k = 0;
        for (; k < obs.Length; k++)
        {
            Destroy(obs[k]);
            obs[k] = null;
        }
        positionList .Clear();
        //Debug.Log("Destroyed " + k + " obs");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
