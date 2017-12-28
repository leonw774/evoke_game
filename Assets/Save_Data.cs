public static class Save_Data {
    
    /*
     * GAME FILE SAVE
     * thing that will read from/write to the save file when Menu_Script is loaded
     * */

    public static int PassedLevel = -1; // when player fifnish the level, this variable++

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

    public readonly static int BossLevel = 11;

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
        if (SelectedLevel < 12)
            SelectedTheme = 1;
        else
            SelectedTheme = 2;
    }
}
