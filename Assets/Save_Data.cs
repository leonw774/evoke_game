using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Save_Data {

    // the level code is ((theme_num) * 10 + (level_num)); so 23 is the third level of theme no.2;
    // theme 0 is reserved for test

    /*
     * GAME PROCESS SAVE
     * thing that will be get from a save file in future
     * */

    public static int levelProcess = 0; // when player fifnish the level, this variable++

    /*
     * GAME STATUS SAVE
     * record the status of the game
     * */
    
    public static string SelectedLevel = null;

    public static string SelectedTheme = "0"; // MapFileName = "map" + (level code)
}
