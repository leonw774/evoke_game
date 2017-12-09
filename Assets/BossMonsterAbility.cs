using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonsterAbility : MonoBehaviour {

    public SpriteRenderer sr_frame1, sr_frame2;
    protected Monster self;

    // BossMonster Object can only be added in Monsters()
    // in which they'll handle it like most of the monster
    // the function here is for its special ability

    public BossMonsterAbility(Monster _self)
    {
        sr_frame1 = GameObject.Find("Boss Sprite Frame1").GetComponent<SpriteRenderer>();
        sr_frame2 = GameObject.Find("Boss Sprite Frame2").GetComponent<SpriteRenderer>();
        self = _self;
    }
    
    virtual public void DoAbility()
    {

    }
}
