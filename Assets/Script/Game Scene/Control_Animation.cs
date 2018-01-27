using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TileTypeDefine;

public class Control_Animation : MonoBehaviour {

    private Player_Display thePlayerDisplay;
    private Level_Map levelMap;

    public float times_irreponsive = 0;
    public float times_monster_change_sprite = 0;
    public float times_boss_hurted_sprite = 0;
    public float times_boss_ability_sprite = 0;
    public float times_player_hurted_sprite = 0;
    public float time_obs_update = 0;
    public float time_view_map_mode = 0;

    public readonly float ANIM_DUR_TIME = 0.225f;
    public bool isViewMapMode = false;
    public bool viewMapModeAnimation = false;
    private Vector3 vamm_pos, vamm_scale;
    GameObject Game_Panel;

    private bool monsters_ask_for_end = false, player_ask_for_end = false;
    private bool moveAnimation = false;
    private bool playerHurtedAnimation = false;
    private bool bossMonsterHurtedAnimation = false;
    private bool bossMonsterAbilityAnimation = false;
    private bool obsUpdateAnimation = false;
    private SpriteRenderer bossSpecialSprite = null;

	// Use this for initialization
	void Start ()
    {
	}

    public void Initialize()
    {
        levelMap = GameObject.Find("Game Panel").GetComponent<Level_Map>();
        Game_Panel = GameObject.Find("Game Panel");
        thePlayerDisplay = levelMap.thePlayer.thePlayerDisp;
    }

    /* ANIMATION */

    /*
    * in player.move() -> setup begin & end pos (become not 0,0,0)
    * in monsters.move() -> sset up begin & end pos (become not 0,0,0)
    * 
    * animsetup()
    * 
    * Update call Anim() and find out that it have to work
    * 
    * In Anim(), do PlayerAnim & monsterAnim
    * when anim() find it should stop, call playeranimend() & monsteranimend()
    * 
    * playerAnimEnd() set begin & end back to 0,0,0
    * playerAnimEnd() check if player is attcked
    * monsterAnimEnd() set begin & end back to 0,0,0
    * */
    
    /* PLAYER ANIM */

    public void PlayerAnimSetup(Vector3 begin, Vector3 end)
    {
        //Debug.Log("PlayerAnimSetup: " + begin.ToString() + " -> " + end.ToString());
        thePlayerDisplay.animBeginPos = begin;
        thePlayerDisplay.animEndPos = end;
    }

    private bool PlayerAnim()
    {
        levelMap.thePlayer.thePlayerDisp.objPosition += (thePlayerDisplay.animEndPos - thePlayerDisplay.animBeginPos) / ANIM_DUR_TIME * Time.deltaTime * 0.9f;
        return (thePlayerDisplay.animEndPos - levelMap.thePlayer.thePlayerDisp.objPosition).magnitude < 0.01
            || (thePlayerDisplay.animEndPos - levelMap.thePlayer.thePlayerDisp.objPosition).normalized == (thePlayerDisplay.animBeginPos - thePlayerDisplay.animEndPos).normalized;
    }

    private void PlayerAnimEnd()
    {
        levelMap.thePlayer.thePlayerDisp.objPosition = thePlayerDisplay.animEndPos;
        thePlayerDisplay.animEndPos = new Vector3(0.0f, 0.0f, -1.0f);
        thePlayerDisplay.animBeginPos = new Vector3(0.0f, 0.0f, -1.0f);
        if (levelMap.thePlayer.IsPlayerAttacked())
        {
            PlayerHurtedAnimStart();
        }
    }

    private void PlayerHurtedAnimStart()
    {
        times_player_hurted_sprite = Time.time + ANIM_DUR_TIME;
        playerHurtedAnimation = true;
        GameObject.Find("Player Attacked Effect").GetComponent<SpriteRenderer>().enabled = true;

    }

    private void PlayerHurtedAnimEnd()
    {
        playerHurtedAnimation = false;
        GameObject.Find("Player Attacked Effect").GetComponent<SpriteRenderer>().enabled = false;

        if (levelMap.thePlayer.healthPoint <= 0)
            levelMap.thePlayer.theControlPanel.toggleFailMenu();
    }

    /* MONSTER ANIM */

    public void MonsterAnimSetup(int index, Vector3 begin, Vector3 end)
    {
        levelMap.theMonsters.monsList[index].animBeginPos = begin;
        levelMap.theMonsters.monsList[index].animEndPos = end;
    }

    private bool MonstersAnim()
    {
        if (levelMap.theMonsters.monsList.Count > 0)
        {
            foreach (Monster x in levelMap.theMonsters.monsList)
            {
                if (x.animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
                    x.SpriteObj.transform.position += (x.animEndPos - x.animBeginPos) / ANIM_DUR_TIME * Time.deltaTime * 0.9f;
            }
            Monster exampleMons = levelMap.theMonsters.monsList[0];
            if (exampleMons.animEndPos != new Vector3(0.0f, 0.0f, -1.0f)
                && (exampleMons.animEndPos - exampleMons.SpriteObj.transform.position).normalized == (exampleMons.animBeginPos - exampleMons.animEndPos).normalized)
                return true;
        }
        return false;
    }

    private void MonstersAnimEnd()
    {
        foreach (Monster x in levelMap.theMonsters.monsList)
        {
            if (x.animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
            {
                x.SpriteObj.transform.position = x.animEndPos;
                x.animEndPos = new Vector3(0.0f, 0.0f, -1.0f);
                x.animBeginPos = new Vector3(0.0f, 0.0f, -1.0f);
            }
        }
    }

    public void BossMonsterAbilityAnimStart()
    {
        bossMonsterAbilityAnimation = true;
        times_boss_ability_sprite = Time.time + ANIM_DUR_TIME;
        if (bossSpecialSprite == null)
        {
            switch (levelMap.theMonsters.boss.faceTo)
            {
                case FACETO.UP:
                    bossSpecialSprite = GameObject.Find("Back Boss Sprite Ability").GetComponent<SpriteRenderer>();
                    break;
                case FACETO.LEFT:
                    bossSpecialSprite = GameObject.Find("Left Boss Sprite Ability").GetComponent<SpriteRenderer>();
                    break;
                case FACETO.DOWN:
                    bossSpecialSprite = GameObject.Find("Front Boss Sprite Ability").GetComponent<SpriteRenderer>();
                    break;
                case FACETO.RIGHT:
                    bossSpecialSprite = GameObject.Find("Right Boss Sprite Ability").GetComponent<SpriteRenderer>();
                    break;
                default:
                    return;
            }
            bossSpecialSprite.enabled = true;
        }
    }

    private void BossMonsterAbilityAnimEnd()
    {
        if (bossSpecialSprite != null)
        {
            bossMonsterAbilityAnimation = false;
            bossSpecialSprite.enabled = false;
            bossSpecialSprite = null;
        }
    }

    public void BossMonsterHurtedAnimStart()
    {
        bossMonsterHurtedAnimation = true;
        times_boss_hurted_sprite = Time.time + ANIM_DUR_TIME;
        if (bossSpecialSprite == null)
        {
            switch (levelMap.theMonsters.boss.faceTo)
            {
                case FACETO.UP:
                    bossSpecialSprite = GameObject.Find("Back Boss Sprite Hurted").GetComponent<SpriteRenderer>();
                    break;
                case FACETO.LEFT:
                    bossSpecialSprite = GameObject.Find("Left Boss Sprite Hurted").GetComponent<SpriteRenderer>();
                    break;
                case FACETO.DOWN:
                    bossSpecialSprite = GameObject.Find("Front Boss Sprite Hurted").GetComponent<SpriteRenderer>();
                    break;
                case FACETO.RIGHT:
                    bossSpecialSprite = GameObject.Find("Right Boss Sprite Hurted").GetComponent<SpriteRenderer>();
                    break;
                default:
                    return;
            }
            bossSpecialSprite.enabled = true;
        }
        GameObject.Find("Boss Hurted Effect").GetComponent<SpriteRenderer>().enabled = true;
    }

    private void BossMonsterHurtedAnimEnd()
    {
        if (bossSpecialSprite != null)
        {
            bossMonsterHurtedAnimation = false;
            bossSpecialSprite.enabled = false;
            bossSpecialSprite = null;
        }
        GameObject.Find("Boss Hurted Effect").GetComponent<SpriteRenderer>().enabled = false;
        if (levelMap.theMonsters.boss != null)
        {
            if (levelMap.theMonsters.boss.monAbility.killed)
                levelMap.theMonsters.KillMonsterById(-1);
        }
    }

    /*
     * OBSTACLE ANIMS
     * */

    public void ObsUpdateAnimStart()
    {
        obsUpdateAnimation = true;
        int h = levelMap.thePlayer.h;
        int w = levelMap.thePlayer.w;
        int dh = -1, dw = -1, pos = -1;
        SpriteRenderer thisObsSprtie;
        while (dh <= 1)
        {
            pos = (h + dh) * levelMap.width + (w + dw);
            levelMap.theMonsters.KillMonsterByPos(pos);
            if (levelMap.theObstacles.positionList.Exists(x => x == pos))
            {
                // to be Destroyed
                thisObsSprtie = GameObject.Find("Obstacle Sprite" + pos.ToString()).GetComponent<SpriteRenderer>();
                thisObsSprtie.transform.localScale = new Vector3(1f, 0.45f, 1f);
                thisObsSprtie.transform.position -= new Vector3(0f, 0.27f, 0f);
            }
            else if (levelMap.tiles[h+ dh, w + dw] == TILE_TYPE.WALKABLE)
            {
                // Created
                levelMap.theObstacles.ObsCreate(pos);
                thisObsSprtie = GameObject.Find("Obstacle Sprite" + pos.ToString()).GetComponent<SpriteRenderer>();
                thisObsSprtie.transform.localScale = new Vector3(1f, 0.55f, 1f);
                thisObsSprtie.transform.position -= new Vector3(0f, 0.27f, 0f);
            }
            // upadte neighbor tiles ij
            if (dw == 1)
            {
                dh++;
                dw = -1;
            }
            else if (dh == 0 & dw == -1) dw = 1;
            else dw++;
        }
        time_obs_update = Time.time + ANIM_DUR_TIME / 9f;
    }

    private void ObsUpdateAnim()
    {
        int h = levelMap.thePlayer.h;
        int w = levelMap.thePlayer.w;
        int dh = -1, dw = -1, pos = -1;
        bool is_last = false;
        SpriteRenderer thisObsSprtie;
        while (dh <= 1)
        {
            if (levelMap.tiles[h + dh, w + dw] == TILE_TYPE.WALKABLE)
            {
                pos = (h + dh) * levelMap.width + (w + dw);
                thisObsSprtie = GameObject.Find("Obstacle Sprite" + pos.ToString()).GetComponent<SpriteRenderer>();
                if (thisObsSprtie.transform.localScale.y < 0.5f)
                {
                    // to be Destroyed
                    thisObsSprtie.transform.localScale -= new Vector3(0f, 0.05f, 0f);
                    thisObsSprtie.transform.position -= new Vector3(0f, 0.03f, 0f);
                    if (thisObsSprtie.transform.localScale.y <= 0f)
                    {
                        thisObsSprtie = null;
                        levelMap.theObstacles.ObsDestroy(pos);
                        is_last = true;
                    }
                }
                else
                {
                    // Created
                    thisObsSprtie.transform.localScale += new Vector3(0f, 0.05f, 0f);
                    thisObsSprtie.transform.position += new Vector3(0f, 0.03f, 0f);
                }
            }
            // upadte neighbor tiles ij
            if (dw == 1)
            {
                dh++;
                dw = -1;
            }
            else if (dh == 0 & dw == -1) dw = 1;
            else dw++;
        }
        if (is_last)
            obsUpdateAnimation = false;
    }

    /*
     * VIEW ALL MAP MODE
     * */
    public void ViewMapModeAnimStart()
    {
        float s = Mathf.Min(9f / levelMap.height, 11f / levelMap.width);
        float dh = (levelMap.height / 2 - levelMap.thePlayer.h);
        float dw = (levelMap.thePlayer.w - levelMap.width / 2);
        levelMap.thePlayer.thePlayerDisp.playerFacingSprite.enabled = isViewMapMode;
        GameObject.Find("Field Frontground Outring").GetComponent<SpriteRenderer>().enabled = isViewMapMode;
        GameObject.Find("Player Control Canvas").GetComponent<Canvas>().enabled = isViewMapMode;
        GameObject.Find("Ability Button Icon").GetComponent<SpriteRenderer>().enabled = isViewMapMode;
        if (isViewMapMode)
        {
            vamm_pos = new Vector3(0f, -0.1f);
            vamm_scale = new Vector3(1, 1, 1);
            GameObject.Find("Map Button Text").GetComponent<Text>().text = "VIEW WHOLE MAP";
            GameObject.Find("CD Output").GetComponent<Text>().text = "";
            isViewMapMode = false;
        }
        else
        {
            vamm_pos = new Vector3(dw + 1f, dh + 0.1f);
            vamm_scale = new Vector3(s, s, 1);
            GameObject.Find("Map Button Text").GetComponent<Text>().text ="RECENTER TO YOU";
            GameObject.Find("CD Output").GetComponent<Text>().text = "you can now dragl\nor room in & out\nto look around map";
        }
        viewMapModeAnimation = true;
        time_view_map_mode = Time.time + ANIM_DUR_TIME / 12;
    }

    private void ViewMapModeAnim()
    {
        Game_Panel.transform.position = vamm_pos * 0.2f + Game_Panel.transform.position * 0.8f;
        Game_Panel.transform.localScale = vamm_scale * 0.2f + Game_Panel.transform.localScale * 0.8f;
        time_view_map_mode = Time.time + ANIM_DUR_TIME / 16;
        if (Mathf.Abs(Game_Panel.transform.position.magnitude - vamm_pos.magnitude) < 0.001f)
        {
            viewMapModeAnimation = false;
            if (vamm_scale != new Vector3(1, 1, 1))
            {
                isViewMapMode = true;
            }
        }
    }

    private void ViewMapModeMouseZoom(Vector2 v)
    {
        float ds = v.y * 0.05f;
        Debug.Log("v.x: " + v.x);
        if (Game_Panel.transform.localScale.x + ds > 0.2 && Game_Panel.transform.localScale.x + ds <= 1f)
            Game_Panel.transform.localScale += new Vector3(ds, ds);
    }

    private void ViewMapModeTouchZoom()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            // Dot > 0 means same move toward
            float input0_direction =
                (Input.GetTouch(1).position - Input.GetTouch(0).position).x * Input.GetTouch(0).deltaPosition.x +
                (Input.GetTouch(1).position - Input.GetTouch(0).position).y * Input.GetTouch(0).deltaPosition.y;
            float input1_direction =
                (Input.GetTouch(0).position - Input.GetTouch(1).position).x * Input.GetTouch(1).deltaPosition.x +
                (Input.GetTouch(0).position - Input.GetTouch(1).position).y * Input.GetTouch(1).deltaPosition.y;
            float ds = input0_direction * input1_direction * 0.05f;
            if (Game_Panel.transform.localScale.x + ds > 0.2 && Game_Panel.transform.localScale.x + ds <= 1f)
            {
                if (input0_direction > 0 && input1_direction > 0)
                    Game_Panel.transform.localScale += new Vector3(ds, ds);
                else if (input0_direction < 0 && input1_direction < 0)
                    Game_Panel.transform.localScale -= new Vector3(ds, ds);
            }
        }
    }

    private Vector3 preMousePos = new Vector3();

    private void ViewMapModeMouseDrag()
    {
        if (preMousePos != new Vector3())
        {
            Vector3 newPos = Game_Panel.transform.position + (Input.mousePosition - preMousePos) * 0.025f;
            if (Mathf.Abs(newPos.x) < 20 && Mathf.Abs(newPos.y) < 20)
                Game_Panel.transform.position = newPos;
        }
        preMousePos = Input.mousePosition;
    }

    private void ViewMapModeTouchDrag()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector3 newPos = Game_Panel.transform.position + new Vector3(Input.GetTouch(0).deltaPosition.x * 0.025f, Input.GetTouch(0).deltaPosition.y * 0.025f); ;
            if (Mathf.Abs(newPos.x) < 20 && Mathf.Abs(newPos.y) < 20)
                Game_Panel.transform.position = newPos;
        }
    }

    /*
     * MAIN MOVE CONTROL ANIMATION
     * */
    
    public void AnimStart()
    {
        times_irreponsive = Time.time + ANIM_DUR_TIME;
        moveAnimation = true;
    }

    private void Anim()
    {
        if (times_irreponsive <= Time.time || player_ask_for_end || monsters_ask_for_end)
        {
            moveAnimation = false;
            monsters_ask_for_end = false;
            player_ask_for_end = false;

            // tidy up player pos
            // if player was doing ability
            if (thePlayerDisplay.animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
                PlayerAnimEnd();
            else
                levelMap.thePlayer.CheckPlayerBlocked();

            // tidy up monster pos
            MonstersAnimEnd();
        }
        else
        {
            // if player was doing ability
            if (thePlayerDisplay.animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
            {
                player_ask_for_end = PlayerAnim();
            }
            monsters_ask_for_end = MonstersAnim();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (times_monster_change_sprite <= Time.time)
        {
            if (times_monster_change_sprite == 0)
                times_monster_change_sprite = Time.time + 0.5f;
            else
                times_monster_change_sprite += 1.1f;
            levelMap.theMonsters.AllChangeFrame();
        }

        if (playerHurtedAnimation && times_player_hurted_sprite <= Time.time)
        {
            PlayerHurtedAnimEnd();
        }
        else if (bossMonsterHurtedAnimation && times_boss_hurted_sprite <= Time.time)
        {
            BossMonsterHurtedAnimEnd();
        }

        if (bossMonsterAbilityAnimation && times_boss_ability_sprite <= Time.time)
        {
            BossMonsterAbilityAnimEnd();
        }

        if (obsUpdateAnimation && time_obs_update <= Time.time)
        {
            ObsUpdateAnim();
        }

        if (viewMapModeAnimation && time_view_map_mode <= Time.time)
        {
            ViewMapModeAnim();
        }

        // can do drags to look around map in View-Map-Mode
        if (isViewMapMode)
        {
            if (Input.touchCount == 1)
                ViewMapModeTouchDrag();
            else if (Input.GetMouseButton(1) || Input.GetMouseButton(0))
                ViewMapModeMouseDrag();
            else if (!Input.GetMouseButton(1) && !Input.GetMouseButton(0))
                preMousePos = new Vector3();
            else if (Input.touchCount == 2)
                ViewMapModeTouchZoom();
            else if (Input.mouseScrollDelta.y != 0)
                ViewMapModeMouseZoom(Input.mouseScrollDelta);
        }

        // can't do control in View-Map-Mode
        // for playing on PC
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            levelMap.thePlayer.playerMoveUp();
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            levelMap.thePlayer.playerMoveLeft();
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            levelMap.thePlayer.playerMoveDown();
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            levelMap.thePlayer.playerMoveRight();
        }
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            levelMap.thePlayer.playerDoAbility();
        }

        if (moveAnimation)
        {
            Anim();
        }
    }
}
