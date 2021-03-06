﻿using UnityEngine;
using UnityEngine.UI;

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
    private FACETO faceTo;
    public FACETO FaceTo
    {
        get
        {
            return faceTo;
        }
        set
        {
            faceTo = value;
            if (playerFacingSprite != null)
                playerFacingSprite.enabled = false;
            switch (faceTo)
            {
                case FACETO.DOWN:
                    playerFacingSprite = GameObject.Find("Front Player Sprite").GetComponent<SpriteRenderer>();
                    break;
                case FACETO.LEFT:
                    playerFacingSprite = GameObject.Find("Left Player Sprite").GetComponent<SpriteRenderer>();
                    break;
                case FACETO.UP:
                    playerFacingSprite = GameObject.Find("Back Player Sprite").GetComponent<SpriteRenderer>();
                    break;
                case FACETO.RIGHT:
                    playerFacingSprite = GameObject.Find("Right Player Sprite").GetComponent<SpriteRenderer>();
                    break;
                default:
                    return;
            }
            playerFacingSprite.enabled = true;
        }
    }

    public Player_Display()
    {
        playerPositionObject = GameObject.Find("Panel Offset");
        playerFacingSprite = null;
        animBeginPos = new Vector3(0, 0, -1);
        animEndPos = new Vector3(0, 0, -1);
    }

    public void ChangeFacingSpriteTo(FACETO ft)
    {
        if (playerFacingSprite != null)
            playerFacingSprite.enabled = false;
        switch (ft)
        {
            case FACETO.DOWN:
                playerFacingSprite = GameObject.Find("Front Player Sprite").GetComponent<SpriteRenderer>();
                break;
            case FACETO.LEFT:
                playerFacingSprite = GameObject.Find("Left Player Sprite").GetComponent<SpriteRenderer>();
                break;
            case FACETO.UP:
                playerFacingSprite = GameObject.Find("Back Player Sprite").GetComponent<SpriteRenderer>();
                break;
            case FACETO.RIGHT:
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

    public Text energyPointObject;
    public Text healthPointObject;

    private int energyPoint;
    private int healthPoint;
    private int abilityCooldown;
    public int EP
    {
        get
        {
            return energyPoint;
        }
        set
        {
            energyPointObject.color = (value <= 10) ? new Color(1.0f, 0.2f, 0.2f) : new Color(0.1098f, 0.882353f, 0.882353f);
            energyPointObject.fontSize = (value <= 10) ? 22 : 20;
            energyPointObject.text = (energyPoint = value).ToString();
        }
    }
    public int HP
    {
        get
        {
            return healthPoint;
        }
        set
        {
            healthPointObject.color = (value <= 1) ? new Color(1.0f, 0.2f, 0.2f) : new Color(0.1098f, 0.882353f, 0.1098f);
            healthPointObject.fontSize = (value <= 1) ? 22 : 20;
            healthPointObject.text = (healthPoint = value).ToString();
        }
    }
    public int CD
    {
        get
        {
            return abilityCooldown;
        }
        set
        {
            abilityCooldown = value;
            if (abilityCooldown > 0)
            {
                GameObject.Find("Ability Button").GetComponent<Button>().interactable = false;
            }
            else
            {
                GameObject.Find("Ability Button").GetComponent<Button>().interactable = true;
            }
        }
        
    }

    private Control_Animation theAnimation;
    private Level_Map levelMap;
    private AudioSource abilitySound;
    public Player_Display thePlayerDisp;
    public Game_Menu theControlPanel;

    // Use this for initialization
    void Start()
    {
    }

    public void Initialize()
    {
        theAnimation = GameObject.Find("Control Panel").GetComponent<Control_Animation>();
        thePlayerDisp = new Player_Display();
        theControlPanel = GameObject.Find("Game Menu Canvas").GetComponent<Game_Menu>();
        levelMap = GameObject.Find("Game Panel").GetComponent<Level_Map>();

        energyPointObject = GameObject.Find("EP Output").GetComponent<Text>();
        healthPointObject = GameObject.Find("HP Output").GetComponent<Text>();
        abilitySound = GameObject.Find("Ability Sound").GetComponent<AudioSource>();
    }

    public void PlayerMove(int direction)
    {
        if (!theAnimation.is_irresponsive && theControlPanel.MenuBtn.GetComponent<Button>().enabled)
        {
            theAnimation.is_irresponsive = true;
            thePlayerDisp.FaceTo = (FACETO)direction;
            TILE_TYPE goingTo = Move(direction);
            if (goingTo == TILE_TYPE.WALKABLE) // it is monster's turn only if player did change position
            {
                levelMap.theMonsters.MonstersTurn();
                StartCoroutine(theAnimation.PlayerMoveAnim());
                StartCoroutine(theAnimation.MonstersMoveAnim());
            }
            else if (goingTo == TILE_TYPE.FINISH_POINT)
            {
                levelMap.UpdateSaveLevel();
                theControlPanel.ToggleFinishMenu();
            }
        }
    }

    public void PlayerDoAbility()
    {
        if (!theAnimation.is_irresponsive && theControlPanel.MenuBtn.GetComponent<Button>().enabled)
        {
            if (DoAbility())
            {
                theAnimation.is_irresponsive = true;
                abilitySound.Play();
                StartCoroutine(levelMap.theAnimation.PlayerAbilityAnim()); 
                levelMap.theMonsters.MonstersTurn();
                StartCoroutine(levelMap.theAnimation.MonstersMoveAnim());
            }
        }
    }

    /* HANDEL REAL THING THERE */

    // retrun true: player did change position; return false: player didn't move
    private TILE_TYPE Move(int direction)
    {
        int newh = h + ((direction % 2 == 0) ? (direction - 1) : 0);
        int neww = w + (direction % 2 == 1 ? (direction - 2) : 0);
        if (healthPoint <= 0 || energyPoint <= 0)
            return TILE_TYPE.WALL;
        else if (levelMap.IsTileWalkable(newh, neww))
        {
            h = newh;
            w = neww;
            EP--;
            CD--;
            thePlayerDisp.animBeginPos = thePlayerDisp.ObjPos;
            thePlayerDisp.animEndPos = levelMap.MapCoordToWorldVec3(h, w, 0);
            return TILE_TYPE.WALKABLE;
        }
        else if (newh == levelMap.finishTile[0] && neww == levelMap.finishTile[1])
        {
            thePlayerDisp.animBeginPos = thePlayerDisp.ObjPos;
            thePlayerDisp.animEndPos = levelMap.MapCoordToWorldVec3(newh, neww, 0);
            return TILE_TYPE.FINISH_POINT;
        }
        return TILE_TYPE.WALL;       
    }

    // retrun true: player did do ability; return false: player couldn't do it
    private bool DoAbility()
    {
        if (abilityCooldown > 0)
            return false;
        CD = 1;
        EP--;
        return true;
    }

    /* after monster move animation end, we check if play is attacked */
    public bool IsPlayerAttacked()
    {
        // if player finish the map, monster can not fail it afterward.
        if (theControlPanel.isFinishMenu)
            return false;
        
        int loss = levelMap.theMonsters.TryAttackPlayer(h * levelMap.width + w);
        if (levelMap.theObstacles.positionList.Exists(x => x == h * levelMap.width + w))
        {
            levelMap.theObstacles.ObsDestroy(h * levelMap.width + w);
            loss++;
        }
        if (loss > 0)
        {
            // Debug.Log("player hurted");
            HP -= loss;
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
        if (!not_blocked && abilityCooldown > 0)
            theControlPanel.ToggleFailMenu();
    }

    /* SET VALUES */

    public void SetPositionTo(int newh, int neww)
    {
        if (newh < levelMap.height && neww < levelMap.width && newh >= 0 && neww >= 0)
        {
            if (levelMap.tiles[newh, neww] != TILE_TYPE.WALL)
            {
                levelMap.theObstacles.ObsDestroy(newh * levelMap.width + neww);
                h = newh;
                w = neww;
                thePlayerDisp.ObjPos = levelMap.MapCoordToWorldVec3(h, w, 0);
                return;
            }
        }
        Debug.Log("Player_Control.SetPositionTo(): illegal position");
    }
}
