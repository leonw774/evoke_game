using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class Menu_Script : MonoBehaviour {

	public StreamWriter SaveW = null;
	public StreamReader SaveR = null;
	public string SaveFilePath = null;

    public bool isTitleAnimPlaying = false;
    public SpriteRenderer titleImg;
    public SpriteRenderer titleImgIcon;
    public GameObject mainCam;

    public void Start()
    {
        mainCam = GameObject.Find("Main Camera");
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
                string tttest = GameObject.Find("ContinueBtn").GetComponentInChildren<Text>().text = "Begin";
                Debug.Log(tttest);
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
			if(lvlBtns[i].name.CompareTo("LevelBtn" + Temp_Save_Data.levelPassed.ToString()) <= 0)
            {
                lvlBtns[i].interactable = true;
            }
        }
    }

    public void AnimationStart()
    {
        isTitleAnimPlaying = true;
    }

    public void TitleAnimation()
    {
        Color toFadeInColor = titleImgIcon.color;
        Color toFadeOutColor = titleImg.color;
        if (toFadeInColor.a < 0.99f)
        {
            toFadeInColor.a = Mathf.Lerp(toFadeInColor.a, 1.0f, 4.0f * Time.deltaTime);
            toFadeOutColor.a = Mathf.Lerp(toFadeOutColor.a, 0.0f, 4.0f * Time.deltaTime);
            titleImgIcon.color = toFadeInColor;
            titleImg.color = toFadeOutColor;
        }
        else
        {
            isTitleAnimPlaying = false;
            CameraSwitch(true);
        }
    }

	public void CameraSwitch(bool isMaintoMenu)
	{
        if (isMaintoMenu)
            mainCam.transform.Translate(new Vector3(-20, 0, 0));
        else
            mainCam.transform.Translate(new Vector3(20, 0, 0));
        // set title logo back to transparent
        Color c = titleImg.color, ci = titleImgIcon.color;
        c.a = 1.0f;
        ci.a = 0.0f;
        titleImg.color = c;
        titleImgIcon.color = ci;
	}

    public void LoadLevel(int level)
    {
		Temp_Save_Data.SelectedLevel = level;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Menu Scene"));
        SceneManager.LoadScene("Game Scene");
    }

    void Update()
    {
        if (isTitleAnimPlaying)
            TitleAnimation();
    }
}