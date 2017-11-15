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
    public GameObject thisMonsterSprite;

    public Monster(int _h, int _w, GameObject ms)
    {
        isActive = false;
        faceTo = (FACING) Random.Range(0, 4);
        h = _h;
        w = _w;
        thisMonsterSprite = ms;
    }

    public void MoveTo(int newh, int neww)
    {
        h = newh;
        w = neww;
        thisMonsterSprite.transform.position = thisMonsterSprite.transform.position + new Vector3(newh - h, neww - w, 0);
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
        int minDistanceBetweenMonsters = 6;
        int posRandomRangeMin = (levelMap.blocks.Length - levelMap.wallsNumber) / totalNum;
        int posRandomRangeMax = (levelMap.blocks.Length - levelMap.wallsNumber) / totalNum + (int)(minDistanceBetweenMonsters * 2);
        int spawnedCount = 0;
        int emegercyJumpOut = 0;
        int tryLimit = totalNum * 4, tryCount = 0;
        int h = -1, w = -1;
        int mapWidth = levelMap.width;
        int prePos = levelMap.playerStartBlock[0] * levelMap.width + levelMap.playerStartBlock[1];
        //int[] prePos = new int[2] {levelMap.playerStartBlock[0] , levelMap.playerStartBlock[1]};

        /* LINEAR SPAWN */
        while(spawnedCount < totalNum)
        {
            // make random pos
            int pos = prePos;
            pos += Random.Range(posRandomRangeMin, posRandomRangeMax);
            // check map range
            if (pos > (levelMap.height - 1) * levelMap.width)
            {
                pos -= (levelMap.height - 2) * levelMap.width;
            }
            h = pos / levelMap.width;
            w = pos % levelMap.width;
            // check if too close to other monster
            bool tooClose = false;
            if (minDistanceBetweenMonsters > (System.Math.Abs(levelMap.playerStartBlock[0] - h) + System.Math.Abs(levelMap.playerStartBlock[1] - w)))
                tooClose = true;
            else if (minDistanceBetweenMonsters > (System.Math.Abs(levelMap.finishBlock[0] - h) + System.Math.Abs(levelMap.finishBlock[1] - w)))
                tooClose = true;
            for (int i = 0; i < monsterList.Count && !tooClose; ++i)
            {
                if (minDistanceBetweenMonsters > (System.Math.Abs(monsterList[i].h - h) + System.Math.Abs(monsterList[i].w - w)))
                    tooClose = true;
            }
            if (tooClose) prePos = pos;
            else if (levelMap.blocks[h, w] != (int)Level_Map.BLOCK_TYPE.WALL && !levelMap.theObstacles.positionList.Exists(x => x == (h * levelMap.width + w)))
            {   
                // not too close and this is not wall/obstacle, then make a monster here
                Spawn(h, w, spawnedCount);
                Debug.Log("Spawn a monster at " + h + ", " + w);
                spawnedCount++;
            }
        }
        /**/
        /* TREE SPAWN
        while (spawnedCount < totalNum && emegercyJumpOut < totalNum * 4)
        {
            prePos = new int[2] { levelMap.playerStartBlock[0], levelMap.playerStartBlock[1] };
            tryCount = 0;
            while (tryCount < tryLimit)
            {
                // make random h
                if (prePos[0] > levelMap.height - minDistanceBetweenMonsters)
                    h = prePos[0] - Random.Range(minDistanceBetweenMonsters / 2, minDistanceBetweenMonsters * 3 / 2);
                else if (prePos[0] < minDistanceBetweenMonsters)
                    h = prePos[0] + Random.Range(minDistanceBetweenMonsters / 2, minDistanceBetweenMonsters * 3 / 2);
                else
                    h = prePos[0] + ((Random.Range(-1, 1) == 0) ? 1 : -1) * Random.Range(minDistanceBetweenMonsters / 2, minDistanceBetweenMonsters * 3 / 2);
                // make random w
                if (prePos[1] > levelMap.width - minDistanceBetweenMonsters)
                    w = prePos[1] - Random.Range(minDistanceBetweenMonsters / 2, minDistanceBetweenMonsters * 3 / 2);
                else if (prePos[1] < minDistanceBetweenMonsters)
                    w = prePos[1] + Random.Range(minDistanceBetweenMonsters / 2, minDistanceBetweenMonsters * 3 / 2);
                else
                    w = prePos[1] + ((Random.Range(-1, 1) == 0) ? 1 : -1) * Random.Range(minDistanceBetweenMonsters / 2, minDistanceBetweenMonsters * 3 / 2);
                Debug.Log("Monster Spawn Try:" + h + ", " + w);
                tryCount++;

                // check boundry
                if (h < 1 || h > levelMap.height - 1 || w < 1 || w > levelMap.width - 1)
                    continue;
                Debug.Log("boundry pass");
                // is a valid leaf, no matter it will spawn or not
                prePos = new int[2] { h, w };
                emegercyJumpOut++;

                // check if too close to other monsters
                bool tooClose = false;
                if (minDistanceBetweenMonsters > (System.Math.Abs(levelMap.playerStartBlock[0] - h) + System.Math.Abs(levelMap.playerStartBlock[1] - w))
                    || minDistanceBetweenMonsters > (System.Math.Abs(levelMap.finishBlock[0] - h) + System.Math.Abs(levelMap.finishBlock[1] - w)))
                {
                    tooClose = true;
                }
                for (int i = 0; i < monsterList.Count; ++i)
                {
                    if (tooClose = (minDistanceBetweenMonsters > System.Math.Abs(monsterList[i].pos / mapWidth - h) + System.Math.Abs(monsterList[i].pos % mapWidth - w)))
                        break; // break this for
                }
                // if it is too close, dont spawn
                if (tooClose)
                    continue;
                Debug.Log("distribution pass");

                // check if is WALL or Obs
                if (levelMap.blocks[h, w] == (int)Level_Map.BLOCK_TYPE.WALL || levelMap.theObstacles.positionList.Exists(x => x == (h * levelMap.width + w)))
                    continue;
                else
                {
                    Debug.Log("no wall and obs");
                    Spawn(h, w, spawnedCount);
                    spawnedCount++;
                    break; // to break while: tryCount
                }
            }
            // even if doesnt find a good spot, we continue anyway, but it just dont spawn enough monsters
        }
        */

        Debug.Log("Monster Ganeration: " + monsterList.Count + "mons are spawned.");
    }

    public void Spawn(int h, int w, int index)
    {
        int pos = h * levelMap.width + w;
        Debug.Log("monster creation happened");
        Vector3 trans = new Vector3((w - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - h - 0.5f), 0);
        GameObject created = Instantiate(prototype);
        created.name = "Monster Sprite" + index.ToString();
        created.tag = "Monster";
        created.transform.parent = GameObject.Find("Game Panel").transform;
        created.transform.position = trans;
        monsterList.Add(new Monster(h, w, created));
        monsterList.Sort(mpc);
    }

    public void Kill(int pos)
    {
        
    }

    public void MonstersPosUpdate()
    {
        int mapWidth = levelMap.width;
        for (int i = 0; i < monsterList.Count; i++)
        {
            int goingTo = Random.Range(0, 4);
            int newh = 0, neww = 0;
            switch(goingTo)
            {
                case 0:
                    newh = monsterList[i].h - 1;
                    break;
                case 1:
                    neww = monsterList[i].w - 1;
                    break;
                case 2:
                    newh = monsterList[i].h + 1;
                    break;
                case 3:
                    neww = monsterList[i].h - 1;
                    break;
                default:
                    break;
            }
            if (levelMap.blocks[newh, neww] != (int)Level_Map.BLOCK_TYPE.WALL && !levelMap.theObstacles.positionList.Exists(x => x == (newh * mapWidth + neww)))
            {
                monsterList[i].MoveTo(newh, neww);
                monsterList[i].faceTo = (FACING) goingTo;
            }
        }
    }
	
    public void DestroyMonsters()
    {
        GameObject[] mon = GameObject.FindGameObjectsWithTag("Monster");
        int k = 0;
        for (; k < mon.Length; k++)
        {
            Destroy(mon[k]);
            mon[k] = null;
        }
        monsterList = new List<Monster>();
        Debug.Log("destroy " + k + " monsters");
    }

	// Update is called once per frame
	void Update () {
		
	}
}
