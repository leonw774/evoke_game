using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using System;

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

		SaveFilePath = Application.persistentDataPath + "/save.dat";

		// if it is the the first to the main menu
        if (Save_Data.levelPassed == -1)
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
                string cbt = GameObject.Find("Continue Button Text").GetComponent<Text>().text;
                cbt = "Begin";
                // leave it unloaded
            }
        }
        // if the game has been played over some level
        else
        {
            saveFileDebugOutput.text = "false\n";
            if (File.Exists(SaveFilePath))
            {
                saveFileDebugOutput.text += "true\n";
                writeSaveData(Save_Data.levelPassed);
            }
            else // and there is not a save file yet!
            {
                saveFileDebugOutput.text += "false\n";
                writeSaveData(Save_Data.levelPassed);
            }
        }
		
        saveFileDebugOutput.text += Save_Data.levelPassed;
	    //Save_Data.levelPassed = 2;

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
            if (bnum <= (Save_Data.levelPassed + 1))
            {
                lvlBtns[i].interactable = true;
            }
        }
    }

    void loadSaveData()
    {
        SaveR = new StreamReader(SaveFilePath);
        Save_Data.levelPassed = int.Parse(SaveR.ReadLine());
        SaveR.Close();
    }

    void writeSaveData(int level)
    {
        if (File.Exists(SaveFilePath))
        {
            SaveW = new StreamWriter(SaveFilePath, false);
            SaveW.WriteLine(level.ToString() + "\n");
            SaveW.Close();
        }
    }

    public void LoadLevel(int level)
    {
        Save_Data.SelectedLevel = level;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Menu Scene"));
        SceneManager.LoadScene("Game Scene");
    }

    public void AnimationStart()
    {
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
            toFadeInColor.a = Mathf.Lerp(toFadeInColor.a, 1.0f, 2.4f * Time.deltaTime);
            toFadeOutColor.a = Mathf.Lerp(toFadeOutColor.a, 0.0f, 2.4f * Time.deltaTime);
            titleImgIcon.color = toFadeInColor;
            titleImg.color = toFadeOutColor;
        }
        else if (toFadeInColor.a < 0.96f)
        {
            toFadeInColor.a = Mathf.Lerp(toFadeInColor.a, 1.0f, 3.2f * Time.deltaTime);
            toFadeOutColor.a = Mathf.Lerp(toFadeOutColor.a, 0.0f, 3.2f * Time.deltaTime);
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
        if (isTitleAnimPlaying)
            TitleAnimation();
    }
}