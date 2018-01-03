using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Security.Policy;

public class Level_Menu : MonoBehaviour {

	public string SaveFilePath = null;
    public StreamReader SaveR = null;
    public StreamWriter SaveW = null;
    public bool isTitleAnimPlaying = false;
    public SpriteRenderer titleImg;
    public SpriteRenderer titleImgIcon;
    public GameObject mainCam;

    public void Start()
    {
        mainCam = GameObject.Find("Main Camera");
        titleImg = GameObject.Find("Main Title").GetComponent<SpriteRenderer>();
        titleImgIcon = GameObject.Find("Main Title Icon").GetComponent<SpriteRenderer>();
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
                GameObject.Find("Continue Button Text").GetComponent<Text>().text = "Begin";
                // Passedlevel is still -1 until player finish level 0
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
    }

    void SetupLevelMenuButton()
    {
        Button[] lvlBtns = FindObjectsOfType<Button>();
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
                lvlBtns[i].interactable = true;
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

    private void FirstTimeInGameIntro()
    {
        Save_Data.SelectLevel(0);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Menu Scene"));
        SceneManager.LoadScene("Slide Scene");
    }

    public void LoadIntroSlide(int num)
    {
        GameObject.Find("Loading Menu").GetComponent<SpriteRenderer>().enabled = true;
        Save_Data.SelectLevel(num);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Menu Scene"));
        SceneManager.LoadScene("Slide Scene");
    }

    public void LoadContinueLevel()
    {
        GameObject.Find("Loading Title").GetComponent<SpriteRenderer>().enabled = true;
        Save_Data.SelectLevel(Save_Data.PassedLevel + ((Save_Data.PassedLevel == Save_Data.BossLevel) ? 0 : 1));
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

    public void AnimationStart()
    {
        if (Save_Data.PassedLevel == -1 && Save_Data.SelectedLevel == -1)
            FirstTimeInGameIntro();
        else
            isTitleAnimPlaying = true;
    }

    public void CameraMain2Menu()
    {
        // move camera
        mainCam.transform.Translate(new Vector3(-20, 0, 0));
        // set title logo back to transparent
        Color c = titleImg.color, ci = titleImgIcon.color;
        c.a = 1.0f;
        ci.a = 0.0f;
        titleImg.color = c;
        titleImgIcon.color = ci;
    }

    public void CameraMenu2Main()
    {
        mainCam.transform.Translate(new Vector3(20, 0, 0));
    }

    public void TitleAnimation()
    {
        Color toFadeInColor = titleImgIcon.color;
        Color toFadeOutColor = titleImg.color;
        if (toFadeInColor.a < 0.2f)
        {
            toFadeInColor.a = Mathf.Lerp(toFadeInColor.a, 1.0f, 3.0f * Time.deltaTime);
            toFadeOutColor.a = Mathf.Lerp(toFadeOutColor.a, 0.0f, 3.0f * Time.deltaTime);
            titleImgIcon.color = toFadeInColor;
            titleImg.color = toFadeOutColor;
        }
        else if (toFadeInColor.a < 0.98f)
        {
            toFadeInColor.a = Mathf.Lerp(toFadeInColor.a, 1.0f, 4.0f * Time.deltaTime);
            toFadeOutColor.a = Mathf.Lerp(toFadeOutColor.a, 0.0f, 4.0f * Time.deltaTime);
            titleImgIcon.color = toFadeInColor;
            titleImg.color = toFadeOutColor;
        }
        else
        {
            isTitleAnimPlaying = false;
            CameraMain2Menu();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        if (isTitleAnimPlaying)
            TitleAnimation();
    }
}