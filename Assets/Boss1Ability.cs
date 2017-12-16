using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1_Ability : BossMonsterAbility {

    public Boss1_Ability(Level_Map lm, int hp) : base(lm, hp)
    {
    }

    override public int TryAttackPlayer(int playerPos)
    {
        if (attackCooldown > 0) return 0;

        // returns the hp the player to be loss
        if (1 == (System.Math.Abs(levelMap.thePlayer.h - self.h) + System.Math.Abs(levelMap.thePlayer.w - self.w)))
        {
            if (levelMap.playerStartTile[0] - self.h == 1)
                self.faceTo = FACING.UP;
            else if (levelMap.playerStartTile[0] - self.h == 0)
            {
                if (levelMap.playerStartTile[1] - self.w == 1)
                    self.faceTo = FACING.RIGHT;
                else
                    self.faceTo = FACING.LEFT;
            }
            else
                self.faceTo = FACING.DOWN;

            attackCooldown = 1;
            return 1;
        }
        return 0;
    }

    override public bool TryDoAbility()
    {
        int foundPos = levelMap.theObstacles.positionList.Find(x => x == self.h * levelMap.width + self.w);
        if (foundPos > 0)
            levelMap.theObstacles.ObsDestroy(foundPos);

        // if player is next to it: attack
        if (1 == (System.Math.Abs(levelMap.thePlayer.h - self.h) + System.Math.Abs(levelMap.thePlayer.w - self.w)))
        {
            // don't do ability but also don't move
            return true;
        }

        // if player is no next to it and they are seperated by some obstacles
        // do ability and don't move
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
        foundPos = levelMap.theObstacles.positionList.Find(x => x == h_tocheck * levelMap.width + w_tocheck);
        if (foundPos > 0)
        {
            levelMap.theObstacles.ObsDestroy(foundPos);
            return true;
        }
        attackCooldown = 1;
        return false;
    }

    override public void DoAbility()
    {

    }
}
