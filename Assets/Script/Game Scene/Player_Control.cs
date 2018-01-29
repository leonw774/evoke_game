﻿using TileTypeDefine;
using UnityEngine;
using UnityEngine.UI;

public enum CHARACTER_FACING : int {BACK = 0, LEFT, FRONT, RIGHT};

public class Player_Display
{
    private GameObject playerPositionObject;
    public SpriteRenderer playerFacingSprite;
    public Vector3 animBeginPos;
    public Vector3 animEndPos;
    public Vector3 ObjPos
    {
        set
        {
            playerPositionObject.transform.position = value;
        }
        get
        {
            return playerPositionObject.transform.position;
        }
    }

    public Player_Display()
    {
        playerPositionObject = GameObject.Find("Panel Offset");
        playerFacingSprite = null;
        animBeginPos = new Vector3(0, 0, -1);
        animEndPos = new Vector3(0, 0, -1);
    }

    public void ChangeFacingSpriteTo(CHARACTER_FACING ft)
    {
        if (playerFacingSprite != null)
            playerFacingSprite.enabled = false;
        switch (ft)
        {
            case CHARACTER_FACING.FRONT:
                playerFacingSprite = GameObject.Find("Front Player Sprite").GetComponent<SpriteRenderer>();
                break;
            case CHARACTER_FACING.LEFT:
                playerFacingSprite = GameObject.Find("Left Player Sprite").GetComponent<SpriteRenderer>();
                break;
            case CHARACTER_FACING.BACK:
                playerFacingSprite = GameObject.Find("Back Player Sprite").GetComponent<SpriteRenderer>();
                break;
            case CHARACTER_FACING.RIGHT:
                playerFacingSprite = GameObject.Find("Right Player Sprite").GetComponent<SpriteRenderer>();
                break;
            default:
                return;
        }
        playerFacingSprite.enabled = true;
    }
}

public class Player_Control : MonoBehaviour {
    public int h;
    public int w;

    public int energyPoint;
    public int healthPoint;
    private int abilityCooldown;

    private Text energyPointObject;
    private Text healthPointObject;
    private Text abilityCooldownObject;
    private AudioSource abilitySound, moveSound;

    public Control_Animation theAnimation;
    public Player_Display thePlayerDisp;
    public Game_Menu theControlPanel;
    private Level_Map levelMap;

    // Use this for initialization
    void Start()
    {
        //Debug.Log("Player_Control.Start()");
    }

    public void Initialize()
    {
        theAnimation = GameObject.Find("Control Panel").GetComponent<Control_Animation>();
        thePlayerDisp = new Player_Display();
        theControlPanel = GameObject.Find("Game Menu Canvas").GetComponent<Game_Menu>();
        levelMap = GameObject.Find("Game Panel").GetComponent<Level_Map>();

        energyPointObject = GameObject.Find("EP Output").GetComponent<Text>();
        healthPointObject = GameObject.Find("HP Output").GetComponent<Text>();
        abilityCooldownObject = GameObject.Find("CD Output").GetComponent<Text>();
        moveSound = GameObject.Find("Move Sound").GetComponent<AudioSource>();
        abilitySound = GameObject.Find("Ability Sound").GetComponent<AudioSource>();
    }

    public void PlayerMoveUp()
    {
        if (!theAnimation.is_irresponsive && !theAnimation.isViewMapMode && !theAnimation.viewMapModeAnimation)
        {
            thePlayerDisp.ChangeFacingSpriteTo(CHARACTER_FACING.BACK);
            if (Move(-1, 0)) // it is monster's turn only if player did change position
            {
                moveSound.Play();
                levelMap.theMonsters.MonstersTurn();
                levelMap.theAnimation.playerAnim.Start();
                levelMap.theAnimation.monstersAnim.Start();
            }
        }
    }

    public void PlayerMoveLeft()
    {
        if (!theAnimation.is_irresponsive && !theAnimation.isViewMapMode && !theAnimation.viewMapModeAnimation)
        {
            thePlayerDisp.ChangeFacingSpriteTo(CHARACTER_FACING.LEFT);
            if (Move(0, -1))
            {
                moveSound.Play();
                levelMap.theMonsters.MonstersTurn();
                levelMap.theAnimation.playerAnim.Start();
                levelMap.theAnimation.monstersAnim.Start();
            }
        }
    }

    public void PlayerMoveDown()
    {
        if (!theAnimation.is_irresponsive && !theAnimation.isViewMapMode && !theAnimation.viewMapModeAnimation)
        {
            thePlayerDisp.ChangeFacingSpriteTo(CHARACTER_FACING.FRONT);
            if (Move(1, 0))
            {
                moveSound.Play();
                levelMap.theMonsters.MonstersTurn();
                levelMap.theAnimation.playerAnim.Start();
                levelMap.theAnimation.monstersAnim.Start();
            }
        }
    }

    public void PlayerMoveRight()
    {
        if (!theAnimation.is_irresponsive && !theAnimation.isViewMapMode && !theAnimation.viewMapModeAnimation)
        {
            thePlayerDisp.ChangeFacingSpriteTo(CHARACTER_FACING.RIGHT);
            if (Move(0, 1))
            {
                moveSound.Play();
                levelMap.theMonsters.MonstersTurn();
                levelMap.theAnimation.playerAnim.Start();
                levelMap.theAnimation.monstersAnim.Start();
            }
        }
    }

    public void PlayerDoAbility()
    {
        if (!theAnimation.is_irresponsive && !theAnimation.isViewMapMode && !theAnimation.viewMapModeAnimation)
        {
            if (DoAbility())
            {
                abilitySound.Play();
                levelMap.theAnimation.playerAbilityAnim.Start();
                levelMap.theMonsters.MonstersTurn();
                levelMap.theAnimation.monstersAnim.Start();
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
        else if (levelMap.IsTileWalkable(h + dh, w + dw))
        {
            h = h + dh;
            w = w + dw;
            //Debug.Log("player position has been changed to (" + h + ", " + w + ")");
            thePlayerDisp.animBeginPos = thePlayerDisp.ObjPos;
            thePlayerDisp.animEndPos = levelMap.MapCoordToWorldVec3(h, w, 0);
            SetEnergyPoint(energyPoint - 1);
            SetAbilityCooldown(--abilityCooldown);
            return true;
        }
        else if ((h + dh) == levelMap.finishTile[0] && (w + dw) == levelMap.finishTile[1])
        {
            if (Save_Data.SelectedLevel != Save_Data.BossLevel || levelMap.theMonsters.boss == null)
            {
                levelMap.UpdateSaveLevel();
                theControlPanel.ToggleFinishMenu();
                return true;
            }
        }
        return false;       
    }

    // retrun true: player did do ability; return false: player didn't make it
    private bool DoAbility()
    {
        if (theControlPanel.isMenuActive || abilityCooldown > 0)
            return false;
        SetAbilityCooldown(1);
        energyPointObject.text = (--energyPoint).ToString();
        return true;
    }

    /* Checks for Animation */

    public bool IsPlayerAttacked()
    {
        // if player finish the map, monster can not fail it afterward.
        if (theControlPanel.isFinishMenu)
            return false;
        
        int success = levelMap.theMonsters.TryAttackPlayer(h * levelMap.width + w);
        if (levelMap.theObstacles.positionList.Exists(x => x == h * levelMap.width + w))
            success++;
        if (success > 0)
        {
            // Debug.Log("player hurted");
            // try destroy obs because player might be hurted by obs in boss monster attack
            levelMap.theObstacles.ObsDestroy(h * levelMap.width + w);
            SetHealthPoint(healthPoint - success);
            return true;
        }
        return false;
    }

    public void CheckPlayerBlocked()
    {
        bool not_blocked = (levelMap.IsTileWalkable(h + 1, w) ||
                        levelMap.IsTileWalkable(h - 1, w) ||
                        levelMap.IsTileWalkable(h, w + 1) ||
                        levelMap.IsTileWalkable(h, w - 1));
        if (!not_blocked)
            theControlPanel.ToggleFailMenu();
    }

    /* SET VALUES */

    public void SetPositionTo(int newh, int neww)
    {
        if (newh >= levelMap.height || neww >= levelMap.width || newh < 0 || neww < 0)
        {
            Debug.Log("illegal position");
            return;
        }
        //Debug.Log("thePlayer.SetPositionTo() is called in Game_Panel");
        if (levelMap.tiles[newh, neww] != TILE_TYPE.WALL)
        {
            levelMap.theObstacles.ObsDestroy(newh * levelMap.width + neww);
            h = newh;
            w = neww;
            //Debug.Log("player position has been changed to (" + h + ", " + w + ")");
            thePlayerDisp.ObjPos = levelMap.MapCoordToWorldVec3(h, w, 0);
        }
        else
        {
            Debug.Log("Player_Control.SetPositionTo(): illegal position");
        }
    }

    public void SetEnergyPoint(int e)
    {
        energyPointObject.color = (e <= 10) ? new Color(1.0f, 0.2f, 0.2f) : new Color(0.1098f, 0.882353f, 0.882353f);
        energyPointObject.fontSize = (e <= 10) ? 36 : 30;
        energyPointObject.text = (energyPoint = e).ToString();
    }

    public void SetHealthPoint(int h)
    {
        healthPointObject.color = (h <= 1) ? new Color(1.0f, 0.2f, 0.2f) : new Color(0.1098f, 0.882353f, 0.1098f);
        healthPointObject.fontSize = (h <= 1) ? 36 : 30;
        healthPointObject.text = (healthPoint = h).ToString();
    }

    public void SetAbilityCooldown(int cd)
    {
        abilityCooldown = cd;
        if (cd > 0)
        {
            abilityCooldownObject.text = "C.D."; // abilityCooldown.ToString();
            GameObject.Find("Ability Button").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("Ability Button").GetComponent<Button>().interactable = true;
            abilityCooldownObject.text = "";
        }
    }
}
