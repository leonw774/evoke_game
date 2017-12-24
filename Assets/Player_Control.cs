using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player_Control : MonoBehaviour {
    public enum FACING : int {FRONT = 0, LEFT, BACK, RIGHT};
    public int h;
    public int w;

    public int energyPoint;
    public int healthPoint;

    private int abilityCooldown;

    private GameObject playerPositionObject;
    private SpriteRenderer playerFacingSprite = null;
    private Text energyPointObject;
    private Text healthPointObject;
    private Text abilityCooldownObject;
    private AudioSource abilitySound;
    private Game_Menu theControlPanel;
    private Level_Map levelMap;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Player_Control.Start()");
    }

    public void Initialize()
    {
        playerPositionObject = GameObject.Find("Player Sprite");
        theControlPanel = GameObject.Find("Game Menu Canvas").GetComponent<Game_Menu>();
        levelMap = GameObject.Find("Game Panel").GetComponent<Level_Map>();
        energyPointObject = GameObject.Find("EP Output").GetComponent<Text>();
        healthPointObject = GameObject.Find("HP Output").GetComponent<Text>();
        abilityCooldownObject = GameObject.Find("CD Output").GetComponent<Text>();
        abilitySound = GameObject.Find("Ability Sound").GetComponent<AudioSource>();
        abilitySound.playOnAwake = false;
    }

    public void playerMoveUp()
    {
        if (times_irreponsive <= Time.time)
        {
            if (Move(-1, 0)) // it is monster's turn only if player did change position
            {
                SetFaceTo(FACING.BACK);
                levelMap.theMonsters.MonstersMove();
                AnimSetup();
                if (energyPoint == 0) theControlPanel.toggleFailMenu();
            }
        }
    }

    public void playerMoveLeft()
    {
        if (times_irreponsive <= Time.time)
        {
            if (Move(0, -1))
            {
                SetFaceTo(FACING.LEFT);
                levelMap.theMonsters.MonstersMove();
                AnimSetup();
                if (energyPoint == 0) theControlPanel.toggleFailMenu();
            }
        }
    }

    public void playerMoveDown()
    {
        if (times_irreponsive <= Time.time)
        {
            if (Move(1, 0))
            {
                SetFaceTo(FACING.FRONT);
                levelMap.theMonsters.MonstersMove();
                AnimSetup();
                if (energyPoint == 0) theControlPanel.toggleFailMenu();
            }
        }
    }

    public void playerMoveRight()
    {
        if (times_irreponsive <= Time.time)
        {
            if (Move(0, 1))
            {
                SetFaceTo(FACING.RIGHT);
                levelMap.theMonsters.MonstersMove();
                AnimSetup();
                if (energyPoint == 0) theControlPanel.toggleFailMenu();
            }
        }
    }

    public void playerDoAbility()
    {
        if (times_irreponsive <= Time.time)
        {
            if (DoAbility())
            {
                abilitySound.Play();
                levelMap.theMonsters.MonstersMove();
                AnimSetup();
                if (energyPoint == 0) theControlPanel.toggleFailMenu();
            }
        }
    }

    /* HANDEL REAL THING THERE */

    // retrun true: player did change position; return false: player didn't move
    private bool Move(int dh, int dw)
    {
        //Debug.Log("thePlayer.Move(" + dh + ", " + dw + ") is called");
        //Debug.Log("plar wanna go to " + (h + dh) + ", " + (w + dw));
        if (theControlPanel.isMenuActive)
            return false;
        else if (levelMap.tiles[(h + dh), (w + dw)] != (int)Level_Map.TILE_TYPE.WALL
            && !levelMap.theObstacles.positionList.Exists(x => x == (h + dh) * levelMap.width + (w + dw)))
        {
            h = h + dh;
            w = w + dw;
            //Debug.Log("player position has been changed to (" + h + ", " + w + ")");
            PlayerAnimSetup(playerPositionObject.transform.position, new Vector3((w - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - h - 0.5f), 0));
            energyPointObject.text = (--energyPoint).ToString();
            SetAbilityCooldown(--abilityCooldown);
        }
        else if ((h + dh) == levelMap.finishTile[0] && (w + dw) == levelMap.finishTile[1])
        {
            PlayerAnimSetup(playerPositionObject.transform.position, new Vector3((w - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - h - 0.5f), 0));
            theControlPanel.toggleFinishMenu();
            levelMap.GameFinish();
        }
        else
        {
            return false;
        }
        return true;
    }

    // retrun true: player did do ability; return false: player didn't make it
    private bool DoAbility()
    {
        int dh = -1, dw = -1;
        if (theControlPanel.isMenuActive || abilityCooldown > 0)
            return false;
        while (dh <= 1)
        {
            levelMap.theObstacles.ObsUpdate(h + dh, w + dw);
            // upadte neighbor tiles ij
            if (dw == 1)
            {
                dh++;
                dw = -1;
            }
            else if (dh == 0 & dw == -1) dw = 1;
            else dw++;
        }
        SetAbilityCooldown(1);
        energyPointObject.text = (--energyPoint).ToString();
        return true;
    }

    private void CheckPlayerAttacked()
    {
        // if player finish the map, monster can not fail it afterward.
        if (theControlPanel.isFinishMenu)
            return;
        
        int success = levelMap.theMonsters.TryAttackPlayer(h * levelMap.width + w);
        if (success > 0)
        {
            healthPoint -= success;
            healthPointObject.text = healthPoint.ToString();
            AttackedAnimStart();
        }
    }

    private void CheckPlayerBlocked()
    {
        bool blocked = (levelMap.tiles[(h + 1), w] == (int)Level_Map.TILE_TYPE.WALL || levelMap.theObstacles.positionList.Exists(x => x == (h + 1) * levelMap.width + w))
            && (levelMap.tiles[(h - 1), w] == (int)Level_Map.TILE_TYPE.WALL || levelMap.theObstacles.positionList.Exists(x => x == (h - 1) * levelMap.width + w))
            && (levelMap.tiles[h, (w + 1)] == (int)Level_Map.TILE_TYPE.WALL || levelMap.theObstacles.positionList.Exists(x => x == h * levelMap.width + (w + 1)))
                && (levelMap.tiles[h, (w - 1)] == (int)Level_Map.TILE_TYPE.WALL || levelMap.theObstacles.positionList.Exists(x => x == h * levelMap.width + (w - 1)));
        if (blocked)
            theControlPanel.toggleFailMenu();
    }

    /* SET VALUES */

    public void SetPositionTo(int newh, int neww)
    {
        Debug.Log("Player_Control.SetPositionTo(): thePlayer.SetPositionTo()");
        if (newh >= levelMap.height || neww >= levelMap.width || newh < 0 || neww < 0)
        {
            Debug.Log("illegal position");
            return;
        }
        //Debug.Log("thePlayer.SetPositionTo() is called in Game_Panel");
        if (levelMap.tiles[newh, neww] != (int)Level_Map.TILE_TYPE.WALL)
        {
            if (levelMap.theObstacles.positionList.Exists(x => x == (newh * levelMap.width + neww)))
            {
                levelMap.theObstacles.ObsUpdate(newh, neww);
            }
            h = newh;
            w = neww;
            //Debug.Log("player position has been changed to (" + h + ", " + w + ")");
            playerPositionObject.transform.position = new Vector3((w - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - h - 0.5f), 0);
        }
        else
        {
            Debug.Log("Player_Control.SetPositionTo(): illegal position");
        }
    }

    public void SetEnergyPoint(int e)
    {
        energyPointObject.text = (energyPoint = e).ToString();
    }

    public void SetHealthPoint(int h)
    {
        healthPointObject.text = (healthPoint = h).ToString();
    }

    public void SetAbilityCooldown(int cd)
    {
        if (cd > 0)
        {
            abilityCooldownObject.text = (abilityCooldown = cd).ToString();
            GameObject.Find("Ability Button").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("Ability Button").GetComponent<Button>().interactable = true;
            abilityCooldownObject.text = " ";
            abilityCooldown = 0;
        }
    }

    public void SetFaceTo(FACING ft)
    {
        if (playerFacingSprite != null)
            playerFacingSprite.enabled = false;
        switch (ft)
        {
            case FACING.FRONT:
                playerFacingSprite = GameObject.Find("Front Player Sprite").GetComponent<SpriteRenderer>();
                break;
            case FACING.LEFT:
                playerFacingSprite = GameObject.Find("Left Player Sprite").GetComponent<SpriteRenderer>();
                break;
            case FACING.BACK:
                playerFacingSprite = GameObject.Find("Back Player Sprite").GetComponent<SpriteRenderer>();
                break;
            case FACING.RIGHT:
                playerFacingSprite = GameObject.Find("Right Player Sprite").GetComponent<SpriteRenderer>();
                break;
            default:
                return;
        }
        playerFacingSprite.enabled = true;
    }

    /* ANIMATION */

    /*
     * in player.move() -> setup begin & end pos (become not 0,0,0)
     * in monsters.move() -> sset up begin & end pos (become not 0,0,0)
     * 
     * animsetup()
     * 
     * Update begin to call Anim()
     * 
     * Anim() do PlayerAnim & monsterAnim
     * Anim() find it should stop
     * Anim() call PlayerAnimEnd() & MonsterAnimEnd()
     * 
     * PlayerAnimEnd() set begin & end back to 0,0,0
     * PlayerAnimEnd() check if player is attcked
     * MonsterAnimEnd() set begin & end back to 0,0,0
     * */
    
    void PlayerAnimSetup(Vector3 begin, Vector3 end)
    {
        animBeginPos = begin;
        animEndPos = end;
    }

    void PlayerAnim()
    {
        playerPositionObject.transform.position = playerPositionObject.transform.position + (animEndPos - animBeginPos) / (Time.deltaTime / 0.0014f);
    }

    void PlayerAnimEnd()
    {
        playerPositionObject.transform.position = animEndPos;
        animEndPos = new Vector3(0.0f, 0.0f, -1.0f);
        animBeginPos = new Vector3(0.0f, 0.0f, -1.0f);
        CheckPlayerAttacked();
    }

    void AttackedAnimStart()
    {
        times_irreponsive = Time.time + 0.2f;
        playerAttackedAnimation = true;
        GameObject.Find("Player Attacked Effect").GetComponent<SpriteRenderer>().enabled = true;
    }

    void AttackedAnimEnd()
    {
        playerAttackedAnimation = false;
        GameObject.Find("Player Attacked Effect").GetComponent<SpriteRenderer>().enabled = false;
        if (healthPoint <= 0)
            theControlPanel.toggleFailMenu();
    }

    void AnimSetup()
    {
        times_irreponsive = Time.time + animDurTime;
        moveAnimation = true;
    }

    void Anim()
    {
        
        if (times_irreponsive <= Time.time || player_ask_for_end_anim || monsters_ask_for_end_anim)
        {
            moveAnimation = false;
            monsters_ask_for_end_anim = false;
            player_ask_for_end_anim = false;

            // tidy up player pos
            if (animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
                PlayerAnimEnd();
            else
                CheckPlayerBlocked();

            // tidy up monster pos
            levelMap.theMonsters.MonstersAnimEnd();
        }
        else
        {
            if (animBeginPos != new Vector3(0.0f, 0.0f, -1.0f))
            {
                PlayerAnim();
                player_ask_for_end_anim = (animEndPos - playerPositionObject.transform.position).magnitude < 0.01
                                    || (animEndPos - playerPositionObject.transform.position).normalized == (animBeginPos - animEndPos).normalized;
            }
            monsters_ask_for_end_anim = levelMap.theMonsters.MonstersAnim();
        }
    }

    float times_irreponsive = 0;
    float times_monster_change_sprite = 0;
    float animDurTime = 0.2f;
    Vector3 animBeginPos;
    Vector3 animEndPos;
    bool moveAnimation = false;
    bool playerAttackedAnimation = false;
    bool monsters_ask_for_end_anim = false, player_ask_for_end_anim = false;
    // Update is called once per frame
    void Update()
    {
        if (times_monster_change_sprite <= Time.time)
        {
            levelMap.theMonsters.MonstersChangeFrame();
            times_monster_change_sprite += 1.1f;
        }

        if (playerAttackedAnimation && times_irreponsive <= Time.time)
        {
            AttackedAnimEnd();
        }

        /* for testing on PC */
        if (Input.GetKey(KeyCode.UpArrow))
        {
            playerMoveUp();
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            playerMoveDown();
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            playerMoveLeft();
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            playerMoveRight();
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            playerDoAbility();
        }
        /* for testing on PC */

        if (moveAnimation)
        {
            Anim();
        }
    }
}
