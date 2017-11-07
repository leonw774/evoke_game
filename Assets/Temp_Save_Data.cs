using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Temp_Save_Data {

    /*
     * GAME PROCESS SAVE
     * thing that will read from/write to the save file when Menu_Script is loaded or Update
     * */
	// the level code is ((theme_num) * 10 + (level_num)); so 23 is the third level of theme no.2;
	// theme 0 is reserved for test
    public static int levelPassed = 0; // when player fifnish the level, this variable++
	public static void UpdateLevel()
	{
		Temp_Save_Data.levelPassed += ((levelPassed % 10 == 3) ? 7 : 1);
	}

    /*
     * GAME STATUS SAVE
     * record the status in any time of the game
     * */
    
    public static string SelectedLevel = null;

    public static string SelectedTheme = "0"; // MapFileName = "map" + (level code)
}
