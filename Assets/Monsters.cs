using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FACING : int {UP = 0, LEFT, DOWN, RIGHT};

public class Monster
{
    public bool isActive;
    public FACING faceTo;
    public int h;
    public int w;
    public int index;
    public GameObject monSpriteObject;

    public Monster(int _h, int _w, int _index, GameObject ms)
    {
        isActive = false;
        faceTo = (FACING) Random.Range(0, 4);
        h = _h;
        w = _w;
        index = _index;
        monSpriteObject = ms;
    }

    public void MoveTo(int newh, int neww)
    {
        h = newh;
        w = neww;
    }
}

public class MonsterPosComparer : IComparer<Monster>
{
    public int Compare(Monster x, Monster y)
    {
        if (x.h < y.h) return -1;
        else if (x.h == y.h)
        {
            if (x.w < y.w) return -1;
            else if (x.w == y.w) return 0;
        }
        return 1;
    }
}

public class Monsters : MonoBehaviour {

    public List<Monster> monsterList = null;  // store obstacle position in as integer(h * width + w)
    public Level_Map levelMap;
    public GameObject prototype;
    MonsterPosComparer mpc = new MonsterPosComparer();

    // Use this for initialization
    void Start()
    {
        Debug.Log("Monsters.Start()");
    }

    public void Initialize()
    {
        monsterList = new List<Monster>();
        levelMap = gameObject.GetComponent<Level_Map>();
        prototype = GameObject.Find("Prototype Monster Sprite");
    }

    public void Generate(int totalNum)
    {
        int minDisBtwnMons = 6;
        int posRandMin = (levelMap.blocks.Length - levelMap.wallsNumber) / totalNum;
        int posRandMax = (levelMap.blocks.Length - levelMap.wallsNumber) / totalNum + (int)(minDisBtwnMons * 2);
        int spawnedCount = 0;
        int emegercyJumpOut = 0;
        int tryLimit = totalNum * 2, tryCount = 0;
        int h = -1, w = -1;
        int mapWidth = levelMap.width;
        int prePos = levelMap.playerStartBlock[0] * mapWidth + levelMap.playerStartBlock[1];
        //int[] prePos = new int[2] {levelMap.playerStartBlock[0] , levelMap.playerStartBlock[1]};

        /* LINEAR SPAWN */
        while(spawnedCount < totalNum)
        {
            // make random pos
            int pos = prePos;
            pos += Random.Range(posRandMin, posRandMax);
            // check map range
            if (pos > (levelMap.height - 1) * mapWidth)
            {
                pos -= (levelMap.height - 2) * mapWidth;
            }
            h = pos / mapWidth;
            w = pos % mapWidth;
            bool tooClose = false;
            // check if too close to player or finsh
            if (minDisBtwnMons > (System.Math.Abs(levelMap.playerStartBlock[0] - h) + System.Math.Abs(levelMap.playerStartBlock[1] - w))
             || minDisBtwnMons > (System.Math.Abs(levelMap.finishBlock[0] - h) + System.Math.Abs(levelMap.finishBlock[1] - w)))
                tooClose = true;
            // check if too close to other monster
            for (int i = 0; i < monsterList.Count && !tooClose; ++i)
            {
                if (minDisBtwnMons > (System.Math.Abs(monsterList[i].h - h) + System.Math.Abs(monsterList[i].w - w)))
                    tooClose = true;
            }
            if (tooClose) prePos = pos;
            else if (levelMap.blocks[h, w] != (int)Level_Map.BLOCK_TYPE.WALL && !levelMap.theObstacles.positionList.Exists(x => x == (h * mapWidth + w)))
            {
                // not too close and this is not wall/obstacle
                // check if is stuck
                int walkable_neighbor_count = 0;
                int direction = 0;
                int h_tocheck = 0, w_tocheck = 0;
                while(direction < 4)
                {
                    h_tocheck = h;
                    w_tocheck = w;
                    switch (direction)
                    {
                        case 0: // top
                            h_tocheck--;
                            break;
                        case 1: // left
                            w_tocheck--;
                            break;
                        case 2: // down
                            h_tocheck++;
                            break;
                        case 3: // right
                            w_tocheck++;
                            break;
                    }
                    direction++;
                    if (levelMap.blocks[h_tocheck, w_tocheck] != (int)Level_Map.BLOCK_TYPE.WALL || !levelMap.theObstacles.positionList.Exists(x => x == (h_tocheck * mapWidth + w_tocheck)))
                        walkable_neighbor_count++;
                }
                if(walkable_neighbor_count > 0)
                {
                    Spawn(h, w, spawnedCount);
                    spawnedCount++;
                }
            }
        }
        /**/
        /* TREE SPAWN 
        while (spawnedCount < totalNum && emegercyJumpOut < totalNum * 2)
        {
            prePos = new int[2] { levelMap.playerStartBlock[0], levelMap.playerStartBlock[1] };
            tryCount = 0;
            while (tryCount < tryLimit)
            {
                h = prePos[0];
                w = prePos[1];
                // make random h
                if (prePos[0] > levelMap.height - minDisBtwnMons)   h -= Random.Range((minDisBtwnMons + posRandMin) / 2, posRandMax / 2);
                else if (prePos[0] < minDisBtwnMons)                h += Random.Range((minDisBtwnMons + posRandMin) / 2, posRandMax / 2);
                else                                        h += (Random.Range((minDisBtwnMons + posRandMin) / 2, posRandMax / 2) * (Random.Range(-1, 1) == 0 ? 1 : -1));
                // make random w
                if (prePos[1] > levelMap.width - minDisBtwnMons)    w -= Random.Range((minDisBtwnMons + posRandMin) / 2, posRandMax / 2);
                else if (prePos[1] < minDisBtwnMons)                w += Random.Range((minDisBtwnMons + posRandMin) / 2, posRandMax / 2);
                else                                        w += (Random.Range((minDisBtwnMons + posRandMin) / 2, posRandMax / 2) * (Random.Range(-1, 1) == 0 ? 1 : -1));
                //Debug.Log("Monster Spawn Try:" + h + ", " + w);
                tryCount++;

                // check boundry
                if (h < 1 || h > levelMap.height - 1 || w < 1 || w > levelMap.width - 1)
                {
                    continue;
                }
                // is a valid leaf, no matter it will spawn or not
                prePos = new int[2] { h, w };
                emegercyJumpOut++;

                // check if too close to other monsters
                bool tooClose = false;
                if (minDisBtwnMons > (System.Math.Abs(levelMap.playerStartBlock[0] - h) + System.Math.Abs(levelMap.playerStartBlock[1] - w))
                    || minDisBtwnMons > (System.Math.Abs(levelMap.finishBlock[0] - h) + System.Math.Abs(levelMap.finishBlock[1] - w)))
                {
                    tooClose = true;
                }
                for (int i = 0; i < monsterList.Count; ++i)
                {
                    if (tooClose = (minDisBtwnMons > System.Math.Abs(monsterList[i].h - h) + System.Math.Abs(monsterList[i].w - w)))
                        break; // break this for
                }
                // if it is too close, dont spawn
                if (tooClose) 
                {
                    continue;
                }

                // check if is WALL or Obs
                if (levelMap.blocks[h, w] == (int)Level_Map.BLOCK_TYPE.WALL || levelMap.theObstacles.positionList.Exists(x => x == (h * levelMap.width + w)))
                {
                    continue;
                }

                // check if is stuck
                int walkable_neighbor_count = 0, direction = 0;
                int h_tocheck = 0, w_tocheck = 0;
                while (direction < 4)
                {
                    h_tocheck = h;
                    w_tocheck = w;
                    switch (direction)
                    {
                        case 0: // top
                            h_tocheck--;
                            break;
                        case 1: // left
                            w_tocheck--;
                            break;
                        case 2: // down
                            h_tocheck++;
                            break;
                        case 3: // right
                            w_tocheck++;
                            break;
                    }
                    direction++;
                    if (levelMap.blocks[h_tocheck, w_tocheck] != (int)Level_Map.BLOCK_TYPE.WALL || !levelMap.theObstacles.positionList.Exists(x => x == (h_tocheck * mapWidth + w_tocheck)))
                        walkable_neighbor_count++;
                }
                if (walkable_neighbor_count > 0)
                {
                    Spawn(h, w, spawnedCount);
                    spawnedCount++;
                }
            }
            // even if doesnt find a good spot, we continue anyway, but it just dont spawn enough monsters
        }
        */

        Debug.Log("Monster Ganeration: " + monsterList.Count + "mons are spawned.");
    }

    public void Spawn(int h, int w, int index)
    {
        //Debug.Log("monster creation happened");
        Vector3 trans = new Vector3((w - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - h - 0.5f), 0);
        GameObject created = Instantiate(prototype);
        created.name = "Monster Sprite" + index.ToString();
        created.tag = "Monster";
        created.transform.parent = GameObject.Find("Game Panel").transform;
        created.transform.position = trans;
        monsterList.Add(new Monster(h, w, index, created));
        monsterList.Sort(mpc);
    }

    public void Kill(int pos)
    {
        
    }

    public void MonstersPosUpdate()
    {
        //Debug.Log("theMonster.MonstersPosUpdate()");
        int mapWidth = levelMap.width;
        int tryCount = 0;
        for (int i = 0; i < monsterList.Count; i++)
        {
            int goingTo = Random.Range(0, 4);
            int newh = monsterList[i].h, neww = monsterList[i].w;
            switch(goingTo)
            {
                case 0:
                    newh--;
                    break;
                case 1:
                    neww--;
                    break;
                case 2:
                    newh++;
                    break;
                case 3:
                    neww++;
                    break;
                default:
                    break;
            }
            //Debug.Log("montser try" + newh + "," + neww);
            if (levelMap.blocks[newh, neww] != (int)Level_Map.BLOCK_TYPE.WALL && !levelMap.theObstacles.positionList.Exists(x => x == (newh * mapWidth + neww)))
            {
                int j = 0;
                for (; j < monsterList.Count; j++)
                {
                    if (monsterList[i].h == newh && monsterList[i].w == neww)
                        break;
                }
                if(j == monsterList.Count)
                {
                    //Debug.Log("Monster " + i + "moved from " + monsterList[i].h + "," + monsterList[i].w + " to " + newh + "," + neww);
                    monsterList[i].MoveTo(newh, neww);
                    monsterList[i].monSpriteObject.transform.position = new Vector3((neww - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - newh - 0.5f), 0);
                    monsterList[i].faceTo = (FACING) goingTo;
                }
            }
            else if (tryCount < 2)
            {
                tryCount++;
                continue;
            }
        }
    }
	
    public void DestroyMonsters()
    {
        GameObject[] mon = GameObject.FindGameObjectsWithTag("Monster");
        monsterList = new List<Monster>();
        int k = 0;
        for (; k < mon.Length; k++)
        {
            Destroy(mon[k]);
            mon[k] = null;
        }
        Debug.Log("destroy " + k + " monsters");
    }

	// Update is called once per frame
	void Update () {
		
	}
}
