using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class Menu_Script : MonoBehaviour {

	public StreamWriter SaveW = null;
	public StreamReader SaveR = null;
	public string SaveFilePath = null;

    public bool titleAnimationPlay = false;
    public SpriteRenderer titleImg;
    public SpriteRenderer titleImgIcon;
    public void Start()
    {
		
        titleImg = GameObject.Find("Main Title").GetComponent<SpriteRenderer>();
        titleImgIcon = GameObject.Find("Main Title Icon").GetComponent<SpriteRenderer>();

		string SaveFilePath = Application.persistentDataPath + "/save.dat";

		// if save file not read yet
		if (Temp_Save_Data.levelPassed == 0)
		{
			if (File.Exists(SaveFilePath))
			{
                SaveR = new StreamReader(SaveFilePath);
				Temp_Save_Data.levelPassed = int.Parse(SaveR.ReadLine());
                SaveR.Close();
			}
			else
			{
                SaveW = new StreamWriter(SaveFilePath, true);
				SaveW.WriteLine("0\n");
                SaveW.Close();
                GameObject.Find("ContinueBtn").GetComponent<Text>().text = "Begin";
			}
		}
		
		/*
		if (Temp_Save_Data.levelPassed == 0)
		{
			// to unlock level for debug
			Temp_Save_Data.levelPassed = 2;
		}
		*/
        Button[] lvlBtns = FindObjectsOfType<Button>();
        for(int i = 0; i < lvlBtns.Length; ++i)
        {
			if(lvlBtns[i].name.CompareTo("MenuBtn" + Temp_Save_Data.levelPassed.ToString()) <= 0)
            {
                lvlBtns[i].interactable = true;
            }
        }
    }

    public void AnimationStart()
    {
        titleAnimationPlay = true;
    }

    public void TitleAnimation()
    {
        Color toFadeInColor = titleImgIcon.color;
        if (toFadeInColor.a < 0.99f)
        {
            toFadeInColor.a = Mathf.Lerp(toFadeInColor.a, 1.0f, 4.0f * Time.deltaTime);
            titleImgIcon.color = toFadeInColor;
        }
        else
        {
            titleAnimationPlay = false;
            CameraSwitch();
        }
    }

	public void CameraSwitch()
	{
		Camera mainCam = GameObject.Find("Main Camera").GetComponent<Camera>(), menuCam = GameObject.Find("Menu Camera").GetComponent<Camera>();
		bool t;
		t = mainCam.enabled;
		mainCam.enabled = menuCam.enabled;
		menuCam.enabled = t;
        Color c = titleImgIcon.color;
        c.a = 0.0f;
        titleImgIcon.color = c;
	}

    public void LoadLevel(int level)
    {
		Temp_Save_Data.SelectedLevel = level;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Menu Scene"));
        SceneManager.LoadScene("Game Scene");
    }

    void Update()
    {
        if (titleAnimationPlay)
            TitleAnimation();
    }
}