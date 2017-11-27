using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Info_Menu : MonoBehaviour {

    GameObject MenuBtn;
    //GameObject InfoBtn;
    GameObject infoCanvas;
    public bool isMenuActive = false;

	// Use this for initialization
	void Start () {
        infoCanvas = GameObject.Find("Info Menu Canvas");
        MenuBtn = GameObject.Find("Menu Button");
        //InfoBtn = GameObject.Find("Info Button");

        if (Save_Data.SelectedLevel == 0) // level zero is tutoriel level
        {
            toggleInfoMenu();
        }
	}

    public void toggleInfoMenu()
    {
        if (isMenuActive)
        {
            gameObject.transform.Translate(new Vector3(0.0f, 0.0f, -2.0f));
            MenuBtn.transform.Translate(new Vector3(0, 0, -1000));
            MenuBtn.GetComponent<Button>().enabled = true;
        }
        else
        {
            gameObject.transform.Translate(new Vector3(0.0f, 0.0f, 2.0f));
            MenuBtn.transform.Translate(new Vector3(0, 0, 1000));
            MenuBtn.GetComponent<Button>().enabled = false;
        }

        isMenuActive = !isMenuActive;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
