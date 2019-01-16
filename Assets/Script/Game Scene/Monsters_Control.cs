using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FACETO {UP = 0, LEFT, DOWN, RIGHT};
public enum DECISION { NONE = 0, MOVE, ABILITY, SPECIAL };

public class Monster
{
    public int h;
    public int w;
    public int id; // -1 to identify the boss, >=0 to label normal monsters
    public FACETO faceTo;
    public GameObject SpriteObj;
    public SpriteRenderer facingSprite;

    public Vector3 animBeginPos;
    public Vector3 animEndPos;

    public Monster(int _h, int _w, int _id, GameObject _ms)
    {
        h = _h;
        w = _w;
        id = _id;
        SpriteObj = _ms;
        facingSprite = null;
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
    public DECISION decision;
    Level_Map levelMap;
    public readonly int FULL_HP;
    public Text hpOutput;
    public int healthPoint;
    public int hp
    {
        get
        {
            return healthPoint;
        }
        set
        {
            if (value < FULL_HP)
                if (hpOutput != null)
                    hpOutput.text = (healthPoint = value).ToString();
        }
    }

    public BossMonster(int _h, int _w, int _id, int _hp, GameObject _state, GameObject _sprite, Level_Map _lm) : base(_h, _w, _id, _sprite)
    {
        decision = DECISION.NONE;
        levelMap = _lm;
        hpOutput = _state.GetComponent<Text>();
        FULL_HP = _hp;
        healthPoint = _hp;
        hpOutput.text = healthPoint.ToString();
        hp = _hp;
        facingSprite = _sprite.GetComponent<SpriteRenderer>();
        // need to set faceTo again
        // because the assignment in base will not do it
        // until ability is giving
        FaceTo = (FACETO)Random.Range(0, 4);
        
    }

    public int GetPostion()
    {
        return h * levelMap.width + w;
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
            if (facingSprite != null)
                facingSprite.enabled = false;
            switch (value)
            {
                case FACETO.UP:
                    facingSprite.sprite = GameObject.Find("Back Boss Sprite").GetComponent<SpriteRenderer>().sprite;
                    break;
                case FACETO.LEFT:
                    facingSprite.sprite = GameObject.Find("Left Boss Sprite").GetComponent<SpriteRenderer>().sprite;
                    break;
                case FACETO.DOWN:
                    facingSprite.sprite = GameObject.Find("Front Boss Sprite").GetComponent<SpriteRenderer>().sprite;
                    break;
                case FACETO.RIGHT:
                    facingSprite.sprite = GameObject.Find("Right Boss Sprite").GetComponent<SpriteRenderer>().sprite;
                    break;
                default:
                    return;
            }
            facingSprite.enabled = true; 
        }
    }

    public DECISION Decide()
    {
        int distanceToPlayer = System.Math.Abs(levelMap.thePlayer.h - h) + System.Math.Abs(levelMap.thePlayer.w - w);

        // if player is next to it or
        // if player is near to it and they are seperated by some obstacles: ability
        if (CanDoAbility() || distanceToPlayer == 1)
            return decision = DECISION.ABILITY;

        // if player not near and health point is low: Special Move
        /*
        if (distanceToPlayer < 12 && distanceToPlayer >= 6 && Random.Range(-1, FULL_HP - healthPoint + 1) > 0)
            return decision = 3;
        */
        return decision = DECISION.MOVE;
    }

    public bool CanDoAbility()
    {
        Astar monAstar = new Astar(levelMap.tiles, levelMap.height, levelMap.width, levelMap.theObstacles.positionList,
                            new int[2] { h, w },
                            new int[2] { levelMap.thePlayer.h, levelMap.thePlayer.w });
        monAstar.FindPath(false, true, true);
        List<int> pathList = monAstar.GetPath();
        if (pathList.Count > 1)
            FaceTo = (FACETO)pathList[0];

        //monAstar.PrintPath();
        //Debug.Log("self.FaceTo = " + (int) self.faceTo);
        int h_tocheck = h + (((int)faceTo % 2 == 0) ? ((int)faceTo - 1) : 0);
        int w_tocheck = w + (((int)faceTo % 2 == 1) ? ((int)faceTo - 2) : 0);
        //Debug.Log("boss TryDoAbility(): boss at " + self.h + ", " + self.w + "; try at " + h_tocheck + ", " + w_tocheck);
        return levelMap.theObstacles.positionList.Exists(x => x == h_tocheck * levelMap.width + w_tocheck);
    }

    public void DoSpecialMove()
    {
        int goingTo = -1;
        Astar monAstar;
        List<int> pathList;

        monAstar = new Astar(levelMap.tiles, levelMap.height, levelMap.width, levelMap.theObstacles.positionList,
                            new int[2] { h, w },
                            new int[2] { levelMap.thePlayer.h, levelMap.thePlayer.w });
        monAstar.FindPath(false, true, true);
        pathList = monAstar.GetPath();
        if (pathList.Count > 0) goingTo = pathList[0];

        int newh = h, neww = w;
        if (goingTo == -1)
        {
            Debug.Log("boss DoSpecialMove() failed");
            return;
        }
        else if (pathList.Count > 3)
        {
            // move toward!
            switch (goingTo)
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
        }
        else
        {
            // move away!
            switch (goingTo)
            {
                case 0: // up
                    newh++; break;
                case 1: // left
                    neww++; break;
                case 2: // down
                    newh--; break;
                case 3: // right
                    neww--; break;
            }
        }

        if (levelMap.IsTileWalkable(newh, neww))
        {
            // no need to detect whether there is another mon on the way or not
            MoveTo(newh, neww);
            FaceTo = (FACETO)goingTo;
        }
    }
}

public class Monsters_Control: MonoBehaviour {

    public List<Monster> monsList = null;  // store obstacle position in as integer(h * width + w)
    public List<BossMonster> bossList = null;

    /* public resource for general monsters */
    public Sprite sprite_frame1, sprite_frame2;
    public GameObject monsPrototype, bossSpritePrototype, bossStatePrototype;

    private Level_Map levelMap;

    // Use this for initialization
    void Start()
    {
    }

    public void Initialize()
    {
        monsList = new List<Monster>();
        bossList = new List<BossMonster>();
        levelMap = gameObject.GetComponent<Level_Map>();
        bossSpritePrototype = GameObject.Find("Prototype Boss Sprite Vessel");
        bossStatePrototype = GameObject.Find("Prototype Boss HP Output");
        monsPrototype = GameObject.Find("Prototype Monster Sprite Frame 1");
        sprite_frame1 = monsPrototype.GetComponent<SpriteRenderer>().sprite;
        sprite_frame2 = GameObject.Find("Prototype Monster Sprite Frame 2").GetComponent<SpriteRenderer>().sprite;
    }

    public void SpawnAll(int monsNum, int bossNum)
    {
        SpawnAllBoss(bossNum);
        SpawnAllMonster(monsNum);
    }

    private void SpawnAllMonster(int totalNum)
    {
        if (totalNum == 0)
            return;

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
            // check if too close to bosses
            foreach (BossMonster m in bossList)
            {
                if (5 > (System.Math.Abs(m.h - h) + System.Math.Abs(m.w - w)))
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
                        SpawnMonster(h, w, spawnedCount);
                        spawnedCount++;
                    }
                }
                // else: spawn failed;
            }
        }
        Debug.Log("emegercyJumpOut: " + emegercyJumpOut);
        Debug.Log("MonsGen: " + monsList.Count + "mons spawned.");
    }

    private void SpawnAllBoss(int totalNum)
    {
        if (totalNum == 0)
            return;

        int spawnedCount = 0;
        bool tooClose = false;
        while (spawnedCount < totalNum)
        {
            tooClose = false;
            int h = Random.Range(1, levelMap.height - 1);
            int w = Random.Range(1, levelMap.width - 1);
            int pos = h * levelMap.width + w;

            // check if too close to player and boss have to spawn close to finsh
            if (12 > (System.Math.Abs(levelMap.playerStartTile[0] - h) + System.Math.Abs(levelMap.playerStartTile[1] - w))
             || (totalNum * 8) < (System.Math.Abs(levelMap.finishTile[0] - h) + System.Math.Abs(levelMap.finishTile[1] - w)))
                tooClose = true;
            // check if too close to other boss
            foreach (BossMonster m in bossList)
            {
                if (6 > (System.Math.Abs(m.h - h) + System.Math.Abs(m.w - w)))
                {
                    tooClose = true;
                    break;
                }
            }
            if (!tooClose && levelMap.tiles[h, w] != TILE_TYPE.WALL)
            {
                // clear space for boss
                levelMap.theObstacles.ObsDestroy(pos);
                levelMap.theObstacles.ObsDestroy(pos + 1);
                levelMap.theObstacles.ObsDestroy(pos - 1);
                levelMap.theObstacles.ObsDestroy(pos + levelMap.width);
                levelMap.theObstacles.ObsDestroy(pos - levelMap.width);
                SpawnBoss(h, w, spawnedCount);
                spawnedCount++;
            }
        }
        Debug.Log("BossGen: " + bossList.Count + "boss spawned.");
    }

    private void SpawnMonster(int h, int w, int index)
    {
        //Debug.Log("monster spawn at " + h + "," + w);
        Vector3 trans = levelMap.MapCoordToWorldVec3(h, w, 1);
        GameObject created = Instantiate(monsPrototype);
        created.name = "Monster Sprite" + index.ToString();
        created.tag = "Monster";
        created.transform.localScale = new Vector3(0.875f, 0.875f, 1f);
        created.transform.parent = GameObject.Find("Game Panel").transform;
        created.transform.localPosition = trans + new Vector3(0.0f, 0.1f, 0.0f); //just a little adjust to y axis
        monsList.Add(new Monster(h, w, index, created));
    }

    private void SpawnBoss(int h, int w, int index)
    {
        Debug.Log("boss spawn at " + h + "," + w);
        Vector3 trans = levelMap.MapCoordToWorldVec3(h, w, 1);
        GameObject spriteCreated = Instantiate(bossSpritePrototype);
        spriteCreated.name = "Boss Sprite" + index.ToString();
        spriteCreated.tag = "Monster";
        spriteCreated.transform.SetParent(GameObject.Find("Game Panel").transform);
        spriteCreated.transform.localScale = new Vector3(0.875f, 0.875f, 1f);
        spriteCreated.transform.localPosition = trans + new Vector3(0.0f, 0.1f, 1.0f); //just a little adjust to y axis

        GameObject stateCreated = Instantiate(bossStatePrototype);
        stateCreated.name = "Boss State" + index.ToString();
        stateCreated.tag = "Monster";
        stateCreated.transform.SetParent(spriteCreated.transform);
        stateCreated.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
        stateCreated.transform.localPosition = new Vector3(0.0f, -0.02f, 0.0f); //just a little adjust to y axis

        bossList.Add(new BossMonster(h, w, index, 4, stateCreated, spriteCreated, levelMap));
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
        //Debug.Log("AllChangeFrame" + sp_to_change);
    }

    public void MonstersTurn()
    {
        int distanceToPlayer = 0;
        for (int i = 0; i < monsList.Count; i++)
        {
            distanceToPlayer = System.Math.Abs(levelMap.thePlayer.h - monsList[i].h) + System.Math.Abs(levelMap.thePlayer.w - monsList[i].w);
            if (distanceToPlayer <= 6 && Random.Range(0, 32) > 0)
            {
                MonsterMoveToPlayer(monsList[i], false);
            }
            else if (distanceToPlayer >= 32)
            {
                MonsterMoveRandom(monsList[i]);
            }
        }
        for (int i = 0; i < bossList.Count; i++)
        {
            if (bossList[i].hp > 0)
            {
                bossList[i].Decide();
                distanceToPlayer = System.Math.Abs(levelMap.thePlayer.h - bossList[i].h) + System.Math.Abs(levelMap.thePlayer.w - bossList[i].w);
                //Debug.Log("boss" + i + " decision: " + bossList[i].decision);
                switch (bossList[i].decision)
                {
                    case DECISION.MOVE :
                        if (distanceToPlayer <= 2 || Random.Range(0, 16) > 0)
                            MonsterMoveRandom(bossList[i]);
                        else
                            MonsterMoveToPlayer(bossList[i], true);
                        break;
                    case DECISION.SPECIAL :
                        bossList[i].DoSpecialMove();
                        break;
                    default:
                        // attack & ability behavier is handled in TryAttackPlayer
                        // that wont happen until every other monster are done moving
                        break;
                }
            }
        }    
    }

    private void MonsterMoveToPlayer(Monster thisMon, bool isBoss)
    {
        int goingTo = -1;
        Astar monAstar;
        List<int> pathList;

        monAstar = new Astar(levelMap.tiles, levelMap.height, levelMap.width, levelMap.theObstacles.positionList,
                            new int[2] { thisMon.h, thisMon.w},
                            new int[2] { levelMap.thePlayer.h, levelMap.thePlayer.w });

        monAstar.FindPath(false, isBoss, true);
        pathList = monAstar.GetPath();
        if (pathList.Count > 0) goingTo = pathList[0];

        //Debug.Log("mon #" + monsList[i].id + " goingTo = " + goingTo);
        //for (int k = 0; k < pathList.Count; k++) Debug.Log("[" + k + "]" + ": " + pathList[k]);

        if (goingTo == -1 || pathList.Count > (isBoss ? 24 : 12))
        { // Monster sense player but cannot find path //Debug.Log("try MonsterMoveToPlayer() failed");
            MonsterMoveRandom(thisMon);
        }
        else
        {
            //Debug.Log("MonsterMoveToPlayer()");
            int newh = thisMon.h + ((goingTo % 2 == 0) ? (goingTo - 1) : 0);
            int neww = thisMon.w + ((goingTo % 2 == 1) ? (goingTo - 2) : 0);
            if (levelMap.IsTileWalkable(newh, neww))
            {
                bool blocked = false;
                // there is another mon on the way
                foreach (Monster m in monsList)
                {
                    if (m.h == newh && m.w == neww)
                    {
                        blocked = true;
                        break;
                    }
                }
                foreach (BossMonster m in bossList)
                {
                    if (m.h == newh && m.w == neww)
                    {
                        blocked = true;
                        break;
                    }
                }
                if (!blocked)
                {
                    //Debug.Log((isBoss ? "Boss " : "Monster ") + thisMon.id + " moved from " + thisMon.h + "," + thisMon.w + " to " + newh + "," + neww);
                    thisMon.MoveTo(newh, neww);
                    thisMon.FaceTo = (FACETO) goingTo;
                    thisMon.animBeginPos = thisMon.SpriteObj.transform.position;
                    thisMon.animEndPos = levelMap.MapCoordToWorldVec3(newh, neww, 1);
                }
            }
        }
    }

    private void MonsterMoveRandom(Monster thisMon)
    {
        //Debug.Log("MonsterMoveRandom()");

        if (thisMon.h == levelMap.thePlayer.h && thisMon.w == levelMap.thePlayer.w)
            return;

        int tryCount = 0, goingTo = -1;
        int newh = thisMon.h, neww = thisMon.w;
        while (tryCount++ <= 8)
        {
            goingTo = (Random.Range(0, 4) == 0 ? (int)thisMon.faceTo : Random.Range(0, 4));
            if (Random.Range(0, 8) < 0) break; // monter wont move
            newh = thisMon.h + ((goingTo % 2 == 0) ? (goingTo - 1) : 0);
            neww = thisMon.w + ((goingTo % 2 == 1) ? (goingTo - 2) : 0);
            if (levelMap.tiles[newh, neww] != TILE_TYPE.WALL
                && !levelMap.theObstacles.positionList.Exists(x => x == (newh * levelMap.width + neww))
                && (levelMap.thePlayer.h != newh || levelMap.thePlayer.w != neww))
            {
                bool blocked = false;
                // there is another mon on the way
                foreach (Monster m in monsList)
                {
                    if (m.h == newh && m.w == neww)
                    {
                        blocked = true;
                        break;
                    }
                }
                foreach (BossMonster m in bossList)
                {
                    if (m.h == newh && m.w == neww)
                    {
                        blocked = true;
                        break;
                    }
                }
                if (!blocked)
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

    public int TryAttackPlayer(int playerPos)
    {
        // returns the hp the player to be loss
        int loss = 0;

        // normal monster's attack
        int found = monsList.FindIndex(x => x.GetPostion(levelMap.width) == playerPos);
        if (found >= 0)
        {
            HurtMonsterByListIndex(found);
            loss++;
        }
        foreach(BossMonster x in bossList)
        {
            if (x.decision == DECISION.ABILITY)
            {
                if (1 == (System.Math.Abs(levelMap.thePlayer.h - x.h) + System.Math.Abs(levelMap.thePlayer.w - x.w)))
                {
                    if (levelMap.thePlayer.h < x.h)
                        x.FaceTo = FACETO.UP;
                    else if (levelMap.thePlayer.h == x.h)
                    {
                        if (levelMap.thePlayer.w > x.w)
                            x.FaceTo = FACETO.RIGHT;
                        else
                            x.FaceTo = FACETO.LEFT;
                    }
                    else
                        x.FaceTo = FACETO.DOWN;
                    loss++;
                }
            }
        }
        return loss;
    }

    public void HurtMonsterByListIndex(int i)
    {
        //Debug.Log("destroy monster #" + monsList[i].id);

        //GameObject.Find("Monster Hurt Sound").GetComponent<AudioSource>().PlayDelayed(0.11f);
        Vector3 v = monsList[i].SpriteObj.transform.localScale;
        v.y /= 2f;
        monsList[i].SpriteObj.transform.localScale = v;
        v = monsList[i].SpriteObj.transform.position;
        v.y -= 0.3f;
        monsList[i].SpriteObj.transform.position = v;
        Destroy(monsList[i].SpriteObj, 0.15f);
        monsList.RemoveAt(i);
    }

    public void HurtBossByListIndex(int i)
    {
        if (bossList[i].hp - 1 == -1) // boss dead && hurt animation is over
        {
            //GameObject.Find("Monster Hurt Sound").GetComponent<AudioSource>().PlayDelayed(0.11f);
            Debug.Log("try destroy Boss Sprite" + bossList[i].id);
            Destroy(GameObject.Find("Boss Sprite" + bossList[i].id), 0.05f);
            bossList.RemoveAt(i);
            GameObject.Find("Closed Exit Sprite").GetComponent<SpriteRenderer>().enabled = false;
            GameObject.Find("Exit Sprite").GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            bossList[i].hp = bossList[i].hp - 1;
            StartCoroutine(levelMap.theAnimation.BossMonsterHurtedAnim(i));
        }
    }

    public void HurtMonsterByPos(int pos)
    {
        int found = monsList.FindIndex(x => pos == x.GetPostion(levelMap.width));
        if (found >= 0)
            HurtMonsterByListIndex(found);
    }

    public void HurtMonsterById(int id)
    {
        int found = monsList.FindIndex(x => id == x.id);
        if (found >= 0)
            HurtMonsterByListIndex(found);
    }

    public void HurtBossByPos(int pos)
    {
        int found = bossList.FindIndex(x => pos == x.GetPostion(levelMap.width));
        if (found >= 0)
            HurtBossByListIndex(found);
    }

    public void HurtBossById(int id)
    {
        int found = bossList.FindIndex(x => id == x.id);
        if (found >= 0)
            HurtBossByListIndex(found);
    }

    public void DestroyAllMonsters()
    {
        monsList.ForEach(delegate (Monster x)
            {
                if (x.id >= 0)
                    Destroy(x.SpriteObj);
            }
        );
        monsList.Clear();
        bossList.ForEach(delegate (BossMonster x)
        {
            if (x.id >= 0)
                Destroy(x.SpriteObj);
        }
        );
        bossList.Clear();
        //Debug.Log("destroy " + k + " monsters");
    }

    // Update is called once per frame
    void Update ()
    {
		
	}
}
