using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Control_Panel : MonoBehaviour {

    GameObject menuCanvas;
    bool menuBtn_active = false;
	// Use this for initialization
	void Start () {
        menuCanvas = GameObject.Find("Game Menu Canvas");
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public void gameMenuButton()
    {
        if (menuCanvas.transform.position.z == -9.0f)
        {
            menuCanvas.transform.Translate(new Vector3(0.0f, 0.0f, -2.0f));
            menuBtn_active = false;
        }
        else
        {
            menuCanvas.transform.Translate(new Vector3(0.0f, 0.0f, 2.0f));
            menuBtn_active = true;
        }
    }

    public void gameExitButton()
    {
        SceneManager.LoadScene("Menu Scene");
        //Application.Quit();
    }

}
