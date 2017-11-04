using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu_Script : MonoBehaviour {

    public void Start()
    {
        // set Save_Data to the right value
        if(Save_Data.levelProcess == 0)
        {
            // read value in save file
            Save_Data.levelProcess = 2;
            // Save_Data will get its value from a save file in future
        }
        else
        {
            Save_Data.levelProcess = 1;
        }
        Button[] lvlBtns = FindObjectsOfType<Button>();
        for(int i = 0; i < lvlBtns.Length; ++i)
        {
            if(lvlBtns[i].name.CompareTo("MenuBtn" + Save_Data.levelProcess) <= 0)
            {
                lvlBtns[i].interactable = true;
            }
        }
    }

    public void LoadLevel(string level)
    {
        Save_Data.SelectedLevel = "map" + level;
        Save_Data.levelProcess = int.Parse(level);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Menu Scene"));
        SceneManager.LoadScene("Game Scene");
    }
}
