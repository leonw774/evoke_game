using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonsterAbility : MonoBehaviour {

    public SpriteRenderer sr_frame1, sr_frame2;
    protected Monster self;
    protected Level_Map levelMap;

    // BossMonster Object can only be added in Monsters()
    // in which they'll handle it like most of the monster
    // the function here is for its special ability

    public BossMonsterAbility(Monster _self, Level_Map lm)
    {
        sr_frame1 = GameObject.Find("Boss Sprite Frame1").GetComponent<SpriteRenderer>();
        sr_frame2 = GameObject.Find("Boss Sprite Frame2").GetComponent<SpriteRenderer>();
        self = _self;
        levelMap = lm;
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
}
