using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FACING : int {UP = 0, LEFT, DOWN, RIGHT};

public class Monster
{
    public FACING faceTo;
    public int h;
    public int w;
    public int id;
    public bool killed;
    public GameObject monSpriteObject = null;

    public Vector3 animBeginPos;
    public Vector3 animEndPos;

    public Monster(int _h, int _w, int _id, GameObject ms)
    {
        killed = false;
        faceTo = (FACING) Random.Range(0, 4);
        h = _h;
        w = _w;
        id = _id;
        monSpriteObject = ms;
        animBeginPos = new Vector3();
        animEndPos = new Vector3();
    }

    public void MoveTo(int newh, int neww)
    {
        h = newh;
        w = neww;
    }

}

public class Monsters : MonoBehaviour {

    private List<Monster> monsterList = null;  // store obstacle position in as integer(h * width + w)
    private Level_Map levelMap;
    public GameObject prototype;
    public Sprite sprite_frame1, sprite_frame2;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Monsters.Start()");
    }

    public void Initialize()
    {
        monsterList = new List<Monster>();
        levelMap = gameObject.GetComponent<Level_Map>();
        prototype = GameObject.Find("Prototype Monster Sprite Frame 1");
        sprite_frame1 = prototype.GetComponent<SpriteRenderer>().sprite;
        sprite_frame2 = GameObject.Find("Prototype Monster Sprite Frame 2").GetComponent<SpriteRenderer>().sprite;
    }

    public void Generate(int totalNum)
    {
        if (totalNum == 0)
            return;

        int minDisBtwnMons = 5;
        int posRandMin = (levelMap.tiles.Length - levelMap.wallsNumber) / totalNum - minDisBtwnMons;
        int posRandMax = (levelMap.tiles.Length - levelMap.wallsNumber) / totalNum + (minDisBtwnMons * 2);
        int spawnedCount = 0;
        int emegercyJumpOut = 0;
        int h = -1, w = -1;
        int mapWidth = levelMap.width;
        int prePos = levelMap.playerStartTile[0] * mapWidth + levelMap.playerStartTile[1];
        //int[] prePos = new int[2] {levelMap.playerStartTile[0] , levelMap.playerStartTile[1]};

        Debug.Log("posRandMin:" + posRandMin);
        Debug.Log("posRandMax: " + posRandMax);

        /* ITERATIVE SPAWN */
        while (spawnedCount < totalNum)
        {
            if(emegercyJumpOut++ > (totalNum * 1024))
            {
                Debug.Log("Emegercy Jump-Out Happened.");
                Debug.Log("tryCount: " + emegercyJumpOut);
                Debug.Log("totalNum: " + totalNum);
                break;
            }
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
            if (minDisBtwnMons > (System.Math.Abs(levelMap.playerStartTile[0] - h) + System.Math.Abs(levelMap.playerStartTile[1] - w))
             || minDisBtwnMons > (System.Math.Abs(levelMap.finishTile[0] - h) + System.Math.Abs(levelMap.finishTile[1] - w)))
                tooClose = true;
            // check if too close to other monster
            for (int i = 0; i < monsterList.Count && !tooClose; ++i)
            {
                if (minDisBtwnMons > (System.Math.Abs(monsterList[i].h - h) + System.Math.Abs(monsterList[i].w - w)))
                    tooClose = true;
            }
            if (tooClose) prePos = pos;
            else if (levelMap.tiles[h, w] != (int)Level_Map.TILE_TYPE.WALL)
            {
                bool spawn_on_obs = levelMap.theObstacles.positionList.Exists(x => x == (h * mapWidth + w));
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
                            h_tocheck--; break;
                        case 1: // left
                            w_tocheck--; break;
                        case 2: // down
                            h_tocheck++; break;
                        case 3: // right
                            w_tocheck++; break;
                    }
                    direction++;
                    if (levelMap.tiles[h_tocheck, w_tocheck] != (int)Level_Map.TILE_TYPE.WALL && !levelMap.theObstacles.positionList.Exists(x => x == (h_tocheck * mapWidth + w_tocheck)))
                        walkable_neighbor_count++;
                }
                if (!spawn_on_obs && walkable_neighbor_count > 1)
                {
                    Spawn(h, w, spawnedCount);
                    spawnedCount++;
                }
                else if (spawn_on_obs && walkable_neighbor_count < 3 && walkable_neighbor_count > 1 && Random.Range(-1, 1) < 0)
                {
                    levelMap.theObstacles.ObsDestroy(pos);
                    Spawn(h, w, spawnedCount++);
                }
            }
        }
        /**/
        /* TREE SPAWN (is buggy)
        while (spawnedCount < totalNum && emegercyJumpOut < totalNum * 2)
        {
            prePos = new int[2] { levelMap.playerStartTile[0], levelMap.playerStartTile[1] };
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
                    continue;
                // is a valid leaf, no matter it will spawn or not
                prePos = new int[2] { h, w };
                emegercyJumpOut++;
                // check if too close to other monsters
                bool tooClose = false;
                if (minDisBtwnMons > (System.Math.Abs(levelMap.playerStartTile[0] - h) + System.Math.Abs(levelMap.playerStartTile[1] - w))
                    || minDisBtwnMons > (System.Math.Abs(levelMap.finishTile[0] - h) + System.Math.Abs(levelMap.finishTile[1] - w)))
                    tooClose = true;
                for (int i = 0; i < monsterList.Count; ++i)
                {
                    if (tooClose = (minDisBtwnMons > System.Math.Abs(monsterList[i].h - h) + System.Math.Abs(monsterList[i].w - w)))
                        break; // break this for
                }
                // if it is too close, dont spawn
                if (tooClose) continue;
                // check if is WALL or Obs
                if (levelMap.tiles[h, w] == (int)Level_Map.TILE_TYPE.WALL || levelMap.theObstacles.positionList.Exists(x => x == (h * levelMap.width + w)))
                    continue;
                // check if is stuck
                int walkable_neighbor_count = 0, direction = 0;
                int h_tocheck = 0, w_tocheck = 0;
                while (direction < 4)
                {
                    h_tocheck = h; w_tocheck = w;
                    switch (direction)
                    {
                        case 0: // top
                            h_tocheck--; break;
                        case 1: // left
                            w_tocheck--; break;
                        case 2: // down
                            h_tocheck++; break;
                        case 3: // right
                            w_tocheck++; break;
                    }
                    direction++;
                    if (levelMap.tiles[h_tocheck, w_tocheck] != (int)Level_Map.TILE_TYPE.WALL || !levelMap.theObstacles.positionList.Exists(x => x == (h_tocheck * mapWidth + w_tocheck)))
                        walkable_neighbor_count++;
                }
                if (walkable_neighbor_count > 0)
                {
                    Spawn(h, w, spawnedCount++);
                }
            }
            // even if doesnt find a good spot, we continue anyway, but it just dont spawn enough monsters
        }
        */

        Debug.Log("Monster Ganeration: " + monsterList.Count + "mons are spawned.");
    }

    public void Spawn(int h, int w, int index)
    {
        //Debug.Log("monster creation happened at " + h + "," + w);
        Vector3 trans = new Vector3((w - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - h - 0.5f), 0);
        GameObject created = Instantiate(prototype);
        created.name = "Monster Sprite" + index.ToString();
        created.tag = "Monster";
        created.transform.parent = GameObject.Find("Game Panel").transform;
        created.transform.position = trans;
        monsterList.Add(new Monster(h, w, index, created));
    }

    public bool TryAttackPlayer(int playerPos)
    {
        // true: attack success; false: attack fail
        int found = monsterList.FindIndex(x => x.h * levelMap.width + x.w == playerPos);
        if (found >= 0)
        {
            KillMonsterByIndex(found);
            Debug.Log("because it attacked player in last round");
            return true;
        }
        return false;
    }

    public void MonstersChangeFrame()
    {
        Sprite sp_to_change = null;
        for (int i = 0; i < monsterList.Count; i++)
        {
            if (i == 0)
            {
                if (monsterList[i].monSpriteObject.GetComponent<SpriteRenderer>().sprite == sprite_frame1)
                    sp_to_change = sprite_frame2;
                else
                    sp_to_change = sprite_frame1;
            }
            monsterList[i].monSpriteObject.GetComponent<SpriteRenderer>().sprite = sp_to_change;
        }
    }

    public void MonstersMove()
    {
        int monsterSensePlayer = 6; // == minDisBtwnMon
        for (int i = 0; i < monsterList.Count; i++)
        {
            // change position
            if (monsterSensePlayer >= (System.Math.Abs(levelMap.thePlayer.h - monsterList[i].h) + System.Math.Abs(levelMap.thePlayer.w - monsterList[i].w))
             && Random.Range(-1, 16) > 0)
                MonsterMoveToPlayer(i);
            else
                MonsterMoveRandom(i);
        }
    }

    private void MonsterMoveToPlayer(int i)
    {
        int goingTo = -1;
        Astar m_astar = new Astar(levelMap.tiles, levelMap.height, levelMap.width, levelMap.theObstacles.positionList, new int[2] {monsterList[i].h, monsterList[i].w}, new int[2] { levelMap.thePlayer.h, levelMap.thePlayer.w });
        m_astar.FindPathLength(false, true);
        List<int> pathList = m_astar.GetPath();
        if(pathList.Count > 1) goingTo = pathList[1];

        //for (int k = 0; k < pathList.Count; k++) Debug.Log("[" + k + "]" + ": " + pathList[k]);
        //Debug.Log("goingTo = " + goingTo);

        if (goingTo == -1 || pathList.Count > 16)
        { // Monster sense player but cannot find path //Debug.Log("try MonsterMoveToPlayer() failed");
            MonsterMoveRandom(i);
        }
        else
        {
            //Debug.Log("MonsterMoveToPlayer()");
            int newh = monsterList[i].h, neww = monsterList[i].w;
            switch(goingTo)
            {
                case 0: // up
                    newh--; break;
                case 1: // left
                    neww--; break;
                case 2: // down
                    newh++; break;
                case 3: // right
                    neww++; break;
            }
            if (levelMap.tiles[newh, neww] != (int)Level_Map.TILE_TYPE.WALL && !levelMap.theObstacles.positionList.Exists(x => x == (newh * levelMap.width + neww)))
            {
                int j = 0;
                for (; j < monsterList.Count; j++)
                { // there is another mon on the way
                    if (monsterList[j].h == newh && monsterList[j].w == neww) break;
                }
                if (j == monsterList.Count)
                {
                    //Debug.Log("Monster " + i + "moved from " + monsterList[i].h + "," + monsterList[i].w + " to " + newh + "," + neww);
                    monsterList[i].MoveTo(newh, neww);
                    monsterList[i].faceTo = (FACING) goingTo;
                    MonsterAnimSetup(i, monsterList[i].monSpriteObject.transform.position, new Vector3((neww - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - newh - 0.5f), 0));
                }
            }
        }
    }

    private void MonsterMoveRandom(int i)
    {
        //Debug.Log("MonsterMoveRandom()");
        int tryCount = 0;
        if (monsterList[i].h == levelMap.thePlayer.h && monsterList[i].w == levelMap.thePlayer.w)
            return;

        while (tryCount <= 8)
        {
            int goingTo = Random.Range(0, 4);
            int newh = monsterList[i].h, neww = monsterList[i].w;
            if (Random.Range(-1, 8) < 0) break; // monter wont move
            switch (goingTo)
            {
                case 0:
                    newh--; break;
                case 1:
                    neww--; break;
                case 2:
                    newh++; break;
                case 3:
                    neww++; break;
            }
            //Debug.Log("montser try" + newh + "," + neww);
            if (levelMap.tiles[newh, neww] != (int)Level_Map.TILE_TYPE.WALL
                && !levelMap.theObstacles.positionList.Exists(x => x == (newh * levelMap.width + neww))
                && (levelMap.thePlayer.h != newh || levelMap.thePlayer.w != neww))
            {
                int j = 0;
                for (; j < monsterList.Count; j++)
                { // there is another mon on the way
                    if (monsterList[j].h == newh && monsterList[j].w == neww) break;
                }
                if (j == monsterList.Count)
                {
                    //Debug.Log("Monster " + i + "moved from " + monsterList[i].h + "," + monsterList[i].w + " to " + newh + "," + neww);
                    monsterList[i].MoveTo(newh, neww);
                    monsterList[i].faceTo = (FACING) goingTo;
                    MonsterAnimSetup(i, monsterList[i].monSpriteObject.transform.position, new Vector3((neww - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - newh - 0.5f), 0));
                    break;
                }
            }
            tryCount++;
        } // end of while(trycount < 4)
    }

    private void MonsterAnimSetup(int index, Vector3 begin, Vector3 end)
    {
        monsterList[index].animBeginPos = begin;
        monsterList[index].animEndPos = end;
    }

    public bool MonstersAnim()
    {
        if (monsterList.Count > 0)
        {
            monsterList.ForEach(delegate(Monster x) {
                if (x.animBeginPos != new Vector3(0.0f, 0.0f, 0.0f))
                    x.monSpriteObject.transform.position = x.monSpriteObject.transform.position + (x.animEndPos - x.animBeginPos) / (Time.deltaTime / 0.0013f);
            });
            if (monsterList[0].animEndPos != new Vector3(0.0f, 0.0f, 0.0f)
                && (monsterList[0].animEndPos - monsterList[0].monSpriteObject.transform.position).normalized == (monsterList[0].animBeginPos - monsterList[0].animEndPos).normalized)
                return true;
        }
        return false;
    }

    public void MonstersAnimEnd()
    {
        monsterList.ForEach(delegate(Monster x) {
            if (x.animBeginPos != new Vector3(0.0f, 0.0f, 0.0f))
            {
                x.monSpriteObject.transform.position = x.animEndPos;
                x.animEndPos = new Vector3(0.0f, 0.0f, 0.0f);
                x.animBeginPos = new Vector3(0.0f, 0.0f, 0.0f);
            }
        });
    }

    private void KillMonsterByIndex(int i)
    {
        Debug.Log("destroy monster #" + monsterList[i].id);
        Destroy(monsterList[i].monSpriteObject, 0.15f);
        monsterList.RemoveAt(i);
    }

    public void TryKillMonsterByPos(int pos)
    {
        int found = monsterList.FindIndex(x => pos == x.h * levelMap.width + x.w);
        if (found >= 0)
            KillMonsterByIndex(found);
    }
    
    public void DestroyMonsters()
    {
        int k = 0;
        for (; k < monsterList.Count; k++)
        {
            Destroy(monsterList[k].monSpriteObject);
        }
        monsterList.Clear();
        Debug.Log("destroy " + k + " monsters");
    }

    // Update is called once per frame
    void Update ()
    {
		
	}
}
