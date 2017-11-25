using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Player_Control : MonoBehaviour {
    public enum FACING : int {UP = 0, LEFT, DOWN, RIGHT};
    public int h;
    public int w;

    public int energyPoint;
    public int healthPoint;

    private FACING faceTo;
    private int abilityCooldown;

    private GameObject playerSpriteObject;
    private Text energyPointObject;
    private Text healthPointObject;
    private Text abilityCooldownObject;
    private Game_Menu theControlPanel;
    private Level_Map levelMap;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Player_Control.Start()");
    }

    public void Initialize()
    {
        playerSpriteObject = GameObject.Find("Player Sprite");
        theControlPanel = GameObject.Find("Game Menu Canvas").GetComponent<Game_Menu>();
        levelMap = GameObject.Find("Game Panel").GetComponent<Level_Map>();
        energyPointObject = GameObject.Find("EP Output").GetComponent<Text>();
        healthPointObject = GameObject.Find("HP Output").GetComponent<Text>();
        abilityCooldownObject = GameObject.Find("CD Output").GetComponent<Text>();
    }

    public void playerMoveUp()
    {
        faceTo = FACING.UP;
        if (Move(-1, 0)) // it is monster's turn only if player did change position
        {
            levelMap.theMonsters.MonstersMove();
            AnimSetup();
            if (energyPoint == 0) theControlPanel.toggleFailMenu();
        }
    }

    public void playerMoveLeft()
    {
        faceTo = FACING.LEFT;
        if (Move(0, -1))
        {
            levelMap.theMonsters.MonstersMove();
            AnimSetup();
            if (energyPoint == 0) theControlPanel.toggleFailMenu();
        }
    }

    public void playerMoveDown()
    {
        faceTo = FACING.DOWN;
        if (Move(1, 0))
        {
            levelMap.theMonsters.MonstersMove();
            AnimSetup();
            if (energyPoint == 0) theControlPanel.toggleFailMenu();
        }
    }

    public void playerMoveRight()
    {
        faceTo = FACING.RIGHT;
        if (Move(0, 1))
        {
            levelMap.theMonsters.MonstersMove();
            AnimSetup();
            if (energyPoint == 0) theControlPanel.toggleFailMenu();
        }
    }

    public void playerDoAbility()
    {
        DoAbility();
        levelMap.theMonsters.MonstersMove();
        AnimSetup();
        if (energyPoint == 0) theControlPanel.toggleFailMenu();
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
            PlayerAnimSetup(playerSpriteObject.transform.position, new Vector3((w - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - h - 0.5f), 0));
            energyPointObject.text = (--energyPoint).ToString();
            SetAbilityCooldown(--abilityCooldown);
        }
        else if ((h + dh) == levelMap.finishTile[0] && (w + dw) == levelMap.finishTile[1])
        {
            PlayerAnimSetup(playerSpriteObject.transform.position, new Vector3((w - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - h - 0.5f), 0));
            theControlPanel.toggleFinishMenu();
            levelMap.GameFinish();
        }
        else
        {
            return false;
        }
        return true;
    }

    private void DoAbility()
    {
        int dh = -1, dw = -1;
        if (theControlPanel.isMenuActive || abilityCooldown > 0)
            return;
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
    }

    private void CheckPlayerAttacked()
    {
        bool success = levelMap.theMonsters.TryAttackPlayer(h * levelMap.width + w);
        if (success)
        {
            healthPoint--;
            healthPointObject.text = healthPoint.ToString();
            if (healthPoint == 0)
                theControlPanel.toggleFailMenu();
        }
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
            playerSpriteObject.transform.position = new Vector3((w - levelMap.width / 2.0f + 0.5f), (levelMap.height / 2.0f - h - 0.5f), 0);
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
        playerSpriteObject.transform.position = playerSpriteObject.transform.position + (animEndPos - animBeginPos) / (Time.deltaTime / 0.0014f);
    }

    void PlayerAnimEnd()
    {
        playerSpriteObject.transform.position = animEndPos;
        animEndPos = new Vector3(0.0f, 0.0f, 0.0f);
        animBeginPos = new Vector3(0.0f, 0.0f, 0.0f);
        CheckPlayerAttacked();
    }

    void AnimSetup()
    {
        times_irreponsive = Time.time + animDurTime;
        moveAnimation = true;
    }

    void Anim()
    {
        // do animation in the irreponsive time
        if (times_irreponsive <= Time.time
         || (animEndPos - playerSpriteObject.transform.position).magnitude < 0.01
         || (animEndPos - playerSpriteObject.transform.position).normalized == (animBeginPos - animEndPos).normalized)
        {
            moveAnimation = false;
            // tidy up player pos
            if (animBeginPos != new Vector3(0.0f, 0.0f, 0.0f))
                PlayerAnimEnd();
            // tidy up monster pos
            levelMap.theMonsters.MonstersAnimEnd();
        }
        else
        {
            if (animBeginPos != new Vector3(0.0f, 0.0f, 0.0f))
                PlayerAnim();
            levelMap.theMonsters.MonstersAnim();
        }
    }

    float times_irreponsive = 0;
    float animDurTime = 0.2f;
    Vector3 animBeginPos;
    Vector3 animEndPos;
    bool moveAnimation = false;
    // Update is called once per frame
    void Update()
    {
        if (times_irreponsive <= Time.time)
        {
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
        }
        if (moveAnimation)
        {
            Anim();
        }
    }
}
