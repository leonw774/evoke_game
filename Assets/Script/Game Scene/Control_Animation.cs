using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Control_Animation : MonoBehaviour {

    public Level_Map levelMap;
    public float times_monster_change_sprite = 0;
    public float time_view_map_mode = 0;

    public static readonly float ANIM_DUR_TIME = 0.2f;
    public static readonly float ANIM_PADDING_TIME = 0.025f;

    public bool is_irresponsive = false;
    public bool is_anim = false;
    public bool is_bossability = false;
    public bool is_vmm = false;
    public bool is_double_tap = false;
    private Vector3 vamm_pos, vamm_scale;
    GameObject Game_Panel;

    public IEnumerator PlayerMoveAnim()
    {
        float i = 0;
        Player_Display pd = levelMap.thePlayer.thePlayerDisp;

        while (is_anim)
            yield return 0;

        // update
        while (i <= ANIM_DUR_TIME && (pd.animEndPos - pd.ObjPos).magnitude >= 0.01f)
        {
            pd.ObjPos += (pd.animEndPos - pd.animBeginPos) * (Time.deltaTime * 0.975f / ANIM_DUR_TIME);
            i += Time.deltaTime;
            is_anim = true;
            yield return 0;
        }
        for (i = 0; i < ANIM_PADDING_TIME; i += Time.deltaTime)
            yield return 0;

        // end
        pd.ObjPos = pd.animEndPos;
        pd.animEndPos = new Vector3(0.0f, 0.0f, -1.0f);
        pd.animBeginPos = new Vector3(0.0f, 0.0f, -1.0f);
        is_anim = false;
    }

    public IEnumerator PlayerHurtedAnim()
    {
        GameObject PlayerHurtedObj = GameObject.Find("Player Attacked Effect");
        PlayerHurtedObj.GetComponent<SpriteRenderer>().enabled = true;
        PlayerHurtedObj.GetComponent<AudioSource>().Play();

        float i = 0;
        while (i < ANIM_DUR_TIME)
        {
            i += Time.deltaTime;
            is_anim = true;
            yield return 0;
        }
        for (i = 0; i < ANIM_PADDING_TIME; i += Time.deltaTime)
            yield return 0;
        PlayerHurtedObj.GetComponent<SpriteRenderer>().enabled = false;
        if (levelMap.thePlayer.HP <= 0)
            levelMap.thePlayer.theControlPanel.ToggleFailMenu();
        is_anim = false;
    }

    public IEnumerator MonstersMoveAnim()
    {
        float i = 0;
        bool can_end = false;
        while (i <= ANIM_DUR_TIME && !can_end)
        {
            if (levelMap.theMonsters.monsList.Count > 0)
            {
                foreach (Monster x in levelMap.theMonsters.monsList)
                {
                    if (x.animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
                    {
                        x.SpriteObj.transform.position += (x.animEndPos - x.animBeginPos) * (Time.deltaTime * 0.975f / ANIM_DUR_TIME);
                        can_end = ((x.animEndPos - x.SpriteObj.transform.position).magnitude < 0.01f);
                    }
                }
                foreach (BossMonster x in levelMap.theMonsters.bossList)
                {
                    if (x.decision == DECISION.MOVE)
                    {
                        if (x.animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
                        {
                            x.SpriteObj.transform.position += (x.animEndPos - x.animBeginPos) * (Time.deltaTime * 0.975f / ANIM_DUR_TIME);
                        }
                    }
                }
            }
            i += Time.deltaTime;
            is_anim = true;
            yield return 0;
        }
        for (i = 0; i < ANIM_PADDING_TIME; i += Time.deltaTime)
            yield return 0;

        // end
        foreach (Monster x in levelMap.theMonsters.monsList)
        {
            if (x.animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
            {
                x.SpriteObj.transform.position = x.animEndPos;
                x.animEndPos = new Vector3(0.0f, 0.0f, -1.0f);
                x.animBeginPos = new Vector3(0.0f, 0.0f, -1.0f);
            }
        }
        foreach (BossMonster x in levelMap.theMonsters.bossList)
        {
            if (x.decision == DECISION.MOVE)
            {
                if (x.animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
                {
                    x.SpriteObj.transform.position = x.animEndPos;
                    x.animEndPos = new Vector3(0.0f, 0.0f, -1.0f);
                    x.animBeginPos = new Vector3(0.0f, 0.0f, -1.0f);
                }
            }
        }
        // don't set is_anim to false when BossAbilityAnim and PlayerHurtedAnim may play
        if (levelMap.theMonsters.bossList.Count != 0)
        {
            for (i = 0; i < 0.01; i += Time.deltaTime)
                yield return 0;
            StartCoroutine(BossAbilityAnim()); 
        }
        else 
        {
            if (levelMap.thePlayer.IsPlayerAttacked())
            {
                StartCoroutine(PlayerHurtedAnim());
            }
            else
            {
                levelMap.thePlayer.CheckPlayerBlocked();
                is_anim = false;
            }

        }
        if (levelMap.thePlayer.EP == 0)
            levelMap.thePlayer.theControlPanel.ToggleFailMenu();
    }

    public IEnumerator PlayerAbilityAnim()
    {
        float i = 0;
        bool can_end = false;

        int h = levelMap.thePlayer.h;
        int w = levelMap.thePlayer.w;
        int dh = -1, dw = -1, pos = -1;

        List<GameObject> neighborObsObj = new List<GameObject>();

        // mark obs as to be destroyed or created
        while (dh <= 1)
        {
            pos = (h + dh) * levelMap.width + (w + dw);
            levelMap.theMonsters.HurtMonsterByPos(pos);
            levelMap.theMonsters.HurtBossByPos(pos);
            if (levelMap.theObstacles.positionList.Exists(x => x == pos))
            {
                // to be Destroyed: scale < 0.5
                levelMap.theObstacles.positionList.Remove(pos);
                GameObject thisObsObj = GameObject.Find("Obstacle Sprite" + pos.ToString());
                SpriteRenderer thisObsSprite = thisObsObj.GetComponent<SpriteRenderer>();
                thisObsSprite.transform.localScale = new Vector3(1f, 0.45f, 1f);
                thisObsSprite.transform.position -= new Vector3(0f, 0.27f, 0f);
                neighborObsObj.Add(thisObsObj);
            }
            else if (levelMap.tiles[h + dh, w + dw] == TILE_TYPE.WALKABLE)
            {
                // Created: scale > 0.5
                levelMap.theObstacles.ObsCreate(pos);
                GameObject thisObsObj = GameObject.Find("Obstacle Sprite" + pos.ToString());
                SpriteRenderer thisObsSprite = thisObsObj.GetComponent<SpriteRenderer>();
                thisObsSprite.transform.localScale = new Vector3(1f, 0.55f, 1f);
                thisObsSprite.transform.position -= new Vector3(0f, 0.27f, 0f);
                neighborObsObj.Add(thisObsObj);
            }
            // to next neighbor tile
            if (dw == 1)
            {
                dh++; dw = -1;
            }
            else if (dh == 0 & dw == -1) dw = 1;
            else dw++;
        }

        // update
        while (i <= ANIM_DUR_TIME && !can_end)
        {
            foreach (GameObject x in neighborObsObj)
            {
                if (x != null)
                {
                    SpriteRenderer thisObsSprite = x.GetComponent<SpriteRenderer>();
                    if (thisObsSprite.transform.localScale.y < 0.5f)
                    // to be Destroyed
                    {
                        if (thisObsSprite.transform.localScale.y > 0.001f)
                        {
                            thisObsSprite.transform.localScale -= new Vector3(0f, 0.45f * (Time.deltaTime / ANIM_DUR_TIME * 0.99f), 0f);
                            thisObsSprite.transform.position -= new Vector3(0f, 0.27f * (Time.deltaTime / ANIM_DUR_TIME * 0.99f), 0f);
                        }
                    }
                    else
                    // Created
                    {
                        if (!(can_end = x.transform.localScale.y > 0.999f))
                        {
                            thisObsSprite.transform.localScale += new Vector3(0f, 0.45f * (Time.deltaTime / ANIM_DUR_TIME * 0.99f), 0f);
                            thisObsSprite.transform.position += new Vector3(0f, 0.27f * (Time.deltaTime / ANIM_DUR_TIME * 0.99f), 0f);
                        }
                    }
                }
            }
            i += Time.deltaTime;
            is_anim = true;
            yield return 0;
        }
        for (i = 0; i < ANIM_PADDING_TIME; i += Time.deltaTime)
            yield return 0;

        // end
        foreach (GameObject x in neighborObsObj)
        {
            if (x != null)
            {
                SpriteRenderer thisObsSprite = x.GetComponent<SpriteRenderer>();
                if (thisObsSprite.transform.localScale.y < 0.54f) // Destroy
                {
                    thisObsSprite = null;
                    Destroy(x);
                }
                else // Created
                {
                    thisObsSprite.transform.localScale = new Vector3(1f, 1f, 1f);
                }
            }
        }
        is_anim = false;
    }

    public IEnumerator BossMonsterHurtedAnim(int bossIndex)
    {
        float i = 0;
        BossMonster thisBoss = levelMap.theMonsters.bossList[bossIndex];
        switch (thisBoss.faceTo)
        {
            case FACETO.UP:
                thisBoss.facingSprite.sprite = GameObject.Find("Back Boss Sprite Hurted").GetComponent<SpriteRenderer>().sprite;
                break;
            case FACETO.LEFT:
                thisBoss.facingSprite.sprite = GameObject.Find("Left Boss Sprite Hurted").GetComponent<SpriteRenderer>().sprite;
                break;
            case FACETO.DOWN:
                thisBoss.facingSprite.sprite = GameObject.Find("Front Boss Sprite Hurted").GetComponent<SpriteRenderer>().sprite;
                break;
            case FACETO.RIGHT:
                thisBoss.facingSprite.sprite = GameObject.Find("Right Boss Sprite Hurted").GetComponent<SpriteRenderer>().sprite;
                break;
            default:
                break;
        }
        GameObject.Find("Monster Hurt Sound").GetComponent<AudioSource>().PlayDelayed(0.1f);
        while (i < ANIM_DUR_TIME)
        {
            i += Time.deltaTime;
            is_anim = true;
            yield return 0;
        }
        for (i = 0; i < ANIM_PADDING_TIME; i += Time.deltaTime)
            yield return 0;
        is_anim = false;

        // end
        // this will check if a boss's hp is 0, if true, then erase it
        if (thisBoss.hp == 0)
        {
            thisBoss = null;
            levelMap.theMonsters.HurtBossById(bossIndex);
        }
        else
        {
            // boss is hurted by an obs created on it, if boss was not dead, destroy obs
            levelMap.theObstacles.ObsDestroy(thisBoss.GetPostion());
            
            thisBoss.FaceTo = thisBoss.faceTo;
        }
    }

    public IEnumerator BossAbilityAnim()
    {
        float i = 0;
        float BOSS_ABILITY_ANIM_DUR_TIME = 0.1f;
        int dh = 0, dw = 0;
        List<GameObject> destroyObsObj = new List<GameObject>();

        while (is_anim)
            yield return 0;

        foreach (BossMonster x in levelMap.theMonsters.bossList)
        {
            if (x.decision == DECISION.ABILITY) // attack or ability
            {
                //Debug.Log("boss" + x.id + " try to destroy obs by ability");
                int pos = -1, lookat = -1; // side -1, middle 0, side 1
                dh = ((int)x.faceTo % 2 == 0) ? ((int)x.faceTo - 1) : 0;
                dw = ((int)x.faceTo % 2 == 1) ? ((int)x.faceTo - 2) : 0;
                while (lookat <= 1)
                {
                    pos = (x.h + dh) * levelMap.width + x.w + dw;
                    pos += ((int)x.faceTo % 2 == 0) ? lookat : (lookat * levelMap.width);
                    lookat++;
                    levelMap.theMonsters.HurtMonsterByPos(pos);
                    if (levelMap.theObstacles.positionList.Exists(o => o == pos))
                    {
                        // to be Destroyed: scale < 0.5
                        levelMap.theObstacles.positionList.Remove(pos);
                        GameObject thisObsObj = GameObject.Find("Obstacle Sprite" + pos.ToString());
                        SpriteRenderer thisObsSprite = thisObsObj.GetComponent<SpriteRenderer>();
                        thisObsSprite.transform.localScale = new Vector3(1f, 0.45f, 1f);
                        thisObsSprite.transform.position -= new Vector3(0f, 0.27f, 0f);
                        destroyObsObj.Add(thisObsObj);
                    }
                }
                switch (x.faceTo)
                {
                    case FACETO.UP:
                        x.facingSprite.sprite = GameObject.Find("Back Boss Sprite Ability").GetComponent<SpriteRenderer>().sprite;
                        break;
                    case FACETO.LEFT:
                        x.facingSprite.sprite = GameObject.Find("Left Boss Sprite Ability").GetComponent<SpriteRenderer>().sprite;
                        break;
                    case FACETO.DOWN:
                        x.facingSprite.sprite = GameObject.Find("Front Boss Sprite Ability").GetComponent<SpriteRenderer>().sprite;
                        break;
                    case FACETO.RIGHT:
                        x.facingSprite.sprite = GameObject.Find("Right Boss Sprite Ability").GetComponent<SpriteRenderer>().sprite;
                        break;
                }
            }
        }
        // update
        if (levelMap.theMonsters.bossList.Count > 0 && destroyObsObj.Count > 0)
        {
            while (i <= BOSS_ABILITY_ANIM_DUR_TIME)
            {
                foreach (GameObject x in destroyObsObj)
                {
                    if (x != null)
                    {
                        SpriteRenderer thisObsSprite = x.GetComponent<SpriteRenderer>();
                        if (thisObsSprite.transform.localScale.y < 0.5f)
                        // to be Destroyed
                        {
                            if (thisObsSprite.transform.localScale.y > 0.001f)
                            {
                                thisObsSprite.transform.localScale -= new Vector3(0f, 0.45f * (Time.deltaTime / BOSS_ABILITY_ANIM_DUR_TIME * 0.99f), 0f);
                                thisObsSprite.transform.position -= new Vector3(0f, 0.27f * (Time.deltaTime / BOSS_ABILITY_ANIM_DUR_TIME * 0.99f), 0f);
                            }
                        }
                        //Debug.Log(x.name + " is going to be destroyed");
                    }
                    else
                    {
                        Debug.Log("a boss wants a destroy " + x.name + ", but it is a null reference");
                    }
                }
                i += Time.deltaTime;
                is_bossability = true;
                yield return 0;
            }
            // end
            foreach (GameObject x in destroyObsObj)
            {
                try
                {
                    Destroy(x);
                    //Debug.Log("destroyed " + x.name);
                }
                catch (UnityException e)
                {

                }
            }
            foreach (BossMonster x in levelMap.theMonsters.bossList)
            {
                x.FaceTo = x.faceTo;
            }
            is_bossability = false;
        }
        if (levelMap.thePlayer.IsPlayerAttacked())
        {
            StartCoroutine(PlayerHurtedAnim());
            if (levelMap.thePlayer.HP <= 0)
                levelMap.thePlayer.theControlPanel.ToggleFailMenu();
        }
        else
        {
            levelMap.thePlayer.CheckPlayerBlocked();
        }
        is_bossability = false;
    }

    // Use this for initialization
    void Start()
    {
    }

    public void Initialize()
    {
        Input.multiTouchEnabled = true;
        levelMap = GameObject.Find("Game Panel").GetComponent<Level_Map>();
        Game_Panel = GameObject.Find("Game Panel");
    }

    /*
     * VIEW ALL MAP MODE
     * */
    public void ToggleViewMapMode(bool useAnim = true)
    {
        float s = Mathf.Max(0.45f, Mathf.Min(12f / levelMap.height, 12f / levelMap.width));
        float dh = (levelMap.height / 2 - levelMap.thePlayer.h) - 0.1f;
        float dw = (levelMap.thePlayer.w - levelMap.width / 2);
        
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            o.GetComponent<SpriteRenderer>().enabled = is_vmm;
        }
        GameObject.Find("Field Frontground Outring").GetComponent<SpriteRenderer>().enabled = is_vmm;
        if (is_vmm)
        {
            vamm_pos = new Vector3(0f, -0.1f);
            vamm_scale = new Vector3(1, 1, 1);
            GameObject.Find("Map Button Text").GetComponent<Text>().text = "觀看全地圖";
            GameObject.Find("View Map Description").GetComponent<Text>().text = "";
        }
        else
        {
            vamm_pos = new Vector3(dw + 1f, dh + 0.1f);
            vamm_scale = new Vector3(s, s, 1);
            GameObject.Find("Map Button Text").GetComponent<Text>().text = "回到目前位置";
            GameObject.Find("View Map Description").GetComponent<Text>().text = "現可拖移、縮放\n瀏覽地圖";
            is_vmm = true;
        }
        if (vamm_scale != new Vector3(1, 1, 1))
        {
            GameObject.Find("Player Control Canvas").GetComponent<Canvas>().enabled = false;
            levelMap.thePlayer.thePlayerDisp.playerFacingSprite.enabled = false;
            GameObject.Find("Player State Canvas").GetComponent<Canvas>().enabled = false;
        }
        if (useAnim)
            StartCoroutine(ViewMapModeAnim());
        else
        {
            Game_Panel.transform.localScale = vamm_scale;
            Game_Panel.transform.position = vamm_pos;
            if (!GameObject.Find("Player Control Canvas").GetComponent<Canvas>().enabled)
                GameObject.Find("Player Control Canvas").GetComponent<Canvas>().enabled = true;
            is_vmm = (vamm_scale != new Vector3(1, 1, 1));
        }
        //time_view_map_mode = Time.time + Animation.ANIM_DUR_TIME / 12;
    }

    private IEnumerator ViewMapModeAnim()
    {
        float i = 0;
        float speed = 0.75f;
        float vmm_anim_dur_time = 1.0f;
        while (Mathf.Abs(Game_Panel.transform.position.magnitude - vamm_pos.magnitude) >= 0.01f)
        {
            Game_Panel.transform.position += (vamm_pos - Game_Panel.transform.position) * (speed * i / vmm_anim_dur_time);
            Game_Panel.transform.localScale += (vamm_scale - Game_Panel.transform.localScale) * (speed * i / vmm_anim_dur_time);
            is_anim = true;
            i += Time.deltaTime;
            yield return 0;
        }
        is_anim = false;
        Game_Panel.transform.localScale = vamm_scale;
        Game_Panel.transform.position = vamm_pos;
        if (vamm_scale == new Vector3(1, 1, 1))
        {
            GameObject.Find("Player Control Canvas").GetComponent<Canvas>().enabled = true;
            levelMap.thePlayer.thePlayerDisp.playerFacingSprite.enabled = true;
            GameObject.Find("Player State Canvas").GetComponent<Canvas>().enabled = true;
        }
        is_vmm = (vamm_scale != new Vector3(1, 1, 1));
    }

    private void ViewMapModeMouseZoom(float y)
    {
        float ds = y * 0.025f;
        Vector3 n = Game_Panel.transform.localScale + new Vector3(ds, ds);
        if (n.x > 0.2f && n.x <= 1.2f)
        {
            Game_Panel.transform.localScale = n;
            if (Game_Panel.transform.position.x > vamm_pos.x + levelMap.width / 2 * n.x)
                Game_Panel.transform.position = new Vector3(vamm_pos.x + levelMap.width / 2 * n.x, Game_Panel.transform.position.y, 0);
            else if (Game_Panel.transform.position.x < vamm_pos.x - levelMap.width / 2 * n.x)
                Game_Panel.transform.position = new Vector3(vamm_pos.x - levelMap.width / 2 * n.x, Game_Panel.transform.position.y, 0);
            if (Game_Panel.transform.position.y > vamm_pos.y + levelMap.height / 2 * n.y)
                Game_Panel.transform.position = new Vector3(Game_Panel.transform.position.x, vamm_pos.y + levelMap.height / 2 * n.y, 0);
            else if (Game_Panel.transform.position.y < vamm_pos.y - levelMap.height / 2 * n.y)
                Game_Panel.transform.position = new Vector3(Game_Panel.transform.position.x, vamm_pos.y - levelMap.height / 2 * n.y, 0);
        }
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
            {
                Game_Panel.transform.localScale = n;
                if (Game_Panel.transform.position.x > vamm_pos.x + levelMap.width / 2 * n.x)
                    Game_Panel.transform.position = new Vector3(vamm_pos.x + levelMap.width / 2 * n.x, Game_Panel.transform.position.y, 0);
                else if (Game_Panel.transform.position.x < vamm_pos.x - levelMap.width / 2 * n.x)
                    Game_Panel.transform.position = new Vector3(vamm_pos.x - levelMap.width / 2 * n.x, Game_Panel.transform.position.y, 0);
                if (Game_Panel.transform.position.y > vamm_pos.y + levelMap.height / 2 * n.y)
                    Game_Panel.transform.position = new Vector3(Game_Panel.transform.position.x, vamm_pos.y + levelMap.height / 2 * n.y, 0);
                else if (Game_Panel.transform.position.y < vamm_pos.y - levelMap.height / 2 * n.y)
                    Game_Panel.transform.position = new Vector3(Game_Panel.transform.position.x, vamm_pos.y - levelMap.height / 2 * n.y, 0);
            }
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

    private void HandleDragMoveControl()
    {
        float minDeltaDistance = 9f;
        if (Input.GetTouch(0).phase == TouchPhase.Moved && !is_irresponsive)
        {
            float m = Input.GetTouch(0).deltaPosition.x / Input.GetTouch(0).deltaPosition.y;
            if (m > 3.33f) // left & right
            {
                if (Input.GetTouch(0).deltaPosition.x > minDeltaDistance)
                    levelMap.thePlayer.PlayerMove((int)FACETO.LEFT);
                else if (Input.GetTouch(0).deltaPosition.x < -minDeltaDistance)
                    levelMap.thePlayer.PlayerMove((int)FACETO.RIGHT);
            }
            else if  (m < (1f / 4f)) // up & down
            {
                if (Input.GetTouch(0).deltaPosition.y > minDeltaDistance)
                    levelMap.thePlayer.PlayerMove((int)FACETO.DOWN);
                else if (Input.GetTouch(0).deltaPosition.y < -minDeltaDistance)
                    levelMap.thePlayer.PlayerMove((int)FACETO.UP);
            }
        }
    }

    float lastProgressTime = 0;
    int doubleTapStep = 0;

    // Update is called once per frame
    void Update()
    {
        is_irresponsive = is_anim || is_bossability || is_vmm;

        if (times_monster_change_sprite < Time.time)
        {
            if (times_monster_change_sprite == 0)
                times_monster_change_sprite = Time.time + 1f;
            else
                times_monster_change_sprite += 1f;
            levelMap.theMonsters.AllChangeFrame();
        }

        // can do drags to look around map in View-Map-Mode
        if (is_vmm)
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
        else if(Input.touchSupported && Input.touchCount == 1)
        {
            HandleDragMoveControl();
        }

        // handle double tap
        if (doubleTapStep == 0 || doubleTapStep == 2)
        {
            if (Input.touchCount == 1)
            {
                if (Input.GetTouch(0).deltaTime < 0.12f)
                {
                    if (doubleTapStep == 2 && (Time.time - lastProgressTime) < 0.24f && !is_irresponsive)
                    {
                        levelMap.thePlayer.PlayerDoAbility();
                    }
                    doubleTapStep = 1;
                    lastProgressTime = Time.time;
                }
            }
        }
        else if (doubleTapStep == 1)
        {
            if (Input.touchCount == 0)
            {
                doubleTapStep = 2;
            }
        }

        // can't do control in View-Map-Mode
        // for playing on PC
        if (!is_irresponsive)
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                levelMap.thePlayer.PlayerMove((int)FACETO.UP);
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                levelMap.thePlayer.PlayerMove((int)FACETO.LEFT);
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                levelMap.thePlayer.PlayerMove((int)FACETO.DOWN);
            }
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                levelMap.thePlayer.PlayerMove((int)FACETO.RIGHT);
            }
            else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.Space))
            {
                levelMap.thePlayer.PlayerDoAbility();
            }
        }
    }
}
