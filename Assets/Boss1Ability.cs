﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1_Ability : MonsterAbility {

    public Boss1_Ability(Level_Map lm, int _hp) : base(lm, _hp)
    {
    }

    override public int Decide()
    {
        int foundPos = levelMap.theObstacles.positionList.Find(x => x == self.h * levelMap.width + self.w);
        if (foundPos > 0) levelMap.theObstacles.ObsDestroy(foundPos);

        int distanceToPlayer = System.Math.Abs(levelMap.thePlayer.h - self.h) + System.Math.Abs(levelMap.thePlayer.w - self.w);

        // if player is next to it: attack
        if (distanceToPlayer == 1)
            return decision = 1;

        // if player not near and health point is low: Special Move
        /*
        if (distanceToPlayer < 12 && distanceToPlayer >= 6 && Random.Range(-1, FULL_HP - healthPoint + 1) > 0)
            return decision = 3;
        */

        // if player is near to it and they are seperated by some obstacles: ability
        return decision = (TryDoAbility() ? 2 : 0);
    }

    override public int TryAttackPlayer()
    {
        if (decision != 1) return 0;

        if (1 == (System.Math.Abs(levelMap.thePlayer.h - self.h) + System.Math.Abs(levelMap.thePlayer.w - self.w)))
        {
            if (levelMap.thePlayer.h < self.h)
                self.FaceTo(FACING.UP);
            else if (levelMap.thePlayer.h == self.h)
            {
                if (levelMap.thePlayer.w > self.w)
                    self.FaceTo(FACING.RIGHT);
                else
                    self.FaceTo(FACING.LEFT);
            }
            else
                self.FaceTo(FACING.DOWN);
            // returns the hp the player to be loss
            return 1;
        }
        return 0;
    }

    override public bool TryDoAbility()
    {
        Astar monAstar = new Astar(levelMap.tiles, levelMap.height, levelMap.width, levelMap.theObstacles.positionList,
                            new int[2] { self.h, self.w },
                            new int[2] { levelMap.thePlayer.h, levelMap.thePlayer.w });
        monAstar.FindPathLength(false, true, true);
        List<int> pathList = monAstar.GetPath();
        if (pathList.Count > 1)
            self.FaceTo((FACING) pathList[0]);

        //monAstar.PrintPath();
        //Debug.Log("self.FaceTo = " + (int) self.faceTo);

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
        //Debug.Log("boss TryDoAbility(): boss at " + self.h + ", " + self.w + "; try at " + h_tocheck + ", " + w_tocheck);
        return levelMap.theObstacles.positionList.Exists(x => x == h_tocheck * levelMap.width + w_tocheck);
    }

    override public void DoAbility()
    {
        int h_tocheck = self.h, w_tocheck = self.w;
        int lookat = -1; // side -1, middle 0, side 1
        Debug.Log("self.FaceTo = " + (int) self.faceTo);
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
        while (lookat <= 1)
        {
            if ((int)self.faceTo % 2 == 0)
                levelMap.theObstacles.ObsUpdate(levelMap.theObstacles.positionList.Find(x => x == h_tocheck * levelMap.width + (w_tocheck + lookat)));
            else
                levelMap.theObstacles.ObsUpdate(levelMap.theObstacles.positionList.Find(x => x == (h_tocheck + lookat) * levelMap.width + w_tocheck));
            lookat++;
        }
        Debug.Log("boss DoAbility(): face to " + self.faceTo);
    }

    public override void DoSpecialMove()
    {
        int goingTo = -1;
        Astar monAstar;
        List<int> pathList;

        monAstar = new Astar(levelMap.tiles, levelMap.height, levelMap.width, levelMap.theObstacles.positionList,
                            new int[2] { self.h, self.w },
                            new int[2] { levelMap.thePlayer.h, levelMap.thePlayer.w });
        monAstar.FindPathLength(false, true, true);
        pathList = monAstar.GetPath();
        if (pathList.Count > 0) goingTo = pathList[0];

        int newh = self.h, neww = self.w;
        if (goingTo == -1)
        {
            Debug.Log("boss DoSpecialMove() failed");
            return;
        }
        else if (pathList.Count > 3)
        {
            // move toward!
            switch (goingTo)
            {
                case 0: // up
                    newh--; break;
                case 1: // left
                    neww--; break;
                case 2: // down
                    newh++; break;
                case 3: // right
                    neww++; break;
            }
        }
        else
        {
            // move away!
            switch (goingTo)
            {
                case 0: // up
                    newh++; break;
                case 1: // left
                    neww++; break;
                case 2: // down
                    newh--; break;
                case 3: // right
                    neww--; break;
            }
        }

        if (levelMap.IsTileWalkable(newh, neww))
        {
            // no need to detect whether there is another mon on the way or not
            self.MoveTo(newh, neww);
            self.FaceTo((FACING)goingTo);
        }
    }
}
