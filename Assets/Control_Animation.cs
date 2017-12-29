using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control_Animation : MonoBehaviour {

    private Player_Display thePlayerDisplay;
    private Level_Map levelMap;

    public float times_irreponsive = 0;
    public float times_monster_change_sprite = 0;
    public float times_boss_hurted_sprite = 0;
    public float times_player_hurted_sprite = 0;
    public readonly float ANIM_DUR_TIME = 0.225f;

    private bool moveAnimation = false;
    private bool playerHurtedAnimation = false;
    private bool bossMonsterHurtedAnimation = false;
    private bool monsters_ask_for_end = false, player_ask_for_end = false;

	// Use this for initialization
	void Start ()
    {
	}

    public void Initialize()
    {
        levelMap = GameObject.Find("Game Panel").GetComponent<Level_Map>();
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
        levelMap.thePlayer.thePlayerDisp.objPosition += (thePlayerDisplay.animEndPos - thePlayerDisplay.animBeginPos) / (Time.deltaTime / ANIM_DUR_TIME) * 0.0001f;
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
                    x.SpriteObj.transform.position += 
                        (x.animEndPos - x.animBeginPos) / (Time.deltaTime / ANIM_DUR_TIME) * 0.0001f;
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

    public void BossMonsterHurtedAnimStart()
    {
        bossMonsterHurtedAnimation = true;
        times_boss_hurted_sprite = Time.time + ANIM_DUR_TIME;
        GameObject.Find("Boss Attacked Effect").GetComponent<SpriteRenderer>().enabled = true;
    }

    private void BossMonsterHurtedAnimEnd()
    {
        bossMonsterHurtedAnimation = false;
        GameObject.Find("Boss Attacked Effect").GetComponent<SpriteRenderer>().enabled = false;
        if (levelMap.theMonsters.boss.monAbility.killed)
            levelMap.theMonsters.KillMonsterById(-1);
    }

    /*
    *** MAIN ANIMATION CONTROL ***
    */

    public void AnimSetup()
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
            levelMap.theMonsters.AllChangeFrame();
            times_monster_change_sprite += 1.1f;
        }

        if (playerHurtedAnimation && times_player_hurted_sprite <= Time.time)
        {
            PlayerHurtedAnimEnd();
        }

        if (bossMonsterHurtedAnimation && times_boss_hurted_sprite <= Time.time)
        {
            BossMonsterHurtedAnimEnd();
        }

        /* for testing on PC */
        if (Input.GetKey(KeyCode.UpArrow))
        {
            levelMap.thePlayer.playerMoveUp();
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            levelMap.thePlayer.playerMoveDown();
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            levelMap.thePlayer.playerMoveLeft();
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            levelMap.thePlayer.playerMoveRight();
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            levelMap.thePlayer.playerDoAbility();
        }
        /* for testing on PC */

        if (moveAnimation)
        {
            Anim();
        }
    }
}
