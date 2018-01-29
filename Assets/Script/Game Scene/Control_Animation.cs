using UnityEngine.UI;
using UnityEngine;
using TileTypeDefine;

public class Control_Animation : MonoBehaviour {

    public Level_Map levelMap;
    public float times_monster_change_sprite = 0;
    public float times_boss_hurted_sprite = 0;
    public float times_boss_ability_sprite = 0;
    public float time_view_map_mode = 0;

    public bool isViewMapMode = false;
    public bool viewMapModeAnimation = false;
    private Vector3 vamm_pos, vamm_scale;
    GameObject Game_Panel;

    private bool bossMonsterHurtedAnimation = false;
    private SpriteRenderer bossSpecialSprite = null;

    abstract public class Animation
    {
        protected Level_Map levelMap;
        public float times_flagged = 0;
        public bool isGoing = false;
        public static readonly float ANIM_DUR_TIME = 0.225f;

        public Animation()
        {
            levelMap = GameObject.Find("Game Panel").GetComponent<Level_Map>();
        }

        abstract public void Start();
        abstract public void Update();
        abstract public void End();
    }

    public class PlayerAnim : Animation
    {
        public Player_Display pd;
        private PlayerHurtedAnim playerHurtedAnim = new PlayerHurtedAnim();

        public PlayerAnim()
        {
            pd = levelMap.thePlayer.thePlayerDisp;
        }

        public override void Start()
        {
            times_flagged = Time.time + ANIM_DUR_TIME;
            isGoing = true;
        }

        public override void Update()
        {
            if ((pd.animEndPos - pd.ObjPos).magnitude < 0.01f || this.times_flagged <= Time.time)
            {
                if (!playerHurtedAnim.isGoing)
                {
                    pd.ObjPos = pd.animEndPos;
                    if (levelMap.thePlayer.IsPlayerAttacked())
                        playerHurtedAnim.Start();
                    else
                        End();
                }
                else
                {
                    playerHurtedAnim.Update();
                    // check if it end at this update
                    if (!playerHurtedAnim.isGoing)
                        End();
                }
            }
            else
                pd.ObjPos += (pd.animEndPos - pd.animBeginPos) / ANIM_DUR_TIME * Time.deltaTime * 0.995f;
        }

        public override void End()
        {
            isGoing = false;
            pd.animEndPos = new Vector3(0.0f, 0.0f, -1.0f);
            pd.animBeginPos = new Vector3(0.0f, 0.0f, -1.0f);
        }
    }

    private class PlayerHurtedAnim : Animation
    {
        public override void Start()
        {
            times_flagged = Time.time + ANIM_DUR_TIME;
            isGoing = true;
            GameObject.Find("Player Attacked Effect").GetComponent<SpriteRenderer>().enabled = true;
        }

        public override void Update()
        {
            if (times_flagged <= Time.time)
                End();
        }

        public override void End()
        {
            isGoing = false;
            GameObject.Find("Player Attacked Effect").GetComponent<SpriteRenderer>().enabled = false;
            if (levelMap.thePlayer.healthPoint <= 0)
                levelMap.thePlayer.theControlPanel.ToggleFailMenu();
        }
    }

    public class MonstersAnim : Animation
    {
        public override void Start()
        {
            isGoing = true;
            times_flagged = Time.time + ANIM_DUR_TIME;
        }

        public override void Update()
        {
            if (times_flagged <= Time.time)
            {
                End();
            }
            else if (levelMap.theMonsters.monsList.Count > 0)
            {
                bool can_end = false;
                foreach (Monster x in levelMap.theMonsters.monsList)
                {
                    if (x.animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
                    {
                        x.SpriteObj.transform.position += (x.animEndPos - x.animBeginPos) / ANIM_DUR_TIME * Time.deltaTime * 0.995f;
                        if (!can_end)
                        {
                            if ((x.animEndPos - x.SpriteObj.transform.position).magnitude < 0.01f)
                                can_end = true;
                        }
                    }
                }
                if (can_end)
                {
                    End();
                }
            }
        }

        public override void End()
        {
            isGoing = false;
            foreach (Monster x in levelMap.theMonsters.monsList)
            {
                if (x.animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
                {
                    x.SpriteObj.transform.position = x.animEndPos;
                    x.animEndPos = new Vector3(0.0f, 0.0f, -1.0f);
                    x.animBeginPos = new Vector3(0.0f, 0.0f, -1.0f);
                }
            }
            if (levelMap.thePlayer.energyPoint == 0) levelMap.thePlayer.theControlPanel.ToggleFailMenu();
        }
    }

    public class PlayerAbilityAnim : Animation
    {
        private int h = -1;
        private int w = -1;
        private SpriteRenderer thisObsSprtie;

        public override void Start()
        {
            isGoing = true;
            h = levelMap.thePlayer.h;
            w = levelMap.thePlayer.w;
            int dh = -1, dw = -1, pos = -1;
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
                else if (levelMap.tiles[h + dh, w + dw] == TILE_TYPE.WALKABLE)
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
            times_flagged = Time.time + ANIM_DUR_TIME / 16f;
        }

        public override void Update()
        {
            if (times_flagged <= Time.time)
            {
                int dh = -1, dw = -1, pos = -1;
                bool is_last = false;
                while (dh <= 1)
                {
                    if (levelMap.tiles[h + dh, w + dw] == TILE_TYPE.WALKABLE)
                    {
                        pos = (h + dh) * levelMap.width + (w + dw);
                        if (GameObject.Find("Obstacle Sprite" + pos.ToString()) == null)
                            continue;
                        else
                        {
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
                                if (thisObsSprtie.transform.localScale.y >= 1f)
                                    is_last = true;
                            }
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
                    End();
                else
                    times_flagged = Time.time + ANIM_DUR_TIME / 16f;
            }
        }

        public override void End()
        {
            levelMap.thePlayer.CheckPlayerBlocked();
            isGoing = false;
        }
    }

    public class BossAbilityAnim : Animation
    {
        private int h = -1, w = -1, dh = 0, dw = 0;
        private FACETO f = FACETO.DOWN;
        private SpriteRenderer bossSpecialSprite = null;
        private SpriteRenderer thisObsSprtie;

        public override void Start()
        {
            dh = dw = 0;
            int pos = -1;
            int lookat = -1; // side -1, middle 0, side 1
            h = levelMap.theMonsters.boss.h;
            w = levelMap.theMonsters.boss.w;
            f = levelMap.theMonsters.boss.faceTo;
            switch ((int)f)
            {
                case 0: // up
                    dh--; break;
                case 1: // left
                    dw--; break;
                case 2: // down
                    dh++; break;
                case 3: // right
                    dw++; break;
            }
            while (lookat <= 1)
            {
                if ((int)f % 2 == 0)
                    pos = (h + dh) * levelMap.width + (w + dw + lookat);
                else
                    pos = (h + dh + lookat) * levelMap.width + (w + dw);
                levelMap.theMonsters.KillMonsterByPos(pos);
                if (levelMap.theObstacles.positionList.Exists(x => x == pos))
                {
                    // to be Destroyed
                    thisObsSprtie = GameObject.Find("Obstacle Sprite" + pos.ToString()).GetComponent<SpriteRenderer>();
                    thisObsSprtie.transform.localScale = new Vector3(1f, 0.45f, 1f);
                    thisObsSprtie.transform.position -= new Vector3(0f, 0.27f, 0f);
                }
                else if (levelMap.tiles[h + dh, w + dw] == TILE_TYPE.WALKABLE)
                {
                    // Created
                    levelMap.theObstacles.ObsCreate(pos);
                    thisObsSprtie = GameObject.Find("Obstacle Sprite" + pos.ToString()).GetComponent<SpriteRenderer>();
                    thisObsSprtie.transform.localScale = new Vector3(1f, 0.55f, 1f);
                    thisObsSprtie.transform.position -= new Vector3(0f, 0.27f, 0f);
                }
                lookat++;
            }
            Debug.Log("BossAb: h:" + h + " w: " + w + " f: " + f);
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
            isGoing = true;
            times_flagged = Time.time + ANIM_DUR_TIME / 16f;
        }

        public override void Update()
        {
            if (times_flagged <= Time.time)
            {
                int pos = -1;
                int lookat = -1; // side -1, middle 0, side 1
                bool is_last = false;
                while (lookat <= 1)
                {
                    if ((int)f % 2 == 0)
                        pos = (h + dh) * levelMap.width + (w + dw + lookat);
                    else
                        pos = (h + dh + lookat) * levelMap.width + (w + dw);
                    if (GameObject.Find("Obstacle Sprite" + pos.ToString()) == null)
                        continue;
                    else
                    {
                        thisObsSprtie = GameObject.Find("Obstacle Sprite" + pos.ToString()).GetComponent<SpriteRenderer>();
                        if (thisObsSprtie.transform.localScale.y < 0.5f)
                        {
                            // to be Destroyed
                            thisObsSprtie.transform.localScale -= new Vector3(0f, 0.05f, 0f);
                            thisObsSprtie.transform.position -= new Vector3(0f, 0.03f, 0f);
                            if (thisObsSprtie.transform.localScale.y <= 0f)
                            {
                                Debug.Log("Boss ObsDestroy:" + pos);
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
                            if (thisObsSprtie.transform.localScale.y >= 1f)
                                is_last = true;
                        }
                    }
                    lookat++;
                }
                if (is_last)
                    End();
                else
                    times_flagged = Time.time + ANIM_DUR_TIME / 16f;
            }
        }

        public override void End()
        {
            Debug.Log("BossAb: End");
            levelMap.thePlayer.CheckPlayerBlocked();
            thisObsSprtie = null;
            if (bossSpecialSprite != null)
            {
                bossSpecialSprite.enabled = false;
                bossSpecialSprite = null;
            }
            isGoing = false;
        }
    }

    public PlayerAnim playerAnim;
    public MonstersAnim monstersAnim;
    public PlayerAbilityAnim playerAbilityAnim;
    public BossAbilityAnim bossAbilityAnim;

    // Use this for initialization
    void Start ()
    {
	}

    public void Initialize()
    {
        Input.multiTouchEnabled = true;
        levelMap = GameObject.Find("Game Panel").GetComponent<Level_Map>();
        Game_Panel = GameObject.Find("Game Panel");
        playerAnim = new PlayerAnim();
        monstersAnim = new MonstersAnim();
        playerAbilityAnim = new PlayerAbilityAnim();
        bossAbilityAnim = new BossAbilityAnim();
    }

    public void BossMonsterHurtedAnimStart()
    {
        bossMonsterHurtedAnimation = true;
        times_boss_hurted_sprite = Time.time + Animation.ANIM_DUR_TIME;
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
        time_view_map_mode = Time.time + Animation.ANIM_DUR_TIME / 12;
    }

    private void ViewMapModeAnim()
    {
        Game_Panel.transform.position = vamm_pos * 0.2f + Game_Panel.transform.position * 0.8f;
        Game_Panel.transform.localScale = vamm_scale * 0.2f + Game_Panel.transform.localScale * 0.8f;
        time_view_map_mode = Time.time + Animation.ANIM_DUR_TIME / 16;
        if (Mathf.Abs(Game_Panel.transform.position.magnitude - vamm_pos.magnitude) < 0.001f)
        {
            viewMapModeAnimation = false;
            Game_Panel.transform.localScale = vamm_scale;
            Game_Panel.transform.position = vamm_pos;
            if (vamm_scale != new Vector3(1, 1, 1))
            {
                isViewMapMode = true;
            }
        }
    }

    private void ViewMapModeMouseZoom(float y)
    {
        float ds = y * 0.025f;
        Vector3 n = Game_Panel.transform.localScale + new Vector3(ds, ds);
        if (n.x > 0.2f && n.x <= 1.2f)
            if (Mathf.Abs(Game_Panel.transform.position.x - vamm_pos.x) < levelMap.width / 2 * n.x && Mathf.Abs(Game_Panel.transform.position.y - vamm_pos.y) < levelMap.height / 2 * n.y)
                Game_Panel.transform.localScale = n;
    }

    private Vector3 preMousePos = new Vector3();

    private void ViewMapModeMouseDrag()
    {
        if (preMousePos != new Vector3())
        {
            Vector3 newPos = Game_Panel.transform.position + (Input.mousePosition - preMousePos) * 0.02f;
            if (Mathf.Abs(newPos.x - vamm_pos.x) < levelMap.width / 2 * Game_Panel.transform.localScale.x && Mathf.Abs(newPos.y - vamm_pos.y) < levelMap.height / 2 * Game_Panel.transform.localScale.y)
                Game_Panel.transform.position = newPos;
        }
        preMousePos = Input.mousePosition;
    }

    private void ViewMapModeTouchZoom()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);
        if (touch0.phase == TouchPhase.Moved && touch1.phase == TouchPhase.Moved)
        {
            // Dot > 0 means same move toward
            Vector2 dpos = touch1.position - touch0.position;
            float input0_move_direction =
                dpos.x * touch0.deltaPosition.x +
                dpos.y * touch0.deltaPosition.y;
            float input1_move_direction =
                -dpos.x * touch1.deltaPosition.x +
                -dpos.y * touch1.deltaPosition.y;
            float ds = 0;
            
            if (input0_move_direction > 0 && input1_move_direction > 0)
                ds = (touch0.deltaPosition + touch1.deltaPosition).magnitude * -0.002f;
            else if (input0_move_direction < 0 && input1_move_direction < 0)
                ds = (touch0.deltaPosition + touch1.deltaPosition).magnitude * 0.002f;
            Vector3 n = Game_Panel.transform.localScale + new Vector3(ds, ds);
            if (n.x > 0.2f && n.x <= 1.2f)
                if (Mathf.Abs(Game_Panel.transform.position.x - vamm_pos.x) < levelMap.width / 2 * n.x && Mathf.Abs(Game_Panel.transform.position.y - vamm_pos.y) < levelMap.height / 2 * n.y)
                    Game_Panel.transform.localScale = n;
        }
    }

    private void ViewMapModeTouchDrag()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector3 newPos = Game_Panel.transform.position + new Vector3(Input.GetTouch(0).deltaPosition.x * 0.01f, Input.GetTouch(0).deltaPosition.y * 0.01f);
            if (Mathf.Abs(newPos.x - vamm_pos.x) < levelMap.width / 2 * Game_Panel.transform.localScale.x && Mathf.Abs(newPos.y - vamm_pos.y) < levelMap.height / 2 * Game_Panel.transform.localScale.y)
                Game_Panel.transform.position = newPos;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerAnim.isGoing)
            playerAnim.Update();
        if (monstersAnim.isGoing)
            monstersAnim.Update();

        if (playerAbilityAnim.isGoing)
            playerAbilityAnim.Update();
        else if (bossAbilityAnim.isGoing)
            bossAbilityAnim.Update();

        if (bossMonsterHurtedAnimation && times_boss_hurted_sprite <= Time.time)
            BossMonsterHurtedAnimEnd();
        if (viewMapModeAnimation && time_view_map_mode <= Time.time)
            ViewMapModeAnim();

        if (times_monster_change_sprite <= Time.time)
        {
            if (times_monster_change_sprite == 0)
                times_monster_change_sprite = Time.time + 0.5f;
            else
                times_monster_change_sprite += 1.1f;
            levelMap.theMonsters.AllChangeFrame();
        }

        // can do drags to look around map in View-Map-Mode
        if (isViewMapMode)
        {
            if (Input.touchSupported)
            {
                if (Input.touchCount == 1)
                    ViewMapModeTouchDrag();
                else if (Input.touchCount == 2)
                    ViewMapModeTouchZoom();
            }
            else if (Input.mousePresent)
            {
                if (Input.GetMouseButton(1) || Input.GetMouseButton(0))
                    ViewMapModeMouseDrag();
                else if (Input.mouseScrollDelta.y != 0)
                    ViewMapModeMouseZoom(Input.mouseScrollDelta.y);

                // reset preMousePos
                if (!Input.GetMouseButton(1) && !Input.GetMouseButton(0))
                    preMousePos = new Vector3();
            }
        }

        // can't do control in View-Map-Mode
        // for playing on PC
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            levelMap.thePlayer.PlayerMoveUp();
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            levelMap.thePlayer.PlayerMoveLeft();
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            levelMap.thePlayer.PlayerMoveDown();
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            levelMap.thePlayer.PlayerMoveRight();
        }
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            levelMap.thePlayer.PlayerDoAbility();
        }
    }
}
