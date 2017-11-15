using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;

public class Monsters : MonoBehaviour {

    public List<int> positionList = null;  // store obstacle position in as integer(h * width + w)
    public Level_Map levelMap;
    public GameObject prototype;

    // Use this for initialization
    void Start()
    {

    }

    public void Initialize()
    {
        positionList = new List<int>();
        levelMap = gameObject.GetComponent<Level_Map>();
        prototype = GameObject.Find("Prototype Monster Sprite");
    }

    public void Generate(int totalNum)
    {
        int minDistanceBetweenMonsters = 5;
        int posRandomRangeMin = (levelMap.blocks.Length - levelMap.wallsNumber) / totalNum;
        int posRandomRangeMax = (levelMap.blocks.Length - levelMap.wallsNumber) / totalNum + (int)(minDistanceBetweenMonsters * 2);
        int spawnedCount = 0;
        int emegercyJumpOut = 0;
        int tryLimit = 12, tryCount = 0;
        int h = -1, w = -1;
        int prePos = levelMap.playerStartBlock[0] * levelMap.width + levelMap.playerStartBlock[1];
        //int[] prePos = new int[2] {levelMap.playerStartBlock[0] , levelMap.playerStartBlock[1]};

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
            for (int i = 0; i < positionList.Count && !tooClose; ++i)
            {
                if (minDistanceBetweenMonsters > (System.Math.Abs(positionList[i] / levelMap.width - h) + System.Math.Abs(positionList[i] % levelMap.width - w)))
                    tooClose = true;
            }
            if (tooClose) prePos = pos;
            else if (levelMap.blocks[h, w] != (int)Level_Map.BLOCK_TYPE.WALL && !levelMap.theObstacles.positionList.Exists(x => x == (h * levelMap.width + w)))
            {   
                // not too close and this is not wall/obstacle, then make a monster here
                Spawn(h, w);
                spawnedCount++;
            }
        }

        /*
        while (spawnedCount < totalNum && emegercyJumpOut < totalNum * 8)
        {
            prePos = new int[2] { levelMap.playerStartBlock[0], levelMap.playerStartBlock[1] };
            tryCount = 0;
            while (tryCount < tryLimit)
            {
                // make random h
                if (prePos[0] > levelMap.height - 1)
                    h = prePos[0] - Random.Range(minDistanceBetweenMonsters / 2, minDistanceBetweenMonsters);
                else if (prePos[0] < 1)
                    h = prePos[0] + Random.Range(minDistanceBetweenMonsters / 2, minDistanceBetweenMonsters);
                else
                    h = prePos[0] + ((Random.Range(-1, 1) == 0) ? 1 : -1) * Random.Range(minDistanceBetweenMonsters / 2, minDistanceBetweenMonsters);
                // make random w
                if (prePos[1] > levelMap.width - 1)
                    w = prePos[1] - Random.Range(minDistanceBetweenMonsters / 2, minDistanceBetweenMonsters);
                else if (prePos[1] < 1)
                    w = prePos[1] + Random.Range(minDistanceBetweenMonsters / 2, minDistanceBetweenMonsters);
                else
                    w = prePos[1] + ((Random.Range(-1, 1) == 0) ? 1 : -1) * Random.Range(minDistanceBetweenMonsters / 2, minDistanceBetweenMonsters);
                Debug.Log("Monster Spawn Try:" + h + ", " + w);
                tryCount++;
                // check boundry
                if (h < 1 || h > levelMap.height - 1 || w < 1 || w > levelMap.width - 1)
                {
                    continue;
                }
                emegercyJumpOut++;
                Debug.Log("boundry pass");
                // check if too close to other monsters
                bool tooClose = false;
                if (minDistanceBetweenMonsters > (System.Math.Abs(levelMap.playerStartBlock[0] - h) + System.Math.Abs(levelMap.playerStartBlock[1] - w)))
                {
                    tooClose = true;
                }
                else if (minDistanceBetweenMonsters > (System.Math.Abs(levelMap.finishBlock[0] - h) + System.Math.Abs(levelMap.finishBlock[1] - w)))
                {
                    tooClose = true;
                }
                for (int i = 0; i < positionList.Count; ++i)
                {
                    if (minDistanceBetweenMonsters > (System.Math.Abs(positionList[i] / levelMap.width - h) + System.Math.Abs(positionList[i] % levelMap.width - w)))
                    {
                        tooClose = true;
                        break; // break this for
                    }
                }
                if (!tooClose)
                {
                    Debug.Log("distribution pass");
                    prePos = new int[2] { h, w };
                }
                else
                {
                    continue;
                }
                // check if is WALL or Obs
                if (levelMap.blocks[h, w] == (int)Level_Map.BLOCK_TYPE.WALL || levelMap.theObstacles.positionList.Exists(x => x == (h * levelMap.width + w)))
                    continue;
                else
                {
                    Debug.Log("no wall and obs");
                    Spawn(h, w);
                    spawnedCount++;
                    break; // to break while: tryCount
                }
            }
            // even if doesnt find a good spot, we continue anyway, just dont spwan
        }
        */

        Debug.Log("Monster Ganeration: " + positionList.Count + "mons are spawned.");
    }

    public void Spawn(int h, int w)
    {
        int pos = h * levelMap.width + w;
        Debug.Log("monster creation happened");
        Vector3 trans = new Vector3((w - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - h - 0.5f), 0);
        GameObject created = Instantiate(prototype);
        positionList.Add(pos);
        positionList.Sort();
        created.name = "Monster Sprite" + pos.ToString();
        created.tag = "Monster";
        created.transform.parent = GameObject.Find("Game Panel").transform;
        created.transform.position = trans;
    }

    public void Kill(int pos)
    {
        
    }

    public void MonsterPosUpdate()
    {
        
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
        positionList = new List<int>();
        Debug.Log("destroy " + k + " monsters");
    }

	// Update is called once per frame
	void Update () {
		
	}
}
