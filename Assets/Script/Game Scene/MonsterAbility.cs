using UnityEngine;
using UnityEngine.UI;

/*
 * NOTE
 * Is was planed to have general monsters implement in the same way boss monster did,
 * and we can specificate how many monster of each kind of them would spawn in SpawnMonsters(parameters)
 * then we won't have to check if id == -1 ? -> boss or general monster
 * we can just have a GeneralAbilty : MonsterAbility {desicion is always to move}
 * but we were running out of time, so
 * here leaves a imperfect structure 
 * */

public class BossAbility {

    public Sprite sp_frame1, sp_frame2;
    public SpriteRenderer facingSprite;
    public Text hpOutput;
    public bool killed;
    protected BossMonster self;
    protected Level_Map levelMap;

    public int decision;
    // decision:    -1 -> N/A
    //              0 -> move
    //              1 -> attack
    //              2 -> ability
    //              3 -> special move
    public readonly int FULL_HP;
    public int healthPoint;
    public int hp
    {
        get 
        {
            return healthPoint;
        }
        set 
        {
            if (value < FULL_HP) hpOutput.text = (healthPoint = value).ToString();
        }
    }

    // BossMonster Object can only be added in Monsters()
    // in which they'll handle it like most of the monster
    // the function here is for its special ability

    public BossAbility(Level_Map lm, int _hp)
    {
        facingSprite = null;
        hpOutput = null;
        levelMap = lm;
        FULL_HP = _hp;
        healthPoint = _hp;
        decision = -1;
        killed = false;
        self = null;
    }

    public void SetSelf(BossMonster _self)
    {
        self = _self;
    }

    virtual public int Decide()
    {
        Debug.Log("virtual public void Decide()");
        return -1;
    }

    virtual public int TryAttackPlayer()
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

    virtual public bool TrySpecialMove()
    {
        Debug.Log("virtual public bool TrySpecialMove()");
        return false;
    }

    virtual public void DoSpecialMove()
    {
        Debug.Log("virtual public bool DoSpecialMove()");
    }
}
