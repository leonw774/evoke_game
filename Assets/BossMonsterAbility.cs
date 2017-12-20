using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossMonsterAbility : MonoBehaviour {

    public SpriteRenderer sr_frame1, sr_frame2, sr_frame_hurt, sr_frame_abillity;
    public Text hpOutput;
    protected Monster self;
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
        get { return healthPoint; }
        set { if (value < FULL_HP) hpOutput.text = (healthPoint = value).ToString(); }
    }

    // BossMonster Object can only be added in Monsters()
    // in which they'll handle it like most of the monster
    // the function here is for its special ability

    public BossMonsterAbility(Level_Map lm, int _hp)
    {
        sr_frame1 = GameObject.Find("Boss Sprite Frame1").GetComponent<SpriteRenderer>();
        sr_frame2 = GameObject.Find("Boss Sprite Frame2").GetComponent<SpriteRenderer>();
        hpOutput = GameObject.Find("Boss HP Output").GetComponent<Text>();
        levelMap = lm;
        FULL_HP = _hp;
        healthPoint = _hp;
        hpOutput.text = healthPoint.ToString();
        decision = -1;
        self = null;
    }

    public void SetSelf(Monster _self)
    {
        self = _self;
    }

    virtual public int Decide()
    {
        Debug.Log("virtual public void Decide()");
        return -1;
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

    virtual public void DoSpecialMove()
    {
        Debug.Log("virtual public bool DoSpecialMove()");
    }
}
