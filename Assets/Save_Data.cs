public static class Save_Data {
    
    /*
     * GAME FILE SAVE
     * thing that will read from/write to the save file when Menu_Script is loaded
     * */

    public static int levelPassed = -1; // when player fifnish the level, this variable++

	public static void UpdateLevel()
	{
		Save_Data.levelPassed++;
	}


    /*
     * GAME STATUS SAVE
     * record the status in any time of the game
     * */

    public static int SelectedLevel = -1;

    public static int SelectedTheme = -1;

    public static void SelectLevel(int newlevel)
    {
        SelectedLevel = newlevel;
        SetThemeSelectedLevel();
    }

    public static void SelectedNextLevel()
    {
        SelectedLevel++;
        SetThemeSelectedLevel();
    }

    public static void SetThemeSelectedLevel()
    {
        if (SelectedLevel < 9)
            SelectedTheme = 1;
        else if (SelectedLevel < 10)
            SelectedTheme = 2;
    }
}
