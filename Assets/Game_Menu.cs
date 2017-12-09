using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

public class Game_Menu : MonoBehaviour {

    public StreamReader SaveR = null;
    public StreamWriter SaveW = null;

    GameObject menuCanvas;
    GameObject FinishGO;
    GameObject FailGO;
    GameObject MenuBtn;
    GameObject InfoBtn;
    GameObject ResumeBtn;
    public bool isMenuActive = false;
    public bool isFinishMenu = false, isFailMenu = false;

	// Use this for initialization
	void Start ()
    {
        menuCanvas = GameObject.Find("Game Menu Canvas");
        FinishGO = GameObject.Find("Finish Objects");
        FailGO = GameObject.Find("Fail Objects");
        MenuBtn = GameObject.Find("Menu Button");
        InfoBtn = GameObject.Find("Info Button");
        ResumeBtn = GameObject.Find("Resume Button");
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isMenuActive)
                toggleGameMenu();
            else
                gameExitButton();
        }
    }

    public void toggleGameMenu()
    {
        if (isMenuActive)
        {
            menuCanvas.transform.Translate(new Vector3(0.0f, 0.0f, -1.0f));
            MenuBtn.transform.Translate(new Vector3(0, 0, -1000));
            InfoBtn.transform.Translate(new Vector3(0, 0, -1000));
            MenuBtn.GetComponent<Button>().enabled = true;
            InfoBtn.GetComponent<Button>().enabled = true;
        }
        else
        {
            menuCanvas.transform.Translate(new Vector3(0.0f, 0.0f, 1.0f));
            MenuBtn.transform.Translate(new Vector3(0, 0, 1000));
            InfoBtn.transform.Translate(new Vector3(0, 0, 1000));
            MenuBtn.GetComponent<Button>().enabled = false;
            InfoBtn.GetComponent<Button>().enabled = false;
        }
        isMenuActive = !isMenuActive;

        if (isFailMenu)
        {
            FailGO.transform.Translate(new Vector3(0, 0, -1000));
            ResumeBtn.transform.Translate(new Vector3(0, 0, -999.9f));
            isFailMenu = false;
        }
        if (isFinishMenu)
        {
            FinishGO.transform.Translate(new Vector3(0, 0, -1000));
            ResumeBtn.transform.Translate(new Vector3(0, 0, -999.9f));
            isFinishMenu = false;
        }
    }

    public void toggleFinishMenu()
    {
        toggleGameMenu();
        if (!isFinishMenu)
        {
            FinishGO.transform.Translate(new Vector3(0, 0, 1000));
            ResumeBtn.transform.Translate(new Vector3(0, 0, 1000));
            isFinishMenu = true;
        }
        writeSaveData(Application.persistentDataPath + "/save.dat", Save_Data.levelPassed);
    }

    public void toggleFailMenu()
    {
        toggleGameMenu();
        if (!isFailMenu)
        {
            FailGO.transform.Translate(new Vector3(0, 0, 1000));
            ResumeBtn.transform.Translate(new Vector3(0, 0, 1000));
            isFailMenu = true;
        }
    }

    public void gameExitButton()
    {
        writeSaveData(Application.persistentDataPath + "/save.dat", Save_Data.levelPassed);
        SceneManager.LoadScene("Menu Scene");
        //Application.Quit();
    }

    void loadSaveData(string SaveFilePath)
    {
        SaveR = new StreamReader(SaveFilePath);
        Save_Data.levelPassed = int.Parse(SaveR.ReadLine());
        SaveR.Close();
    }

    void writeSaveData(string SaveFilePath, int level)
    {
        if (File.Exists(SaveFilePath))
        {
            SaveW = new StreamWriter(SaveFilePath, false);
            SaveW.WriteLine(level.ToString() + "\n");
            SaveW.Close();
        }
    }
}
