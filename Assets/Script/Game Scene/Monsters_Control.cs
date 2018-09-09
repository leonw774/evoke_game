using System.Collections.Generic;
using UnityEngine;

public enum FACETO {UP = 0, LEFT, DOWN, RIGHT};

public class Monster
{
    public int h;
    public int w;
    public int id; // -1 to identify the boss, >=0 to label normal monsters
    public FACETO faceTo;
    public GameObject SpriteObj;

    public BossAbility monAbility;

    public Vector3 animBeginPos;
    public Vector3 animEndPos;

    public Monster(int _h, int _w, int _id, GameObject _ms)
    {
        h = _h;
        w = _w;
        id = _id;
        SpriteObj = _ms;
        FaceTo = (FACETO)Random.Range(0, 4);
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

    public FACETO FaceTo
    {
        get
        {
            return faceTo;
        }
        set
        {
            SpriteRenderer sr = SpriteObj.GetComponent<SpriteRenderer>();
            faceTo = value;
            if (sr != null)
            {
                if (value == FACETO.RIGHT)
                    SpriteObj.GetComponent<SpriteRenderer>().flipX = true;
                else if (value == FACETO.LEFT)
                    SpriteObj.GetComponent<SpriteRenderer>().flipX = false;
            }
        }
    }
}

public class BossMonster : Monster
{
    public BossMonster(int _h, int _w, int _id, GameObject _ms, BossAbility _a) : base(_h, _w, _id, _ms)
    {
        monAbility = _a;
        FaceTo = (FACETO)Random.Range(0, 4);
        // need to set faceTo again
        // because the assignment in base will not do it
        // until ability is giving
    }

    public new FACETO FaceTo
    {
        get
        {
            return faceTo;
        }
        set
        {
            faceTo = value;
            if (monAbility.facingSprite != null)
                monAbility.facingSprite.enabled = false;
            switch (value)
            {
                case FACETO.UP:
                    monAbility.facingSprite = GameObject.Find("Back Boss Sprite").GetComponent<SpriteRenderer>();
                    break;
                case FACETO.LEFT:
                    monAbility.facingSprite = GameObject.Find("Left Boss Sprite").GetComponent<SpriteRenderer>();
                    break;
                case FACETO.DOWN:
                    monAbility.facingSprite = GameObject.Find("Front Boss Sprite").GetComponent<SpriteRenderer>();
                    break;
                case FACETO.RIGHT:
                    monAbility.facingSprite = GameObject.Find("Right Boss Sprite").GetComponent<SpriteRenderer>();
                    break;
                default:
                    return;
            }
            monAbility.facingSprite.enabled = true; 
        }
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

        const int MIN_DIS_BTW_MONS = 6;
        int spawnedCount = 0;
        int emegercyJumpOut = 0;
        int h = -1, w = -1, pos = -1;
        int mapWidth = levelMap.width;
        int prePos = levelMap.playerStartTile[0] * mapWidth + levelMap.playerStartTile[1];
        bool tooClose = false;

        /* ITERATIVE SPAWN */
        while (spawnedCount < totalNum)
        {
            if(emegercyJumpOut++ > (totalNum * 1024))
            {
                Debug.Log("Emegercy Jump-Out Happened.\ntryCount: " + emegercyJumpOut + "\ntotalNum: " + totalNum);
                break;
            }
            // make random pos
            pos = prePos + Random.Range(-MIN_DIS_BTW_MONS, MIN_DIS_BTW_MONS) + Random.Range(-MIN_DIS_BTW_MONS, MIN_DIS_BTW_MONS) * levelMap.width;
            // check map range
            if (pos > (levelMap.height - 1) * mapWidth)
                pos -= (levelMap.height - 2) * mapWidth;
            else if (pos < 0)
                pos += (levelMap.height - 1) * mapWidth;
            h = pos / mapWidth;
            w = pos % mapWidth;
            tooClose = false;
            // check if too close to player or finsh
            if (5 > (System.Math.Abs(levelMap.playerStartTile[0] - h) + System.Math.Abs(levelMap.playerStartTile[1] - w))
             || 3 > (System.Math.Abs(levelMap.finishTile[0] - h) + System.Math.Abs(levelMap.finishTile[1] - w)))
                tooClose = true;
            // check if too close to other monster
            foreach(Monster m in monsList)
            {
                if (MIN_DIS_BTW_MONS > (System.Math.Abs(m.h - h) + System.Math.Abs(m.w - w)))
                {
                    tooClose = true;
                    break;
                }
            }
            prePos = pos;
            if (!tooClose && levelMap.tiles[h, w] != TILE_TYPE.WALL)
            {
                bool spawn_on_obs = levelMap.theObstacles.positionList.Exists(x => x == (h * mapWidth + w));
                // not too close and this is not wall/obstacle
                // check if is stuck
                int chi_of_this_tile = 0;
                int h_tocheck = 0, w_tocheck = 0;
                for (int direction = 0; direction < 4; direction++)
                {
                    h_tocheck = h + ((direction % 2 == 0) ? (direction - 1) : 0);
                    w_tocheck = w + ((direction % 2 == 1) ? (direction - 2) : 0);
                    if (levelMap.IsTileWalkable(h_tocheck, w_tocheck))
                        chi_of_this_tile++;
                }
                if (chi_of_this_tile >= 1)
                {
                    if (!spawn_on_obs)
                    {
                        Spawn(h, w, spawnedCount);
                        spawnedCount++;
                    }
                }
                // else: spawn failed;
            }
        }
        Debug.Log("emegercyJumpOut: " + emegercyJumpOut);
        Debug.Log("MonsGen: " + monsList.Count + "mons spawned.");
    }

    public void SpawnBoss(int bossIndex)
    {
        //Texture2D BossTex;
        //Rect Rect;
        //Sprite Sp;

        // clear space for boss
        int pos = (levelMap.height / 2) * levelMap.width + levelMap.width / 2;
        levelMap.theObstacles.ObsDestroy(pos);

        // create thier object with right ability
        switch (bossIndex)
        {
            case 1:
                boss = new BossMonster(levelMap.height / 2, levelMap.width / 3 * 2, -1, GameObject.Find("Boss Sprites"), new Boss1_Ability(levelMap, (int) (levelMap.monsterNumber / 4) + 4));
                break;
            default:
                break;
        }
        boss.monAbility.SetSelf(boss);
        monsList.Add(boss);

        // load boss sprites
        // only do these when we have more than one boss which is not the case now
        /*
        BossTex = Resources.Load<Texture2D>("Bosses/boss_back_" + bossIndex.ToString());
        Rect = new Rect(0.0f, 0.0f, (float)BossTex.width, (float)BossTex.height);
        Sp = Sprite.Create(BossTex, Rect, new Vector2(0.5f, 0.5f));
        GameObject.Find("Back Boss Sprite") = Sp;
        .........
        */

        // make Boss Sprites appear
        Vector3 trans = levelMap.MapCoordToWorldVec3(boss.h, boss.w, 1);
        boss.SpriteObj.transform.localPosition = trans;
        boss.SpriteObj.transform.localScale = Vector3.one;
        Debug.Log("Boss Spawned");
    }

    private void Spawn(int h, int w, int index)
    {
        //Debug.Log("monster spawn happened at " + h + "," + w);
        Vector3 trans = levelMap.MapCoordToWorldVec3(h, w, 1);
        GameObject created = Instantiate(prototype);
        created.name = "Monster Sprite" + index.ToString();
        created.tag = "Monster";
        created.transform.localScale = new Vector3(0.875f, 0.875f, 1f);
        created.transform.parent = GameObject.Find("Game Panel").transform;
        created.transform.localPosition = trans + new Vector3(0.0f, 0.1f, 0.0f); //just a little adjust to y axis
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
            if (bossloss > 0 && boss.monAbility.decision == 1)
            {
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
        }
    }

    public void MonstersTurn()
    {
        int distanceToPlayer = 0;
        for (int i = 0; i < monsList.Count; i++)
        {
            distanceToPlayer = System.Math.Abs(levelMap.thePlayer.h - monsList[i].h) + System.Math.Abs(levelMap.thePlayer.w - monsList[i].w);
            if (distanceToPlayer <= ((monsList[i].id >= 0) ? 6 : 12)
                && (Random.Range(-1, 30) > 0 || monsList[i].id < 0))
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
                            if (distanceToPlayer > 2)
                                MonsterMoveToPlayer(i);
                            else
                                MonsterMoveRandom(i);
                            break;
                        case 3: // special move
                            boss.monAbility.DoSpecialMove();
                            break;
                        default:
                            // attack & ability behavier is handled in TryAttackPlayer
                            // that wont happen until every other monster are done moving
                            break;
                    }
                }
            }
            else
            {
                if (monsList[i].id < 0 || distanceToPlayer >= 32)
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
            int newh = thisMon.h + ((goingTo % 2 == 0) ? (goingTo - 1) : 0);
            int neww = thisMon.w + ((goingTo % 2 == 1) ? (goingTo - 2) : 0);
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
                    thisMon.FaceTo = (FACETO) goingTo;
                    thisMon.animBeginPos = thisMon.SpriteObj.transform.position;
                    thisMon.animEndPos = levelMap.MapCoordToWorldVec3(newh, neww, 1);
                }
            }
        }
    }

    private void MonsterMoveRandom(int i)
    {
        //Debug.Log("MonsterMoveRandom()");
        Monster thisMon = (monsList[i].id >= 0) ? monsList[i] : boss;

        if (thisMon.h == levelMap.thePlayer.h && thisMon.w == levelMap.thePlayer.w)
            return;

        int tryCount = 0, goingTo = -1;
        int newh = thisMon.h, neww = thisMon.w;
        while (tryCount++ <= 8)
        {
            goingTo = (Random.Range(0, 4) == 0 ? (int)thisMon.faceTo : Random.Range(0, 4));
            if (Random.Range(-1, 8) < 0) break; // monter wont move
            newh = thisMon.h + ((goingTo % 2 == 0) ? (goingTo - 1) : 0);
            neww = thisMon.w + ((goingTo % 2 == 1) ? (goingTo - 2) : 0);
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
                    thisMon.FaceTo = (FACETO) goingTo;
                    thisMon.animBeginPos = thisMon.SpriteObj.transform.position;
                    thisMon.animEndPos = levelMap.MapCoordToWorldVec3(newh, neww, 1);
                    break;
                }
            }
        } // end of while(trycount < 4)
    }

    public void KillMonsterByListIndex(int i)
    {
        //Debug.Log("destroy monster #" + monsList[i].id);

        if (monsList[i].id >= 0)
        {
            //GameObject.Find("Monster Hurt Sound").GetComponent<AudioSource>().PlayDelayed(0.11f);
            Vector3 v = monsList[i].SpriteObj.transform.localScale;
            v.y /= 2f;
            monsList[i].SpriteObj.transform.localScale = v;
            v = monsList[i].SpriteObj.transform.position;
            v.y -= 0.3f;
            monsList[i].SpriteObj.transform.position = v;
            Destroy(monsList[i].SpriteObj, 0.23f);
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
                    GameObject.Find("Monster Hurt Sound").GetComponent<AudioSource>().PlayDelayed(0.11f);
                    GameObject.Find("Closed Exit Sprite").GetComponent<SpriteRenderer>().enabled = false;
                    GameObject.Find("Exit Sprite").GetComponent<SpriteRenderer>().enabled = true;
                }
                levelMap.theAnimation.BossMonsterHurtedAnimStart();
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
