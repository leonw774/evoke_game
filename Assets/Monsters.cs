﻿using System.Collections;
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
    public BossMonsterAbility bossAbility = null;
    public GameObject SpriteObj = null;

    public Vector3 animBeginPos;
    public Vector3 animEndPos;

    public Monster(int _h, int _w, int _id, GameObject ms)
    {
        h = _h;
        w = _w;
        id = _id;
        SpriteObj = ms;
        FaceTo((FACING)Random.Range(0, 4));
        animBeginPos = new Vector3(0.0f, 0.0f, -1.0f);
        animEndPos = new Vector3(0.0f, 0.0f, -1.0f);
    }

    public int GetPostion(int mapWidth)
    {
        return h * mapWidth + w;
    }

    public void MoveTo(int newh, int neww)
    {
        h = newh;
        w = neww;
    }

    public void FaceTo(FACING direction)
    {
        faceTo = direction;
        if (direction == FACING.RIGHT)
            SpriteObj.GetComponent<SpriteRenderer>().flipX = true;
        else if (direction == FACING.LEFT)
            SpriteObj.GetComponent<SpriteRenderer>().flipX = false;
    }
}

public class Monsters : MonoBehaviour {

    private List<Monster> monsterList = null;  // store obstacle position in as integer(h * width + w)
    private Monster boss = null;
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

    public void SpawnMonsters(int totalNum)
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
            if(emegercyJumpOut++ > (totalNum * 2048))
            {
                Debug.Log("Emegercy Jump-Out Happened.");
                Debug.Log("tryCount: " + emegercyJumpOut + "\ntotalNum: " + totalNum);
                break;
            }
            // make random pos
            int pos = prePos + Random.Range(posRandMin, posRandMax);
            // check map range
            if (pos > (levelMap.height - 1) * mapWidth) pos -= (levelMap.height - 2) * mapWidth;
            h = pos / mapWidth;
            w = pos % mapWidth;
            bool tooClose = false;
            // check if too close to player or finsh
            if (minDisBtwnMons > (System.Math.Abs(levelMap.playerStartTile[0] - h) + System.Math.Abs(levelMap.playerStartTile[1] - w))
             || 2 > (System.Math.Abs(levelMap.finishTile[0] - h) + System.Math.Abs(levelMap.finishTile[1] - w)))
                tooClose = true;
            // check if too close to other monster
            for (int i = 0; i < monsterList.Count && !tooClose; ++i)
            {
                if (minDisBtwnMons > ( System.Math.Abs(monsterList[i].h - h) + System.Math.Abs(monsterList[i].w - w)))
                    tooClose = true;
            }
            if (tooClose) prePos = pos;
            else if (levelMap.tiles[h, w] != (int)Level_Map.TILE_TYPE.WALL)
            {
                bool spawn_on_obs = levelMap.theObstacles.positionList.Exists(x => x == (h * mapWidth + w));
                // not too close and this is not wall/obstacle
                // check if is stuck
                int walkable_neighbor_count = 0;
                int h_tocheck = 0, w_tocheck = 0;
                for (int direction = 0; direction < 4; direction++)
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
                    if (levelMap.tiles[h_tocheck, w_tocheck] != (int)Level_Map.TILE_TYPE.WALL
                     && !levelMap.theObstacles.positionList.Exists(x => x == (h_tocheck * mapWidth + w_tocheck)))
                        walkable_neighbor_count++;
                }
                if (!spawn_on_obs && walkable_neighbor_count > 1)
                {
                    Spawn(h, w, spawnedCount);
                    spawnedCount++;
                }
                else if (spawn_on_obs && walkable_neighbor_count < 3 && walkable_neighbor_count > 1 && Random.Range(-1, 8) > 0)
                {
                    levelMap.theObstacles.ObsDestroy(pos);
                    Spawn(h, w, spawnedCount++);
                }
            }
        }
        Debug.Log("Monster Ganeration: " + monsterList.Count + "mons are spawned.");
    }

    public void SpawnBoss(int bossIndex)
    {
        Texture2D BossTex;
        Rect Rect;
        Sprite Sp;
        boss = new Monster(levelMap.height / 2, levelMap.width / 2, 0, GameObject.Find("Boss Sprites"));

        // give them thier ability
        switch (bossIndex)
        {
            case 1:
                boss.bossAbility = new Boss1Ability(boss, levelMap);
                break;
            default:
                break;
        }

        // load boss sprites
        BossTex = Resources.Load<Texture2D>("Bosses/boss_frame1_" + bossIndex.ToString());
        Rect = new Rect(0.0f, 0.0f, (float)BossTex.width, (float)BossTex.height);
        Sp = Sprite.Create(BossTex, Rect, new Vector2(0.5f, 0.5f));
        boss.bossAbility.sr_frame1.sprite = Sp;
        BossTex = Resources.Load<Texture2D>("Bosses/boss_frame2_" + bossIndex.ToString());
        Rect = new Rect(0.0f, 0.0f, (float)BossTex.width, (float)BossTex.height);
        Sp = Sprite.Create(BossTex, Rect, new Vector2(0.5f, 0.5f));
        boss.bossAbility.sr_frame2.sprite = Sp;
    }

    private void Spawn(int h, int w, int index)
    {
        //Debug.Log("monster spawn happened at " + h + "," + w);
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
        int found = monsterList.FindIndex(x => x.GetPostion(levelMap.width) == playerPos);
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
                if (monsterList[i].SpriteObj.GetComponent<SpriteRenderer>().sprite == sprite_frame1)
                    sp_to_change = sprite_frame2;
                else
                    sp_to_change = sprite_frame1;
            }
            if(monsterList[i].bossAbility != null)
            {
                // do boss' change frame here
            }
            else
                monsterList[i].SpriteObj.GetComponent<SpriteRenderer>().sprite = sp_to_change;
        }

        if (boss != null)
        {
            bool t = boss.bossAbility.sr_frame1.enabled;
            boss.bossAbility.sr_frame1.enabled = boss.bossAbility.sr_frame2.enabled;
            boss.bossAbility.sr_frame2.enabled = t;
        }
    }

    public void MonstersMove()
    {
        int monsterSensePlayer = 6; // == minDisBtwnMon
        for (int i = 0; i < monsterList.Count; i++)
        {
            // change position
            if (monsterSensePlayer >= (System.Math.Abs(levelMap.thePlayer.h - monsterList[i].h) + System.Math.Abs(levelMap.thePlayer.w - monsterList[i].w))
             && Random.Range(-1, 18) > 0)
                MonsterMoveToPlayer(i);
            else
                MonsterMoveRandom(i);
        }

        if (boss != null)
        {
            if (monsterSensePlayer * 2 >= (System.Math.Abs(levelMap.thePlayer.h - boss.h) + System.Math.Abs(levelMap.thePlayer.w - boss.w)))
            {
                //if(boss.bossAbility.TryDoAbility() == false)
                    MonsterMoveToPlayer(-1);
            }   
            else
                MonsterMoveRandom(-1);
        }
    }

    private void MonsterMoveToPlayer(int i)
    {
        int goingTo = -1;
        Monster thisMon = (i >= 0) ? monsterList[i] : boss;
        Astar m_astar;
        List<int> pathList;

        m_astar = new Astar(levelMap.tiles, levelMap.height, levelMap.width, levelMap.theObstacles.positionList, new int[2] { thisMon.h, thisMon.w}, new int[2] { levelMap.thePlayer.h, levelMap.thePlayer.w });
        m_astar.FindPathLength(false, true);
        pathList = m_astar.GetPath();
        if (pathList.Count > 1) goingTo = pathList[1];

        //for (int k = 0; k < pathList.Count; k++) Debug.Log("[" + k + "]" + ": " + pathList[k]);
        //Debug.Log("goingTo = " + goingTo);

        if (goingTo == -1 || pathList.Count > 16)
        { // Monster sense player but cannot find path //Debug.Log("try MonsterMoveToPlayer() failed");
            MonsterMoveRandom(i);
        }
        else
        {
            //Debug.Log("MonsterMoveToPlayer()");
            int newh = thisMon.h, neww = thisMon.w;
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
                    if (thisMon.h == newh && thisMon.w == neww) break;
                }
                if (j == monsterList.Count)
                {
                    //Debug.Log("Monster " + i + "moved from " + thisMon.h + "," + thisMon.w + " to " + newh + "," + neww);
                    thisMon.MoveTo(newh, neww);
                    thisMon.FaceTo((FACING) goingTo);
                    MonsterAnimSetup(i, thisMon.SpriteObj.transform.position, new Vector3((neww - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - newh - 0.5f), 0));
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
                    monsterList[i].FaceTo((FACING) goingTo);
                    MonsterAnimSetup(i, monsterList[i].SpriteObj.transform.position, new Vector3((neww - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - newh - 0.5f), 0));
                    break;
                }
            }
            tryCount++;
        } // end of while(trycount < 4)
    }

    /* MONSTER ANIM */

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
                if (x.animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
                    x.SpriteObj.transform.position = x.SpriteObj.transform.position + (x.animEndPos - x.animBeginPos) / (Time.deltaTime / 0.00135f);
            });
            if (monsterList[0].animEndPos != new Vector3(0.0f, 0.0f, -1.0f)
                && (monsterList[0].animEndPos - monsterList[0].SpriteObj.transform.position).normalized == (monsterList[0].animBeginPos - monsterList[0].animEndPos).normalized)
                return true;
        }
        return false;
    }

    public void MonstersAnimEnd()
    {
        monsterList.ForEach(delegate(Monster x) {
            if (x.animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
            {
                x.SpriteObj.transform.position = x.animEndPos;
                x.animEndPos = new Vector3(0.0f, 0.0f, -1.0f);
                x.animBeginPos = new Vector3(0.0f, 0.0f, -1.0f);
            }
        });
    }

    private void KillMonsterByIndex(int i)
    {
        Debug.Log("destroy monster #" + monsterList[i].id);
        Destroy(monsterList[i].SpriteObj, 0.15f);
        monsterList.RemoveAt(i);
    }

    public void TryKillMonsterByPos(int pos)
    {
        int found = monsterList.FindIndex(x => pos == x.GetPostion(levelMap.width));
        if (found >= 0)
            KillMonsterByIndex(found);
    }
    
    public void DestroyMonsters()
    {
        int k = 0;
        for (; k < monsterList.Count; k++)
        {
            Destroy(monsterList[k].SpriteObj);
        }
        monsterList.Clear();
        Debug.Log("destroy " + k + " monsters");
    }

    // Update is called once per frame
    void Update ()
    {
		
	}
}
