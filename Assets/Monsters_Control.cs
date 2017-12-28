using System.Collections.Generic;
using UnityEngine;
using TileTypeDefine;

public enum FACING : int {UP = 0, LEFT, DOWN, RIGHT};

public class Monster
{
    public FACING faceTo;
    public int h;
    public int w;
    public int id; // -1 to identify the boss, >=0 to label normal monsters
    public GameObject SpriteObj;

    public MonsterAbility monAbility;

    public Vector3 animBeginPos;
    public Vector3 animEndPos;

    public Monster(int _h, int _w, int _id, GameObject _ms)
    {
        h = _h;
        w = _w;
        id = _id;
        SpriteObj = _ms;
        if (id >= 0)
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

    virtual public void FaceTo(FACING direction)
    {
        faceTo = direction;
        if (direction == FACING.RIGHT)
            SpriteObj.GetComponent<SpriteRenderer>().flipX = true;
        else if (direction == FACING.LEFT)
            SpriteObj.GetComponent<SpriteRenderer>().flipX = false;
    }
}

public class BossMonster : Monster
{
    public BossMonster(int _h, int _w, int _id, GameObject _ms, MonsterAbility _a) : base(_h, _w, _id, _ms)
    {
        monAbility = _a;
        FaceTo((FACING)Random.Range(0, 4));
    }
}

public class Monsters_Control: MonoBehaviour {

    public List<Monster> monsList = null;  // store obstacle position in as integer(h * width + w)
    public BossMonster boss = null;

    /* public resource for general monsters */
    public Sprite sprite_frame1, sprite_frame2;
    public GameObject prototype;

    private Level_Map levelMap;

    // Use this for initialization
    void Start()
    {
        //Debug.Log("Monsters.Start()");
    }

    public void Initialize()
    {
        monsList = new List<Monster>();
        levelMap = gameObject.GetComponent<Level_Map>();
        prototype = GameObject.Find("Prototype Monster Sprite Frame 1");
        sprite_frame1 = prototype.GetComponent<SpriteRenderer>().sprite;
        sprite_frame2 = GameObject.Find("Prototype Monster Sprite Frame 2").GetComponent<SpriteRenderer>().sprite;
    }

    public void SpawnMonsters(int totalNum)
    {
        if (totalNum == 0)
            return;

        // boss
        // write hard in code is not good!
        // but we're running out of time
        if (Save_Data.SelectedLevel == Save_Data.BossLevel)
            SpawnBoss(1);

        int minDisBtwnMons = 6;
        int posRandMin = (levelMap.tiles.Length - levelMap.wallsNumber) / totalNum - minDisBtwnMons;
        int posRandMax = (levelMap.tiles.Length - levelMap.wallsNumber) / totalNum + (minDisBtwnMons * 2);
        int spawnedCount = 0;
        int emegercyJumpOut = 0;
        int h = -1, w = -1;
        int mapWidth = levelMap.width;
        int prePos = levelMap.playerStartTile[0] * mapWidth + levelMap.playerStartTile[1];
        //Debug.Log("posRandMin:" + posRandMin + "\nposRandMax: " + posRandMax);

        /* ITERATIVE SPAWN */
        while (spawnedCount < totalNum)
        {
            if(emegercyJumpOut++ > (totalNum * 4096))
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
            if (4 > (System.Math.Abs(levelMap.playerStartTile[0] - h) + System.Math.Abs(levelMap.playerStartTile[1] - w))
             || 3 > (System.Math.Abs(levelMap.finishTile[0] - h) + System.Math.Abs(levelMap.finishTile[1] - w)))
                tooClose = true;
            // check if too close to other monster
            for (int i = 0; i < monsList.Count && !tooClose; ++i)
            {
                if (minDisBtwnMons > ( System.Math.Abs(monsList[i].h - h) + System.Math.Abs(monsList[i].w - w)))
                    tooClose = true;
            }
            if (tooClose) prePos = pos;
            else if (levelMap.tiles[h, w] != TILE_TYPE.WALL)
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
                    if (levelMap.IsTileWalkable(h_tocheck, w_tocheck))
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
        Debug.Log("Monster Ganeration: " + monsList.Count + "mons are spawned.");
    }

    public void SpawnBoss(int bossIndex)
    {
        Texture2D BossTex;
        Rect Rect;
        Sprite Sp;

        // clear space for boss
        int pos = (levelMap.height / 2) * levelMap.width + levelMap.width / 2;
        levelMap.theObstacles.ObsDestroy(pos);

        // create thier object with right ability
        switch (bossIndex)
        {
            case 1:
                boss = new BossMonster(levelMap.height / 2, levelMap.width / 3 * 2, -1, GameObject.Find("Boss Sprite"), new Boss1_Ability(levelMap, (int) (levelMap.monsterNumber / 4) + 3));
                Debug.Log("Gave Boss its ability");
                break;
            default:
                break;
        }
        boss.monAbility.SetSelf(boss);
        monsList.Add(boss);

        // load boss sprites
        BossTex = Resources.Load<Texture2D>("Bosses/boss_frame1_test");
        Rect = new Rect(0.0f, 0.0f, (float)BossTex.width, (float)BossTex.height);
        Sp = Sprite.Create(BossTex, Rect, new Vector2(0.5f, 0.5f));
        boss.monAbility.sp_frame1 = Sp;
        BossTex = Resources.Load<Texture2D>("Bosses/boss_frame2_test");
        Rect = new Rect(0.0f, 0.0f, (float)BossTex.width, (float)BossTex.height);
        Sp = Sprite.Create(BossTex, Rect, new Vector2(0.5f, 0.5f));
        boss.monAbility.sp_frame2 = Sp;
        BossTex = Resources.Load<Texture2D>("Bosses/boss_frame3_test");
        Rect = new Rect(0.0f, 0.0f, (float)BossTex.width, (float)BossTex.height);
        Sp = Sprite.Create(BossTex, Rect, new Vector2(0.5f, 0.5f));
        boss.monAbility.sp_frame_hurt = Sp;

        // make Boss Sprites appear
        Vector3 trans = levelMap.MapCoordToWorldVec3(levelMap.height / 2, levelMap.width / 2, 1);
        boss.SpriteObj.transform.transform.position = trans;
    }

    private void Spawn(int h, int w, int index)
    {
        //Debug.Log("monster spawn happened at " + h + "," + w);
        Vector3 trans = levelMap.MapCoordToWorldVec3(h, w, 1);
        GameObject created = Instantiate(prototype);
        created.name = "Monster Sprite" + index.ToString();
        created.tag = "Monster";
        created.transform.parent = GameObject.Find("Game Panel").transform;
        created.transform.position = trans;
        monsList.Add(new Monster(h, w, index, created));
    }

    public int TryAttackPlayer(int playerPos)
    {
        // returns the hp the player to be loss
        int loss = 0;

        // normal monster's attack
        int found = monsList.FindIndex(x => x.GetPostion(levelMap.width) == playerPos);
        if (found >= 0)
        {
            KillMonsterByListIndex(found);
            loss++;
            //Debug.Log("because it attacked player in last round");
        }

        if (boss != null)
        {
            int bossloss = boss.monAbility.TryAttackPlayer();
            if (bossloss > 0)
            {
                boss.monAbility.DoAbility();
                loss += bossloss;
            }
        }
        return loss;
    }

    public void AllChangeFrame()
    {
        int sp_to_change = 0;
        foreach (Monster x in monsList)
        {
            if (x.id >= 0)
            {
                if (sp_to_change == 0)
                {
                    sp_to_change = ((x.SpriteObj.GetComponent<SpriteRenderer>().sprite == sprite_frame1) ? 2 : 1);
                }
                x.SpriteObj.GetComponent<SpriteRenderer>().sprite = ((sp_to_change == 1) ? sprite_frame1 : sprite_frame2);
            }
            else
            {
                if (x.SpriteObj.GetComponent<SpriteRenderer>().sprite == boss.monAbility.sp_frame1)
                {
                    sp_to_change = 2;
                }
                else if (x.SpriteObj.GetComponent<SpriteRenderer>().sprite == boss.monAbility.sp_frame2)
                {
                    sp_to_change = 1;
                }
                x.SpriteObj.GetComponent<SpriteRenderer>().sprite = ((sp_to_change == 1) ? boss.monAbility.sp_frame1 : boss.monAbility.sp_frame2);
            }
        }
    }

    public void MonstersTurn()
    {
        for (int i = 0; i < monsList.Count; i++)
        {
            if (((monsList[i].id >= 0) ? 6 : 12) >= (System.Math.Abs(levelMap.thePlayer.h - monsList[i].h) + System.Math.Abs(levelMap.thePlayer.w - monsList[i].w))
                && Random.Range(-1, 30) > 0)
            {
                if (monsList[i].id >= 0)
                {
                    MonsterMoveToPlayer(i);
                }
                else if (!boss.monAbility.killed)
                {
                    boss.monAbility.Decide();
                    //Debug.Log("boss decision: " + boss.monAbility.decision);
                    switch (boss.monAbility.decision)
                    {
                        case 0: // move
                            MonsterMoveToPlayer(i);
                            break;
                        case 1: // attack behavier wont happen until every other monster are done moving 
                            break;
                        case 2:
                            boss.monAbility.DoAbility();
                            break;
                        case 3:
                            boss.monAbility.DoSpecialMove();
                            MonsterMoveToPlayer(i);
                            break;
                        default:
                            Debug.Log("Boss can not decide!");
                            break;
                    }
                }
            }
            else
            {
                MonsterMoveRandom(i);
            }
        }
    }

    private void MonsterMoveToPlayer(int i)
    {
        int goingTo = -1;
        Monster thisMon = (monsList[i].id >= 0) ? monsList[i] : boss;
        Astar monAstar;
        List<int> pathList;

        monAstar = new Astar(levelMap.tiles, levelMap.height, levelMap.width, levelMap.theObstacles.positionList,
                            new int[2] { thisMon.h, thisMon.w},
                            new int[2] { levelMap.thePlayer.h, levelMap.thePlayer.w });

        monAstar.FindPathLength(false, (monsList[i].id < 0), true);
        pathList = monAstar.GetPath();
        if (pathList.Count > 0) goingTo = pathList[0];

        //Debug.Log("mon #" + monsList[i].id + " goingTo = " + goingTo);
        //for (int k = 0; k < pathList.Count; k++) Debug.Log("[" + k + "]" + ": " + pathList[k]);

        if (goingTo == -1 || pathList.Count > ((i >= 0) ? 12 : 24))
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
            if (levelMap.IsTileWalkable(newh, neww))
            {
                int j = 0;
                for (; j < monsList.Count; j++)
                { // there is another mon on the way
                    if (monsList[j].h == newh && monsList[j].w == neww) break;
                }
                if (j == monsList.Count)
                {
                    //Debug.Log("Monster " + i + "moved from " + thisMon.h + "," + thisMon.w + " to " + newh + "," + neww);
                    thisMon.MoveTo(newh, neww);
                    thisMon.FaceTo((FACING) goingTo);
                    levelMap.theAnimation.MonsterAnimSetup(i, thisMon.SpriteObj.transform.position, levelMap.MapCoordToWorldVec3(newh, neww, 1));
                }
            }
        }
    }

    private void MonsterMoveRandom(int i)
    {
        //Debug.Log("MonsterMoveRandom()");
        Monster thisMon = (monsList[i].id >= 0) ? monsList[i] : boss;

        int tryCount = 0;
        if (thisMon.h == levelMap.thePlayer.h && thisMon.w == levelMap.thePlayer.w)
            return;

        while (tryCount <= 8)
        {
            int goingTo = Random.Range(0, 4);
            int newh = thisMon.h, neww = thisMon.w;
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
            if (levelMap.tiles[newh, neww] != TILE_TYPE.WALL
                && !levelMap.theObstacles.positionList.Exists(x => x == (newh * levelMap.width + neww))
                && (levelMap.thePlayer.h != newh || levelMap.thePlayer.w != neww))
            {
                int j = 0;
                for (; j < monsList.Count; j++)
                { // there is another mon on the way
                    if (monsList[j].h == newh && monsList[j].w == neww) break;
                }
                if (j == monsList.Count)
                {
                    //Debug.Log("Monster " + i + "moved from " + monsterList[i].h + "," + monsterList[i].w + " to " + newh + "," + neww);
                    thisMon.MoveTo(newh, neww);
                    thisMon.FaceTo((FACING) goingTo);
                    levelMap.theAnimation.MonsterAnimSetup(i, thisMon.SpriteObj.transform.position, levelMap.MapCoordToWorldVec3(newh, neww, 1));
                    break;
                }
            }
            tryCount++;
        } // end of while(trycount < 4)
    }

    public void KillMonsterByListIndex(int i)
    {
        //Debug.Log("destroy monster #" + monsList[i].id);

        if (monsList[i].id >= 0)
        {
            GameObject.Find("Monster Hurt Sound").GetComponent<AudioSource>().Play();
            Vector3 v = monsList[i].SpriteObj.transform.localScale;
            v.y /= 3;
            monsList[i].SpriteObj.transform.localScale = v;
            v = monsList[i].SpriteObj.transform.position;
            v.y -= 0.3f;
            monsList[i].SpriteObj.transform.position = v;
            Destroy(monsList[i].SpriteObj, 0.2f);
            monsList.RemoveAt(i);
        }
        else
        {
            // boss is killed so make sprite disappear
            if (boss.monAbility.killed)
            {
                Debug.Log("boss is killed = true");
                boss.SpriteObj.transform.Translate(new Vector3(0, 0, -11));
                monsList.RemoveAt(i);
                boss = null;
            }
            else
            {
                boss.monAbility.hp = boss.monAbility.hp - 1;
                if (boss.monAbility.hp == 0) // boss dead
                {
                    boss.monAbility.killed = true;
                    GameObject.Find("Monster Hurt Sound").GetComponent<AudioSource>().Play();
                    GameObject.Find("Closed Exit Sprite").GetComponent<SpriteRenderer>().enabled = false;
                    GameObject.Find("Exit Sprite").GetComponent<SpriteRenderer>().enabled = true;
                }
                levelMap.theAnimation.BossMonsterHurtedAnimStart();
                boss.SpriteObj.GetComponent<SpriteRenderer>().sprite = boss.monAbility.sp_frame_hurt;
            }
        }
    }

    public void KillMonsterByPos(int pos)
    {
        int found = monsList.FindIndex(x => pos == x.GetPostion(levelMap.width));
        if (found >= 0)
            KillMonsterByListIndex(found);
    }

    public void KillMonsterById(int id)
    {
        int found = monsList.FindIndex(x => id == x.id);
        if (found >= 0)
            KillMonsterByListIndex(found);
    }
    
    public void DestroyAllMonsters()
    {
        monsList.ForEach(delegate (Monster x)
            {
                if (x.id >= 0)
                    Destroy(x.SpriteObj);
                else
                    boss.SpriteObj.transform.Translate(new Vector3(0, 0, -10));
            }
        );
        monsList.Clear();
        //Debug.Log("destroy " + k + " monsters");
    }

    // Update is called once per frame
    void Update ()
    {
		
	}
}
