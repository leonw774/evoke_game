using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;

public class Menu_Scene : MonoBehaviour {

	public string SaveFilePath = null;
    public StreamReader SaveR = null;
    public StreamWriter SaveW = null;
    public bool isTitleAnimPlaying = false;
    public GameObject mainCam;
    public GameObject titleObj;
    public SpriteRenderer titleLogo;
    public SpriteRenderer titleImg;

    public void Start()
    {
        mainCam = GameObject.Find("Main Camera");
        titleObj = GameObject.Find("Main Title");
        titleLogo = GameObject.Find("Main Title").GetComponent<SpriteRenderer>();
        titleImg = GameObject.Find("Main Title Player Sprite").GetComponent<SpriteRenderer>();
        Text saveFileDebugOutput = GameObject.Find("Save File Debug Output").GetComponent<Text>();

		SaveFilePath = Application.persistentDataPath + "/save.txt";

		// if it is the the first to the main menu
        if (Save_Data.PassedLevel == -1)
        {
            saveFileDebugOutput.text = "true\n";
            if (File.Exists(SaveFilePath))
            {
                saveFileDebugOutput.text += "true\n";
                loadSaveData();
            }
            else
            {
                saveFileDebugOutput.text += "false\n";
                GameObject.Find("Continue Button Text").GetComponent<Text>().text = "開始教學關";
                // Passed level is still -1 until player finish level 0
                if (Save_Data.SelectedLevel != -1)
                {
                    CameraMain2Menu();
                }
            }
        }
        // if the game has been played over some level
        else
        {
            CameraMain2Menu();
            saveFileDebugOutput.text = "false\n";
            //saveFileDebugOutput.text += File.Exists(SaveFilePath).ToString() + "\n";
            writeSaveData(Save_Data.PassedLevel);
        }
		
        saveFileDebugOutput.text += Save_Data.PassedLevel;
	    //Save_Data.PassedLevel = 2;

        SetupLevelMenuButton();
        TitleAnimationStart();
    }

    void SetupLevelMenuButton()
    {
        GameObject[] lvlBtns = GameObject.FindGameObjectsWithTag("Level Button");
        int bnum;
        for(int i = 0; i < lvlBtns.Length; ++i)
        {
            try
            {
                bnum = int.Parse(lvlBtns[i].name.Substring(8, 2));
            }
            catch(FormatException)
            {
                continue;
            }
            if (bnum <= (Save_Data.PassedLevel + 1))
            {
                lvlBtns[i].GetComponent<Button>().interactable = true;
            }
            else if (lvlBtns[i].name.Substring(0, 5) == "Level")
            {
                lvlBtns[i].GetComponent<Image>().sprite = null;
            }
        }
    }

    void loadSaveData()
    {
        SaveR = new StreamReader(SaveFilePath);
        Save_Data.PassedLevel = int.Parse(SaveR.ReadLine());
        SaveR.Close();
    }

    void writeSaveData(int level)
    {
        SaveW = new StreamWriter(SaveFilePath, false);
        if (File.Exists(SaveFilePath))
            SaveW.WriteLine(level.ToString() + "\n");
        SaveW.Close();
    }

    /* JUMP TO OTHER SCENE */
    /*
    public void LoadIntroSlide(int num)
    {
        GameObject.Find("Loading Menu").GetComponent<SpriteRenderer>().enabled = true;
        Save_Data.SelectLevel(num);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Menu Scene"));
        SceneManager.LoadScene("Slide Scene");
    }
    */
    public void LoadContinueLevel()
    {
        GameObject.Find("Loading Title").GetComponent<SpriteRenderer>().enabled = true;
        Save_Data.SelectLevel(Save_Data.PassedLevel + ((Save_Data.PassedLevel == Save_Data.MaxLevel) ? 0 : 1));
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Menu Scene"));
        SceneManager.LoadScene("Game Scene");
    }

    public void LoadLevel(int level)
    {
        GameObject.Find("Loading Menu").GetComponent<SpriteRenderer>().enabled = true;
        Save_Data.SelectLevel(level);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Menu Scene"));
        SceneManager.LoadScene("Game Scene");
    }

    /* ANIMATION */ 

    public void TitleAnimationStart()
    {
        titleLogo.color = Color.clear;
        titleImg.color = Color.clear;
        isTitleAnimPlaying = true;
    }

    public void CameraMain2Menu()
    {
        // move camera
        mainCam.transform.Translate(new Vector3(-20, 0, 0));
    }

    public void CameraMenu2Main()
    {
        mainCam.transform.Translate(new Vector3(20, 0, 0));
    }

    public void TitleAnimation()
    {
        if (titleLogo.color.a < 0.99)
        {
            titleImg.color = titleLogo.color * (1 - (Time.deltaTime / 0.5f)) + Color.white * (Time.deltaTime / 0.5f);
            titleLogo.color = titleLogo.color * (1 - (Time.deltaTime / 0.5f)) + Color.white * (Time.deltaTime / 0.5f);
        }
        int cycleNum = (int) (Time.time / 2.5);
        if (cycleNum % 2 == 0)
            titleObj.transform.position += new Vector3(0f, 0.06f * Time.deltaTime, 0f);
        else
            titleObj.transform.position += new Vector3(0f, -0.06f * Time.deltaTime, 0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        if (isTitleAnimPlaying)
            TitleAnimation();
    }
}