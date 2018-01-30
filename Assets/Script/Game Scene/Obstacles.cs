using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour {

    public List<int> positionList = null;  // store obstacle position in as integer(h * width + w)
    private Level_Map levelMap;
    private GameObject prototype;

    // Use this for initialization
    void Start()
    {
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
            DestroyAllObstacles();
            Generate();
            Adjust();
        } while (positionList.Count < (int) (walkableTilesNum * 0.36) || positionList.Count > (int)(walkableTilesNum * 0.64));
        CreateAllObstacles();
        Debug.Log("There are " + positionList.Count + " obs in map");
    }

    public void ObsCreate(int h, int w)
    {
        // Debug.Log("obs creation happened");
        int pos = h * levelMap.width + w;
        positionList.Add(pos);
        positionList.Sort();
        Vector3 trans = levelMap.MapCoordToWorldVec3(h, w, 0);
        GameObject created = Instantiate(prototype);
        created.name = "Obstacle Sprite" + pos.ToString();
        created.tag = "Obstacle";
        created.transform.parent = GameObject.Find("Game Panel").transform;
        created.transform.localPosition = trans;
        created.transform.localScale = Vector3.one;
    }

    public void ObsCreate(int pos)
    {
        ObsCreate(pos / levelMap.width, pos % levelMap.width);
    }

    public void ObsDestroy(int pos)
    {
        GameObject destroy = GameObject.Find("Obstacle Sprite" + pos.ToString());
        if (positionList.Exists(x => x == pos) || destroy != null)
        {
            positionList.Remove(pos);
            Destroy(destroy);
        }
    }

    // Warning: this function is to update the list only
    // not the actul sprite
    // use it with discretion
    private void ListUpdate(int pos)
    {
        int h = pos / levelMap.width, w = pos % levelMap.height;
        if (positionList.Exists(x => x == pos))
            positionList.Remove(pos);
        else if (levelMap.tiles[h, w] == 0)
        {
            positionList.Add(pos);
            positionList.Sort();
        }
    }

    private void ListUpdate(int h, int w)
    {
        int pos = h * levelMap.width + w;
        if (positionList.Exists(x => x == pos))
            positionList.Remove(pos);
        else if (levelMap.tiles[h, w] == 0)
        {
            positionList.Add(pos);
            positionList.Sort();
        }
    }

    public void Generate()
    {
        int obslength = 0;
        bool putObs = false;
        // make horizonal obs
        for (int i = 1; i < levelMap.height - 1; ++i) // for every height
        {
            putObs = Random.Range(-5, 5) > 0;
            for (int j = 1; j < levelMap.width - 1; ++j)
            {
                if (putObs && levelMap.tiles[i, j] == TILE_TYPE.WALKABLE)
                {
                    // STATE: PUT OBS
                    ListUpdate(i, j);
                    obslength++;
                    if (Random.Range(-5, 5) < 0 || obslength > Random.Range(5, 9)) // possibility to change state
                    {
                        putObs = !putObs;
                        obslength = 0;
                    }
                }
                // STATE: DONT PUT OBS
                else if (Random.Range(-5, 9) < 0) // possibility to change state 
                {
                    
                    putObs = !putObs;
                }
            }
        }
        obslength = 0;
        // make vertical obs
        for (int j = 1; j < levelMap.width - 1; ++j)
        {
            putObs = Random.Range(-5, 5) > 0;
            for (int i = levelMap.height - 2; i > 0; --i)
            {
                if (putObs && levelMap.tiles[i, j] == TILE_TYPE.WALKABLE)
                {
                    ListUpdate(i, j);
                    obslength++;
                    if (Random.Range(-5, 5) < 0 || obslength > Random.Range(5, 9))
                    {
                        putObs = !putObs;
                        obslength = 0;
                    }
                }
                else if (Random.Range(-5, 9) < 0)
                {
                    putObs = !putObs;  
                }
            }
        }
    }

    public void Adjust()
    {
        int count = 0;
        bool find_something_to_adjust = true;
        while(find_something_to_adjust && count <= 2)
        {
            find_something_to_adjust = DistributeAdjust();
            CorridorAdjust();
            count++;
        }
        //Debug.Log("Obstacles Adjusted");

        // in opening, too much obstacles should not neighbor or be on same block of the player and finish
        int playerPosition = levelMap.playerStartTile[0] * levelMap.width + levelMap.playerStartTile[1];
        positionList.Remove(playerPosition);
        positionList.Remove(playerPosition + 1);
        positionList.Remove(playerPosition - 1);
        positionList.Remove(playerPosition + levelMap.width);
        positionList.Remove(playerPosition - levelMap.width);
    }

    private bool DistributeAdjust()
    {
        int thisTilePos = -1;
        int chi_of_this = 4;
        int di = -1, dj = -1, sameNeighborCount = 0;
        bool some_adjustment_are_done = false, this_is_obs = false, neighbor_is_obs = false;
        // adjust too crowded and too loosen obstacles
        for (int i = 1; i < levelMap.height - 1; ++i)
        {
            for (int j = 1; j < levelMap.width - 1; ++j)
            {
                thisTilePos = i * levelMap.width + j;
                if (levelMap.tiles[i, j] != TILE_TYPE.WALKABLE)
                    continue;
                // else
                this_is_obs = (positionList.Exists(x => x == thisTilePos));
                chi_of_this = 4;
                di = -1;
                dj = -1;
                sameNeighborCount = 0;
                int neighborTilePos = 0;
                while (di <= 1)
                {
                    if (di == 0 & dj == 0) continue;
                    neighborTilePos = (i + di) * levelMap.width + (j + dj);
                    // no matter the neighbor block is really a obstacle or a wall, it all count as obstacle
                    neighbor_is_obs = (positionList.Exists(x => x == neighborTilePos)) || (levelMap.tiles[i + di, j + dj] != TILE_TYPE.WALKABLE);
                    // check if neighbor is same with this block
                    if (this_is_obs == neighbor_is_obs)
                    {
                        sameNeighborCount++;
                        if(di == 0 || dj == 0)
                            chi_of_this--;
                    }
                    // upadte neighbor tiles ij
                    if (dj == 1)
                    {
                        di++;
                        dj = -1;
                    }
                    else if (di == 0 && dj == -1) dj = 1;
                    else dj++;
                }
                if (sameNeighborCount >= (this_is_obs ? 7 : 5) - ((chi_of_this < 1) ? 1 : 0))
                {
                    some_adjustment_are_done = true;
                    ListUpdate(i, j);
                    if (sameNeighborCount == 8)
                    {
                        ListUpdate(i + Random.Range(-1, 2), j);
                        ListUpdate(i, j + Random.Range(-1, 2));
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
        bool is_up_all_obs = true,
             is_down_all_obs = true,
             is_left_all_obs = true,
             is_right_all_obs = true,
             is_middle_all_walkable = true;
        for (int i = 1; i < levelMap.height - 1; ++i)
        {
            for (int j = 1; j < levelMap.width - 1; ++j)
            {
                if (levelMap.tiles[i, j] == TILE_TYPE.WALKABLE)
                {
                    int pos = i * levelMap.width + j, mw = levelMap.width, d = -1;
                    is_up_all_obs = is_down_all_obs = is_left_all_obs = is_right_all_obs = is_middle_all_walkable = true;
                    // check vertical corridor
                    while (d <= 1)
                    {
                        // Note: Map.TILE_TYPE.WALL is 1
                        is_up_all_obs = is_up_all_obs && (positionList.Exists(x => x == pos - mw + d) || levelMap.tiles[i - 1, j + d] == TILE_TYPE.WALL);
                        is_down_all_obs = is_down_all_obs && (positionList.Exists(x => x == pos + mw + d) || levelMap.tiles[i + 1, j + d] == TILE_TYPE.WALL);
                        is_middle_all_walkable = is_middle_all_walkable && !(positionList.Exists(x => x == pos + d) || levelMap.tiles[i, j + d] == TILE_TYPE.WALL);
                        d++;
                    }
                    if (is_middle_all_walkable && is_up_all_obs && is_down_all_obs && Random.Range(0, 24) > 0)
                    {
                        ListUpdate(i, j); // add an obs in the walk way
                        ListUpdate(i + ((Random.Range(0, 2) == 0) ? 1 : -1), j);// then randomly delete a obs
                        return;
                    }
                    // check horizontal corridor
                    d = -1;
                    is_middle_all_walkable = true;
                    while (d <= 1)
                    {
                        is_left_all_obs = is_left_all_obs && (positionList.Exists(x => x == pos + d * mw - 1) || levelMap.tiles[i + d, j - 1] == TILE_TYPE.WALL);
                        is_right_all_obs = is_right_all_obs && (positionList.Exists(x => x == pos + d * mw + 1) || levelMap.tiles[i + d, j + 1] == TILE_TYPE.WALL);
                        is_middle_all_walkable = is_middle_all_walkable && !(positionList.Exists(x => x == pos + d * mw) || levelMap.tiles[i + d, j] == TILE_TYPE.WALL);
                        d++;
                    }
                    if (is_middle_all_walkable && is_left_all_obs && is_right_all_obs && Random.Range(0, 24) > 0)
                    {
                        ListUpdate(i, j); // add an obs in the walk way
                        ListUpdate(i, j + ((Random.Range(0, 2) == 0) ? 1 : -1)); // then randomly delete a obs
                    }
                }
            } // end of for: j
        } // end of for: i
    }

    public void DestroyAllObstacles()
    {
        GameObject[] obs = GameObject.FindGameObjectsWithTag("Obstacle");
        int n = 0;
        for (; n < obs.Length; n++)
        {
            Destroy(obs[n]);
            obs[n] = null;
        }
        positionList.Clear();
        //Debug.Log("Destroyed " + k + " obs");
    }

    private void CreateAllObstacles()
    {
        int h, w;
        positionList.ForEach
        (
            delegate (int pos)
            {
                h = pos / levelMap.width;
                w = pos % levelMap.width;
                Vector3 trans = levelMap.MapCoordToWorldVec3(h, w, 0);
                GameObject created = Instantiate(prototype);
                created.name = "Obstacle Sprite" + pos.ToString();
                created.tag = "Obstacle";
                created.transform.parent = GameObject.Find("Game Panel").transform;
                created.transform.localPosition = trans;
                created.transform.localScale = Vector3.one;
            }
        );
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
