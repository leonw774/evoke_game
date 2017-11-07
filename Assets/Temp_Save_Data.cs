using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Temp_Save_Data {
    /*
     * GAME PROCESS SAVE
     * thing that will read from/write to the save file when Menu_Script is loaded
     * */
    public static int levelPassed = 0; // when player fifnish the level, this variable++
	public static void UpdateLevel()
	{
		Temp_Save_Data.levelPassed++;
	}

    /*
     * GAME STATUS SAVE
     * record the status in any time of the game
     * */ 
    public static int SelectedLevel = -1;
    public static int SelectedTheme = -1;
    public static void SelectedNextLevel()
    {
        SelectedLevel++;
    }
}
