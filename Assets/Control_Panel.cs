using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Control_Panel : MonoBehaviour {

    GameObject menuCanvas;
    GameObject FinishGO;
    GameObject FailGO;
    GameObject MenuBtn;
    GameObject ResumeBtn;
    public bool isMenuActive = false;
    public bool isFinishMenu = false, isFailMenu = false;
	// Use this for initialization
	void Start () {
        menuCanvas = GameObject.Find("Game Menu Canvas");
        FinishGO = GameObject.Find("Finish Objects");
        FailGO = GameObject.Find("Fail Objects");
        MenuBtn = GameObject.Find("Menu Button");
        ResumeBtn = GameObject.Find("Resume Button");
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public void toggleGameMenu()
    {
        if (isMenuActive)
        {
            menuCanvas.transform.Translate(new Vector3(0.0f, 0.0f, -2.0f));
            MenuBtn.transform.Translate(new Vector3(0, 0, -1000));
        }
        else
        {
            menuCanvas.transform.Translate(new Vector3(0.0f, 0.0f, 2.0f));
            MenuBtn.transform.Translate(new Vector3(0, 0, 1000));
        }
        isMenuActive = !isMenuActive;

        if (isFailMenu)
        {
            FailGO.transform.Translate(new Vector3(0, 0, -1000));
            ResumeBtn.transform.Translate(new Vector3(0, 0, -1000));
            isFailMenu = false;
        }
        if (isFinishMenu)
        {
            FinishGO.transform.Translate(new Vector3(0, 0, -1000));
            ResumeBtn.transform.Translate(new Vector3(0, 0, -1000));
            isFinishMenu = false;
        }
    }

    public void toggleFinishMenu()
    {
        toggleGameMenu();
        FinishGO.transform.Translate(new Vector3(0, 0, 1000));
        ResumeBtn.transform.Translate(new Vector3(0, 0, 1000));
        isFinishMenu = true;
    }

    public void toggleFailMenu()
    {
        toggleGameMenu();
        FailGO.transform.Translate(new Vector3(0, 0, 1000));
        ResumeBtn.transform.Translate(new Vector3(0, 0, 1000));
        isFailMenu = true;
    }

    public void gameExitButton()
    {
        SceneManager.LoadScene("Menu Scene");
        //Application.Quit();
    }

}
