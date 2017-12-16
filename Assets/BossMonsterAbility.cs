using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonsterAbility : MonoBehaviour {

    public SpriteRenderer sr_frame1, sr_frame2, sr_frame_hurt, sr_frame_abillity;
    public Monster self;
    protected Level_Map levelMap;

    public int abilityCooldown;
    public int attackCooldown;
    public int specailMoveCooldown;
    public int healthPoint;

    // BossMonster Object can only be added in Monsters()
    // in which they'll handle it like most of the monster
    // the function here is for its special ability

    public BossMonsterAbility(Level_Map lm, int hp)
    {
        sr_frame1 = GameObject.Find("Boss Sprite Frame1").GetComponent<SpriteRenderer>();
        sr_frame2 = GameObject.Find("Boss Sprite Frame2").GetComponent<SpriteRenderer>();
        levelMap = lm;
        healthPoint = hp;
    }

    virtual public int TryAttackPlayer(int playerPos)
    {
        Debug.Log("virtual public bool TryAttackPlayer(int)");
        return 0;
    }

    // check if it can and will do ability here
    virtual public bool TryDoAbility()
    {
        Debug.Log("virtual public bool TryDoAbility()");
        return false;
    }

    virtual public void DoAbility()
    {
        Debug.Log("virtual public void DoAbility()");
        return;
    }

    virtual public bool TrySpecialMove()
    {
        Debug.Log("virtual public bool TrySpecialMove()");
        return false;
    }

    public void SetAbilityCooldown(int cd)
    {
        if (cd > 0)
            abilityCooldown = cd;
        else
            Debug.Log("BossMonsterAbility SetAbilityCooldown Error");
    }
}
