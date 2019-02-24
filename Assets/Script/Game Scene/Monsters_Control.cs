using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FACETO {UP = 0, LEFT, DOWN, RIGHT};
public enum DECISION { IDLE = 0, MOVE, ABILITY, SPECIAL };

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

    public FACETO FaceTo
    {
        get
        {
            return faceTo;
        }
        set
        {
            faceTo = value;
            if (facingSprite != null)
            {
                if (value == FACETO.RIGHT)
                    facingSprite.flipX = true;
                else if (value == FACETO.LEFT)
                    facingSprite.flipX = false;
            }
        }
    }

    public Monster(int _h, int _w, int _id, GameObject _ms)
    {
        h = _h;
        w = _w;
        id = _id;
        SpriteObj = _ms;
        facingSprite = SpriteObj.GetComponent<SpriteRenderer>();
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

    public BossMonster(int _h, int _w, int _id, int _hp, GameObject _vessel, GameObject _state, GameObject _sprite, Level_Map _lm) : base(_h, _w, _id, _vessel)
    {
        decision = DECISION.IDLE;
        levelMap = _lm;

        FULL_HP = _hp;
        healthPoint = _hp;
        hpOutput = _state.GetComponent<Text>();
        hpOutput.text = _hp.ToString();
        hp = _hp;

        facingSprite = _sprite.GetComponent<SpriteRenderer>();
        // need to set faceTo again
        // because the assignment in base will not do it
        // until ability is giving
        FaceTo = (FACETO) Random.Range(0, 4);
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
                    facingSprite.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
                    facingSprite.flipY = true;
                    break;
                case FACETO.LEFT:
                    facingSprite.sprite = GameObject.Find("Left Boss Sprite").GetComponent<SpriteRenderer>().sprite;
                    facingSprite.flipY = false;
                    facingSprite.transform.rotation = new Quaternion(0f, 0f, 1f, -1f);
                    break;
                case FACETO.DOWN:
                    facingSprite.sprite = GameObject.Find("Front Boss Sprite").GetComponent<SpriteRenderer>().sprite;
                    facingSprite.flipY = false;
                    facingSprite.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
                    break;
                case FACETO.RIGHT:
                    facingSprite.sprite = GameObject.Find("Right Boss Sprite").GetComponent<SpriteRenderer>().sprite;
                    facingSprite.flipY = false;
                    facingSprite.transform.rotation = new Quaternion(0f, 0f, 1f, 1f);
                    break;
                default:
                    return;
            }
            facingSprite.enabled = true; 
        }
    }

    public DECISION Decide()
    {
        int distanceToPlayer = Mathf.Abs(levelMap.thePlayer.h - h) + Mathf.Abs(levelMap.thePlayer.w - w);
        if (distanceToPlayer > 24)
            return decision = DECISION.IDLE;
        if (CanDoAbility() || distanceToPlayer == 1)
            return decision = DECISION.ABILITY;
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
    /*
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
    */
}

public class Monsters_Control: MonoBehaviour {

    public List<Monster> monsList = null;  // store obstacle position in as integer(h * width + w)
    public List<BossMonster> bossList = null;

    /* public resource for general monsters */
    public Sprite sprite_frame1, sprite_frame2;
    public GameObject monsPrototype, bossPrototype, bossSpritePrototype, bossStatePrototype;

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
        bossPrototype = GameObject.Find("Prototype Boss Vessel");
        bossSpritePrototype = GameObject.Find("Prototype Boss Sprite");
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
        Debug.Log("map ask for " + totalNum + " mons");

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
            // check if too close to player or finish
            if (5 > (Mathf.Abs(levelMap.playerStartTile[0] - h) + Mathf.Abs(levelMap.playerStartTile[1] - w))
             || 3 > (Mathf.Abs(levelMap.finishTile[0] - h) + Mathf.Abs(levelMap.finishTile[1] - w)))
                tooClose = true;
            // check if too close to other monster
            foreach(Monster m in monsList)
            {
                if (MIN_DIS_BTW_MONS > (Mathf.Abs(m.h - h) + Mathf.Abs(m.w - w)))
                {
                    tooClose = true;
                    break;
                }
            }
            // check if too close to bosses
            foreach (BossMonster m in bossList)
            {
                if (MIN_DIS_BTW_MONS > (Mathf.Abs(m.h - h) + Mathf.Abs(m.w - w)))
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
        //Debug.Log("emegercyJumpOut: " + emegercyJumpOut);
        Debug.Log("MonsGen: " + monsList.Count + "mons spawned.");
    }

    private void SpawnAllBoss(int totalNum)
    {
        if (totalNum == 0)
            return;
        Debug.Log("map ask " + totalNum + " boss");
        const int MIN_DIS_BTW_MONS = 7;
        int spawnedCount = 0;
        int emegercyJumpOut = 0;
        bool invalid = false;
        
        while (spawnedCount < totalNum)
        {
            invalid = false;
            int h = Random.Range(1, levelMap.height - 1);
            int w = Random.Range(1, levelMap.width - 1);
            int pos = h * levelMap.width + w;
            
            if(emegercyJumpOut++ > (totalNum * 256))
            {
                Debug.Log("Emegercy Jump-Out Happened.\ntryCount: " + emegercyJumpOut + "\ntotalNum: " + totalNum);
                break;
            }

            int disTpPlayer = Mathf.Abs(levelMap.playerStartTile[0] - h) + Mathf.Abs(levelMap.playerStartTile[1] - w);
            int disToFinish = Mathf.Abs(levelMap.finishTile[0] - h) + Mathf.Abs(levelMap.finishTile[1] - w);
            // check if too close to player and boss have to spawn close to finsh
            invalid = disTpPlayer < 14 || (disToFinish < (2 + (spawnedCount * 5))) || (disToFinish > (8 + (spawnedCount * 5)));
            foreach (BossMonster m in bossList)
            {
                if (MIN_DIS_BTW_MONS > (Mathf.Abs(m.h - h) + Mathf.Abs(m.w - w)))
                {
                    invalid = true;
                    break;
                }
            }

            if (!invalid && levelMap.tiles[h, w] != TILE_TYPE.WALL)
            {
                // check chi
                int h_tocheck = 0, w_tocheck = 0;
                int chi_of_this_tile = 0;
                for (int direction = 0; direction < 4; direction++)
                {
                    h_tocheck = h + ((direction % 2 == 0) ? (direction - 1) : 0);
                    w_tocheck = w + ((direction % 2 == 1) ? (direction - 2) : 0);
                    if (levelMap.tiles[h_tocheck, w_tocheck] != TILE_TYPE.WALL)
                        chi_of_this_tile++;
                }
                if (chi_of_this_tile >= 1)
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
        }
        Debug.Log("BossGen: " + bossList.Count + "boss spawned.");
    }

    private void SpawnMonster(int h, int w, int id)
    {
        //Debug.Log("monster spawn at " + h + "," + w);
        Vector3 trans = levelMap.MapCoordToWorldVec3(h, w, 1);
        GameObject created = Instantiate(monsPrototype);
        created.name = "Monster Sprite" + id.ToString();
        created.tag = "Monster";
        created.transform.localScale = new Vector3(0.875f, 0.875f, 1f);
        created.transform.parent = GameObject.Find("Game Panel").transform;
        created.transform.localPosition = trans + new Vector3(0.0f, 0.1f, 0.0f); //just a little adjust to y axis
        monsList.Add(new Monster(h, w, id, created));
    }

    private void SpawnBoss(int h, int w, int id)
    {
        Debug.Log("boss spawn at " + h + "," + w);
        Vector3 trans = levelMap.MapCoordToWorldVec3(h, w, 1);

        GameObject vesselCreated = Instantiate(bossPrototype);
        vesselCreated.name = "Boss Vessel" + id.ToString();
        vesselCreated.tag = "Monster";
        vesselCreated.transform.SetParent(GameObject.Find("Game Panel").transform);
        vesselCreated.transform.localScale = new Vector3(0.875f, 0.875f, 1f);
        vesselCreated.transform.localPosition = trans + new Vector3(0.0f, 0.1f, 1.0f); //just a little adjust to y axis

        GameObject spriteCreated = Instantiate(bossSpritePrototype);
        spriteCreated.name = "Boss Sprite" + id.ToString();
        spriteCreated.tag = "Monster";
        spriteCreated.transform.SetParent(vesselCreated.transform);
        spriteCreated.transform.localScale = new Vector3(1f, 1f, 1f);
        spriteCreated.transform.localPosition = new Vector3(0.0f, 0.0f, 1.0f);

        GameObject stateCreated = Instantiate(bossStatePrototype);
        stateCreated.name = "Boss State" + id.ToString();
        stateCreated.tag = "Monster";
        stateCreated.transform.SetParent(vesselCreated.transform);
        stateCreated.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
        stateCreated.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        bossList.Add(new BossMonster(h, w, id, 3, vesselCreated, stateCreated, spriteCreated, levelMap));
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
        int distanceToPlayer = 0, p_h = levelMap.thePlayer.h, p_w = levelMap.thePlayer.w;
        foreach (Monster m in monsList)
        {
            distanceToPlayer = Mathf.Abs(p_h - m.h) + Mathf.Abs(p_w - m.w);
            if (distanceToPlayer <= 6 && Random.Range(0, 32) > 0)
            {
                MonsterMoveToPlayer(m, false);
            }
            else if (distanceToPlayer <= 32)
            {
                MonsterMoveRandom(m, false);
            }
        }
        foreach (BossMonster m in bossList)
        {
            if (m.hp > 0)
            {
                m.Decide();
                distanceToPlayer = Mathf.Abs(p_h - m.h) + Mathf.Abs(p_w - m.w);
                // Debug.Log("boss" + i + " decision: " + bossList[i].decision);
                if (m.decision == DECISION.MOVE)
                {
                    if (distanceToPlayer <= 2 || (distanceToPlayer == 3 && Random.Range(0, 32) == 0))
                        MonsterMoveRandom(m, true);
                    else
                        MonsterMoveToPlayer(m, true);
                }
            }
        }    
    }

    private void MonsterMoveToPlayer(Monster thisMon, bool isBoss)
    {
        if (Mathf.Abs(thisMon.h - levelMap.thePlayer.h) + Mathf.Abs(thisMon.w - levelMap.thePlayer.w) > 16)
            return;
        
        int goingTo = -1;
        List<int> pathList;
        Astar monAstar = new Astar(levelMap.tiles, levelMap.height, levelMap.width, levelMap.theObstacles.positionList,
                            new int[2] { thisMon.h, thisMon.w},
                            new int[2] { levelMap.thePlayer.h, levelMap.thePlayer.w });

        monAstar.FindPath(false, isBoss, true);
        pathList = monAstar.GetPath();
        goingTo = (pathList.Count == 0) ? -1 : pathList[0];
        //Debug.Log("mon #" + monsList[i].id + " goingTo = " + goingTo);
        //for (int k = 0; k < pathList.Count; k++) Debug.Log("[" + k + "]" + ": " + pathList[k]);

        if (goingTo == -1 || pathList.Count > (isBoss ? 24 : 12))
        { // Monster sense player but cannot find path //Debug.Log("try MonsterMoveToPlayer() failed");
            MonsterMoveRandom(thisMon, isBoss);
        }
        else
        {
            //Debug.Log("MonsterMoveToPlayer()");
            int newh = thisMon.h + ((goingTo % 2 == 0) ? (goingTo - 1) : 0);
            int neww = thisMon.w + ((goingTo % 2 == 1) ? (goingTo - 2) : 0);
            // check if a boss is too close to other boss: don't move
            if (isBoss)
            {
                foreach (BossMonster x in bossList)
                {
                    if (x.h != thisMon.h && x.w != thisMon.w)
                    {
                        if (Mathf.Abs(x.h - newh) + Mathf.Abs(x.w - neww) <= 3)
                        {
                            if (Random.Range(0, 2) > 0)
                                MonsterMoveRandom(thisMon, true);
                            else
                                return;
                                
                        }
                    }
                }
            }
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
                if (!blocked)
                {
                    foreach (BossMonster m in bossList)
                    {
                        if (m.h == newh && m.w == neww)
                        {
                            blocked = true;
                            break;
                        }
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

    private void MonsterMoveRandom(Monster thisMon, bool isBoss)
    {
        //Debug.Log("MonsterMoveRandom()");

        if (thisMon.h == levelMap.thePlayer.h && thisMon.w == levelMap.thePlayer.w)
            return;
        if (Mathf.Abs(thisMon.h - levelMap.thePlayer.h) + Mathf.Abs(thisMon.w - levelMap.thePlayer.w) > 16)
            return;

        int tryCount = 0, goingTo = -1;
        int newh = thisMon.h, neww = thisMon.w;
        while (tryCount++ <= 8)
        {
            goingTo = (Random.Range(0, 4) == 0 ? (int)thisMon.faceTo : Random.Range(0, 4));
            if (Random.Range(0, 8) < 0) break; // monter wont move
            newh = thisMon.h + ((goingTo % 2 == 0) ? (goingTo - 1) : 0);
            neww = thisMon.w + ((goingTo % 2 == 1) ? (goingTo - 2) : 0);
            // check if a boss is too close to other boss: don't move
            if (isBoss)
            {
                bool boss_too_crowded = false;
                foreach (BossMonster x in bossList)
                {
                    if (x.h != thisMon.h && x.w != thisMon.w)
                    {
                        if (Mathf.Abs(x.h - newh) + Mathf.Abs(x.w - neww) <= 3)
                        {
                            boss_too_crowded = Random.Range(0, 2) > 0;
                        }
                    }
                }
                if (boss_too_crowded) continue;
            }
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
                if (1 == (Mathf.Abs(levelMap.thePlayer.h - x.h) + Mathf.Abs(levelMap.thePlayer.w - x.w)))
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

        GameObject.Find("Monster Hurt Sound").GetComponent<AudioSource>().PlayDelayed(0.1f);
        Vector3 v = monsList[i].SpriteObj.transform.localScale;
        v.y /= 2f;
        monsList[i].SpriteObj.transform.localScale = v;
        v = monsList[i].SpriteObj.transform.position;
        v.y -= 0.3f;
        monsList[i].SpriteObj.transform.position = v;
        Destroy(monsList[i].SpriteObj, 0.2f);
        monsList.RemoveAt(i);
    }

    public void HurtBossByListIndex(int i)
    {
        if (bossList[i].hp - 1 == -1) // boss dead && hurt animation is over
        {
            Debug.Log("try destroy Boss Sprite" + bossList[i].id);
            Destroy(GameObject.Find("Boss Vessel" + bossList[i].id), 0.08f);
            Destroy(GameObject.Find("Boss Sprite" + bossList[i].id), 0.08f);
            Destroy(GameObject.Find("Boss State" + bossList[i].id));
            bossList.RemoveAt(i);
            /*GameObject.Find("Closed Exit Sprite").GetComponent<SpriteRenderer>().enabled = false;
            GameObject.Find("Exit Sprite").GetComponent<SpriteRenderer>().enabled = true;*/
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
