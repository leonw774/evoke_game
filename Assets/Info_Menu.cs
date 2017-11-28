using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Info_Menu : MonoBehaviour {

    GameObject MenuBtn;
    public bool isMenuActive = false;

	// Use this for initialization
	void Start () {
        MenuBtn = GameObject.Find("Menu Button");
        if (Save_Data.SelectedLevel == 0) // level zero is tutoriel level
        {
            toggleInfoMenu();
        }
	}

    public void toggleInfoMenu()
    {
        if (isMenuActive)
        {
            gameObject.transform.Translate(new Vector3(0.0f, 0.0f, -1.0f));
            MenuBtn.transform.Translate(new Vector3(0, 0, -1000));
            MenuBtn.GetComponent<Button>().enabled = true;
        }
        else
        {
            gameObject.transform.Translate(new Vector3(0.0f, 0.0f, 1.0f));
            MenuBtn.transform.Translate(new Vector3(0, 0, 1000));
            MenuBtn.GetComponent<Button>().enabled = false;
        }

        isMenuActive = !isMenuActive;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
