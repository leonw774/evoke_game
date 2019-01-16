public static class Save_Data {
    
    /*
     * GAME FILE SAVE
     * thing that will read from/write to the save file when Menu_Script is loaded
     * */

    public static int PassedLevel = -1; // when player finish this level, this variable++
    public readonly static int MaxLevel = 10;

    public static void UpdatePassedLevel()
	{
		Save_Data.PassedLevel++;
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
        if (SelectedLevel < MaxLevel)
            SelectedTheme = 1;
        else
            SelectedTheme = 2;
    }
}
