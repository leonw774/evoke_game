using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;

public class Monsters : MonoBehaviour {

    public List<int> positionList = null;  // store obstacle position in as integer(h * width + w)
    public Level_Map levelMap;
    public GameObject prototype;
    public int totalNum;

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

    public void GenerateMonsters(int num)
    {
        int minDBetweenMs = 6;
        int spawnCount = 0;
        int mapWidth = levelMap.width;
        int emegercyJumpOut = 0;
        int tryLimit = 8, tryCount = 0;
        int h = -1, w = -1;
        int[] prePos = new int[2] {levelMap.playerStartBlock[0] , levelMap.playerStartBlock[1]}; 
        totalNum = num;
        while (spawnCount < totalNum && emegercyJumpOut < totalNum * 4)
        {
            emegercyJumpOut++;
            while (tryCount < tryLimit)
            {
                // make random h
                if (prePos[0] > levelMap.height - 1)
                    h = prePos[0] - Random.Range(minDBetweenMs / 2 - 2, minDBetweenMs / 2 + 2);
                else if (prePos[0] < 1)
                    h = prePos[0] + Random.Range(minDBetweenMs / 2 - 2, minDBetweenMs / 2 + 2);
                else
                    h = prePos[0] + ((Random.Range(-1, 1) == 0) ? 1 : -1) * Random.Range(minDBetweenMs / 2 - 2, minDBetweenMs / 2 + 2);
                // make random w
                if (prePos[1] > levelMap.width - 1)
                    w = prePos[1] - Random.Range(minDBetweenMs / 2 - 2, minDBetweenMs / 2 + 2);
                else if (prePos[1] < 1)
                    w = prePos[1] + Random.Range(minDBetweenMs / 2 - 2, minDBetweenMs / 2 + 2);
                else
                    w = prePos[1] + ((Random.Range(-1, 1) == 0) ? 1 : -1) * Random.Range(minDBetweenMs / 2 - 2, minDBetweenMs / 2 + 2);
                tryCount++;
                Debug.Log("Monster Spawn Try:" + h + ", " + w);
                // check boundry
                if (h < 1 || h > levelMap.height - 1 || w < 1 || w > mapWidth - 1)
                    continue;
                Debug.Log("boundry pass");
                // check if is WALL or Obs
                if (levelMap.blocks[h, w] == (int)Level_Map.BLOCK_TYPE.WALL || levelMap.theObstacles.positionList.Exists(x => x == (h * mapWidth + w)))
                    continue;
                Debug.Log("no wall and obs");
                // check if too close to other monsters
                bool tooClose = true;
                for (int i = 0; i < positionList.Count; ++i)
                {
                    if (minDBetweenMs > System.Math.Abs(positionList[i] / mapWidth - h) + System.Math.Abs(positionList[i] % mapWidth - w))
                    {
                        tooClose = false;
                    }
                }
                if (!tooClose)
                {
                    Debug.Log("Monster Spawn Success!");
                    Spawn(h, w);
                    spawnCount++;
                    break; // to break while: tryCount
                }
                prePos = new int[2] { h, w };
            }
            // even if doesnt find a good spot, we continue anyway, just dont spwan
        }
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
        
    }

	// Update is called once per frame
	void Update () {
		
	}
}
