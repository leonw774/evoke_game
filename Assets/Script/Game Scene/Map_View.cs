using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map_View : MonoBehaviour {

    GameObject MenuBtn;
    GameObject InfoBtn;
    GameObject Game_Panel;
    Text MapBtnTxt;
    public bool isMenuActive = false;

	// Use this for initialization
	void Start () {
        MenuBtn = GameObject.Find("Menu Button");
        InfoBtn = GameObject.Find("Info Button");
        Game_Panel = GameObject.Find("Game Panel");
        MapBtnTxt = GameObject.Find("Map Button Text").GetComponent<Text>();
	}

    public void toggleMapMenu()
    {
        /*
        if (isMenuActive)
        {
            gameObject.transform.Translate(new Vector3(0.0f, 0.0f, -1.0f));
            MenuBtn.transform.Translate(new Vector3(0, 0, -1000));
            InfoBtn.transform.Translate(new Vector3(0, 0, -1000));
            MapBtnTxt.text = "Show Map";
            MenuBtn.GetComponent<Button>().enabled = true;
        }
        else
        {
            gameObject.transform.Translate(new Vector3(0.0f, 0.0f, 1.0f));
            MenuBtn.transform.Translate(new Vector3(0, 0, 1000));
            InfoBtn.transform.Translate(new Vector3(0, 0, 1000));
            MapBtnTxt.text = "Hide Map";
            MenuBtn.GetComponent<Button>().enabled = false;
        }
        isMenuActive = !isMenuActive;
        */
    }
}
