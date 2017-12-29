using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Intro_Slides : MonoBehaviour {

    private bool introAnim = false;
    private SpriteRenderer introImage = null;
    private Sprite[] introSp = null;
    Texture2D[] introTxs;

    float time_change_intro_image = 0.0f;
    int intro_image_num = -1;

	// Use this for initialization
	void Start () {
        introImage = GameObject.Find("Intro Image").GetComponent<SpriteRenderer>();
        introTxs = Resources.LoadAll<Texture2D>("Plot_" + Save_Data.SelectedLevel.ToString());
        if (introTxs.Length > 0)
        {
            IntroAnimStart();
        }
	}
	
    /* INTRO ANIM */

    private void IntroAnimStart()
    {
        // button for switch picture
        GameObject.Find("Intro Image Switch Button").GetComponent<Image>().raycastTarget = true;
        GameObject.Find("Intro Image Switch Button").GetComponent<Button>().interactable = true;

        introSp = new Sprite[introTxs.Length];
        introImage.enabled = true;
        introAnim = true;
        intro_image_num = 0;
        time_change_intro_image = Time.time;

        for (int i = 0; i < introTxs.Length; i++)
        {
            Rect introTxRect = new Rect(0.0f, 0.0f, introTxs[i].width, introTxs[i].height);
            introSp[i] = Sprite.Create(introTxs[i], introTxRect, new Vector2(0.5f, 0.5f));
        }
    }

    public void IntroAnimSwitchPicture()
    {
        if (intro_image_num < introSp.Length && intro_image_num >= 0)
        {
            Debug.Log("show intro image #" + intro_image_num + "at time of " + Time.time);
            introImage.sprite = introSp[intro_image_num];
            intro_image_num++;
            time_change_intro_image = Time.time + 2.0f ;
        }
        else
        {
            IntroAnimEnd();
        }
    }

    private void IntroAnimEnd()
    {
        // button for switch picture
        GameObject.Find("Intro Image Switch Button").GetComponent<Button>().interactable = false;
        GameObject.Find("Intro Image Switch Button").GetComponent<Image>().raycastTarget = false;
        intro_image_num = -1;
        introAnim = false;
        introImage.enabled = false;
        introSp = null;
        time_change_intro_image = 0.0f;

        SceneManager.LoadScene("Menu Scene");

    }

    void Update()
    {
        if (introAnim)
        {
            if (time_change_intro_image <= Time.time)
            {
                IntroAnimSwitchPicture();
            }
        }
    }
}
