using UnityEngine;
using System.Collections;

public class Control_Panel : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public void gameExitButton()
    {
        Application.Quit();
    }
}
