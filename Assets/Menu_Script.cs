using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_Script : MonoBehaviour {

    public string SelectedLevel = null;
    public string SelectedTheme = "0";
    // filename = "map" + SelectedTheme + "_" + SelectedLevel
    // but when in debug, set SelectedLevel as complete file name

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void setSelectedLevel(string level)
    {
        SelectedLevel = level;
    }

	public void LoadLevel()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Menu Scene"));
        SceneManager.LoadScene("Game Scene");
    }
}
