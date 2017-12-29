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
    GameObject MapBtn;
    GameObject ResumeBtn;
    AudioSource FinishSound;
    AudioSource FailSound;

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
        MapBtn = GameObject.Find("Map Button");
        ResumeBtn = GameObject.Find("Resume Button");
        FailSound = GameObject.Find("Fail Sound").GetComponent<AudioSource>();
        FinishSound = GameObject.Find("Finish Sound").GetComponent<AudioSource>();
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
            MapBtn.transform.Translate(new Vector3(0, 0, -1000));
            MenuBtn.GetComponent<Button>().enabled = true;
            InfoBtn.GetComponent<Button>().enabled = true;
        }
        else
        {
            menuCanvas.transform.Translate(new Vector3(0.0f, 0.0f, 1.0f));
            MenuBtn.transform.Translate(new Vector3(0, 0, 1000));
            InfoBtn.transform.Translate(new Vector3(0, 0, 1000));
            MapBtn.transform.Translate(new Vector3(0, 0, 1000));
            MenuBtn.GetComponent<Button>().enabled = false;
            InfoBtn.GetComponent<Button>().enabled = false;
        }
        isMenuActive = !isMenuActive;

        if (isFailMenu)
        {
            FailSound.Pause();
            FailGO.transform.Translate(new Vector3(0, 0, -1000));
            ResumeBtn.transform.Translate(new Vector3(0, 0, -999.999f));
            isFailMenu = false;
        }
        if (isFinishMenu)
        {
            FinishSound.Pause();
            FinishGO.transform.Translate(new Vector3(0, 0, -1000));
            ResumeBtn.transform.Translate(new Vector3(0, 0, -999.999f));
            isFinishMenu = false;
        }
    }

    public void toggleFinishMenu()
    {
        FinishSound.Play();
        toggleGameMenu();
        if (!isFinishMenu)
        {
            if (Save_Data.SelectedLevel != Save_Data.BossLevel)
            {
                FinishGO.transform.Translate(new Vector3(0, 0, 1000));
            }
            ResumeBtn.transform.Translate(new Vector3(0, 0, 1000));
            isFinishMenu = true;
        }
        writeSaveData(Application.persistentDataPath + "/save.dat", Save_Data.PassedLevel);
    }

    public void toggleFailMenu()
    {
        FailSound.Play();
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
        writeSaveData(Application.persistentDataPath + "/save.dat", Save_Data.PassedLevel);
        SceneManager.LoadScene("Menu Scene");
    }

    void loadSaveData(string SaveFilePath)
    {
        SaveR = new StreamReader(SaveFilePath);
        Save_Data.PassedLevel = int.Parse(SaveR.ReadLine());
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
