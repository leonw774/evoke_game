using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1Ability : BossMonsterAbility{

    private int cooldown;

    public Boss1Ability(Monster _self, Level_Map lm) : base(_self, lm)
    {
        cooldown = 0;
    }

    override public bool TryDoAbility()
    {
        int h_tocheck = self.h, w_tocheck = self.w;
        switch ((int)self.faceTo)
        {
            case 0: // up
                h_tocheck--; break;
            case 1: // left
                w_tocheck--; break;
            case 2: // down
                h_tocheck++; break;
            case 3: // right
                w_tocheck++; break;
        }

        if (levelMap.theObstacles.positionList.Exists(x => x == h_tocheck * levelMap.width + w_tocheck))
        {

        }

        return false;
    }

    override public void DoAbility()
    {
       
    }
}
