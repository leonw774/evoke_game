using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using B83.Image.BMP;
using UnityEngine.UI;
using System.Reflection;


public class Level_Map : MonoBehaviour
{
    public enum TILE_TYPE : int { WALKABLE = 0, WALL = 1, ITEM = 2, PLAYER_START_POINT = 3, FINISH_POINT = 4 };
    public int[,] tiles = null;
    public int[] playerStartTile = new int[2] { -1, -1 };
    public int[] finishTile = new int[2];
    public int width = 0, height = 0;
    public int estimatedStep = 0;
    public int wallsNumber = 0;
    public int monsterNumber = 0;

    public Obstacles theObstacles = null;
    public Monsters theMonsters = null;
    public Player_Control thePlayer = null;

    public string mapFileName = null;
    private Color32[] mapPixels = null;

    private bool introAnim = false;
    private SpriteRenderer introImage = null;
    private Sprite[] introSp = null;

    // Use this for initialization
    void Start()
    {
        theObstacles = gameObject.AddComponent<Obstacles>();
        theObstacles.Initialize();
        theMonsters = gameObject.AddComponent<Monsters>();
        theMonsters.Initialize();
        thePlayer = GameObject.Find("Player Control Canvas").GetComponent<Player_Control>();
        introImage = GameObject.Find("Intro Image").GetComponent<SpriteRenderer>();

        if (Save_Data.SelectedLevel != -1)
        {
            GameInitial(Save_Data.SelectedLevel);
            // if parse img succese
            if (ParseMapImg())
                GameStart();
            else
                Debug.Log("Level Read Map Failed.");
        }
        else
            Debug.Log("Selected Level Value Error!");
    }

    public void GameInitial(int thisMapLevel)
    {
        // initialize all the Resources which this level need
        mapFileName = "map" + thisMapLevel.ToString();
        Debug.Log("mapFileMap: " + mapFileName);
        // set up prototype sprites of themes
        LoadThemeSprites();
        // try show intro images
        IntroAnimStart();
        // at the same time, make maps
        LoadMapImg();
    }

    private void LoadThemeSprites()
    {
        Texture2D thisThemeTex;
        Rect Rect;
        Sprite Sp;

        // get backgrounds
        thisThemeTex = Resources.Load<Texture2D>("Themes/Background/background_" + Save_Data.SelectedTheme.ToString());
        Rect = new Rect(0.0f, 0.0f, (float)thisThemeTex.width, (float)thisThemeTex.height);
        Sp = Sprite.Create(thisThemeTex, Rect, new Vector2(0.5f, 0.5f));
        GameObject.Find("Field Background").GetComponent<SpriteRenderer>().sprite = Sp;

        thisThemeTex = Resources.Load<Texture2D>("Themes/Background/background_outring_" + Save_Data.SelectedTheme.ToString());
        Rect = new Rect(0.0f, 0.0f, (float)thisThemeTex.width, (float)thisThemeTex.height);
        Sp = Sprite.Create(thisThemeTex, Rect, new Vector2(0.5f, 0.5f));
        GameObject.Find("Field Frontground Outring").GetComponent<SpriteRenderer>().sprite = Sp;

        // get wall spaite
        thisThemeTex = Resources.Load<Texture2D>("Themes/Wall/wall_" + Save_Data.SelectedTheme.ToString());
        Rect = new Rect(0.0f, 0.0f, (float)thisThemeTex.width, (float)thisThemeTex.height);
        Sp = Sprite.Create(thisThemeTex, Rect, new Vector2(0.5f, 0.5f));
        GameObject.Find("Prototype Wall Sprite").GetComponent<SpriteRenderer>().sprite = Sp;

        // get obs sprite
        thisThemeTex = Resources.Load<Texture2D>("Themes/Obs/obs_" + Save_Data.SelectedTheme.ToString());
        Rect = new Rect(0.0f, 0.0f, (float)thisThemeTex.width, (float)thisThemeTex.height);
        Sp = Sprite.Create(thisThemeTex, Rect, new Vector2(0.5f, 0.5f));
        GameObject.Find("Prototype Obstacle Sprite").GetComponent<SpriteRenderer>().sprite = Sp;

        // get monster sprite
        thisThemeTex = Resources.Load<Texture2D>("Themes/Monster/monster_frame1_" + Save_Data.SelectedTheme.ToString());
        Rect = new Rect(0.0f, 0.0f, (float)thisThemeTex.width, (float)thisThemeTex.height);
        Sp = Sprite.Create(thisThemeTex, Rect, new Vector2(0.5f, 0.5f));
        GameObject.Find("Prototype Monster Sprite Frame 1").GetComponent<SpriteRenderer>().sprite = Sp;

        thisThemeTex = Resources.Load<Texture2D>("Themes/Monster/monster_frame2_" + Save_Data.SelectedTheme.ToString());
        Rect = new Rect(0.0f, 0.0f, (float)thisThemeTex.width, (float)thisThemeTex.height);
        Sp = Sprite.Create(thisThemeTex, Rect, new Vector2(0.5f, 0.5f));
        GameObject.Find("Prototype Monster Sprite Frame 2").GetComponent<SpriteRenderer>().sprite = Sp;
    }

    private void LoadMapImg()
    {
        // read img
        if (mapFileName != null)
        {
            Texture2D bmp = Resources.Load<Texture2D>(mapFileName);
            mapPixels = bmp.GetPixels32();
            tiles = new int[bmp.height, bmp.width];
            height = bmp.height;
            width = bmp.width;
            Debug.Log("Image loaded: " + height + ", " + width);

            // make mini map
            Rect minimapRect = new Rect(0.0f, 0.0f, (float)width, (float)height);
            Sprite minimapSp = Sprite.Create(bmp, minimapRect, new Vector2(0.5f, 0.5f));
            GameObject.Find("Mini Map").GetComponent<SpriteRenderer>().sprite = minimapSp;

            //Resources.UnloadAsset(bmp);
        }
        else
            Debug.Log("No file path!");
    }

    private bool ParseMapImg()
    {
        if (mapPixels == null) return false;
        // parse img
        // white for WALKABLE: 0
        // red for WALL: 1
        // green for FINISH: 2
        // blue for PLAYER_START_POINT: 3
        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                Color32 thisPixel = mapPixels[i * width + j];
                if (thisPixel.Equals(new Color32(255, 255, 255, 255)))
                    tiles[height - 1 - i, j] = (int)(TILE_TYPE.WALKABLE);
                else if (thisPixel.Equals(new Color32(0, 0, 0, 255)))
                    tiles[height - 1 - i, j] = (int)(TILE_TYPE.WALL);
                else if (thisPixel.Equals(new Color32(255, 0, 0, 255)))
                    tiles[height - 1 - i, j] = (int)(TILE_TYPE.FINISH_POINT);
                else if (thisPixel.Equals(new Color32(0, 0, 255, 255)))
                    tiles[height - 1 - i, j] = (int)(TILE_TYPE.PLAYER_START_POINT);
                else
                    return false;
                //Debug.Log(i + "," + j + ":" + tiles[height - 1 - i, j]);
                //Debug.Log(thisPixel.ToString());
            }
        }
        return true;
    }

    /* GAME START */

    // Don't call this function in GameRestart()
    public void MapFirstConstruction()
    {
        // delete previous walls
        DeleteWalls(); // where wallNumber is set to 0
        // make wall objects on map
        CreateWalls();
        //Debug.Log("playerStartTile: " + playerStartTile[0] + "," + playerStartTile[1]);
        //Debug.Log("finishTile: " + finishTile[0] + "," + finishTile[1]);
        SetMonsterNumber();
    }

    public void DeleteWalls()
    {
        GameObject[] wallsToDelete = GameObject.FindGameObjectsWithTag("Wall");
        for (int i = 0; i < wallsToDelete.Length; ++i)
        {
            //Debug.Log("Destroy a wall");
            Destroy(wallsToDelete[i]);
            wallsToDelete[i] = null;
        }
        wallsNumber = 0;
    }

    public void CreateWalls()
    {
        if (tiles.Length <= 1) return;
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                Vector3 trans = new Vector3((w - width / 2.0f + 0.5f), (height / 2.0f - h - 0.5f), 0);
                if (tiles[h, w] == (int)(TILE_TYPE.WALL))
                {
                    GameObject wallCreated;
                    wallCreated = Instantiate(GameObject.Find("Prototype Wall Sprite"));
                    wallCreated.name = "Wall Sprite";
                    wallCreated.tag = "Wall";
                    wallCreated.transform.parent = GameObject.Find("Game Panel").transform;
                    wallCreated.transform.position = trans;
                    wallsNumber++;
                }
                else if (tiles[h, w] == (int)(TILE_TYPE.FINISH_POINT))
                {
                    finishTile = new int[2] { h, w };
                    tiles[h, w] = (int)(TILE_TYPE.WALL);
                    GameObject.Find("Finish Sprite").transform.position = trans;
                }
                else if (tiles[h, w] == (int)(TILE_TYPE.PLAYER_START_POINT))
                {
                    playerStartTile = new int[2] { h, w };
                    tiles[h, w] = (int)(TILE_TYPE.WALKABLE);
                }
            }
        }
    }

    public void SetMonsterNumber()
    {
        switch (Save_Data.SelectedLevel)
        {
            case 0:
                monsterNumber = 2; break;
            case 1:
                monsterNumber = 4; break;
            case 2:
                monsterNumber = 8; break;
            default:
                monsterNumber = (tiles.Length - wallsNumber - 10) / 36 + ((Save_Data.SelectedLevel > 5) ? 3 : Save_Data.SelectedLevel - 2);
                break;
        }
        Debug.Log("the map ask for " + monsterNumber + " monsters");
    }

    public void SetPlayerInfo()
    {
        // use A-star to find least steps to finish
        Astar astar = new Astar(tiles, height, width, theObstacles.positionList, playerStartTile, finishTile);
        estimatedStep = astar.FindPathLength(true, false);
        Debug.Log("estimatedStep:" + estimatedStep);

        int emptyTilesNnum = height * width - wallsNumber;
        double monsterNumAdjust = 2.3;
        double diviedPathAdjustmant = ((int)(emptyTilesNnum / (estimatedStep * 4.3) * 10) / 10.0);
        Debug.Log("diviedPathAdjustmant: " + diviedPathAdjustmant);
        if (diviedPathAdjustmant > 1.0)
            monsterNumAdjust /= diviedPathAdjustmant;
        Debug.Log("monsterNumAdjust: " + monsterNumAdjust);

        thePlayer.Initialize();
        thePlayer.SetEnergyPoint((int)(estimatedStep * 1.15) + (int)(monsterNumber * monsterNumAdjust));
        thePlayer.SetHealthPoint(2 + monsterNumber / 10);
        thePlayer.SetAbilityCooldown(0);
        thePlayer.SetFaceTo(Player_Control.FACING.FRONT);
        thePlayer.SetPositionTo(playerStartTile[0], playerStartTile[1]);
    }

    public void GameStart()
    {
        // only do MapConstruct in first start
        MapFirstConstruction();
        // construct obstacles
        theObstacles.Construct();
        // generate monsters
        if(monsterNumber > 0)
            theMonsters.SpawnMonsters(monsterNumber);
        SetPlayerInfo();
    }

    public void GameRestart()
    {
        Debug.Log("Restart");
        theObstacles.DestroyAllObstacles();
        theMonsters.DestroyMonsters();
        theObstacles.Construct();
        if(monsterNumber > 0)
            theMonsters.SpawnMonsters(monsterNumber);
        SetPlayerInfo();
    }

    // this function is called by Player_Control when it find out player is at finish
    public void GameFinish()
    {
        if (Save_Data.SelectedLevel == Save_Data.levelPassed + 1)
        {
            Save_Data.UpdateLevel();
        }
        Debug.Log("SelectedLevel: " + Save_Data.SelectedLevel);
        Debug.Log("levelPassed: " + Save_Data.levelPassed);
    }

    public void GameNextLevel()
    {
        //Debug.Log("next level");
        theObstacles.DestroyAllObstacles();
        theMonsters.DestroyMonsters();
       
        Save_Data.SelectedNextLevel();
        GameInitial(Save_Data.SelectedLevel);
        // if parse img succese
        if (ParseMapImg())
            GameStart();
        else
            Debug.Log("Level Read Map Failed.");
    }


    /* INTRO ANIM */

    private void IntroAnimStart()
    {
        Texture2D[] introTxs = Resources.LoadAll<Texture2D>("Intro_map" + Save_Data.SelectedLevel.ToString());
        if (introTxs.Length == 0)
            return;

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

    private void IntroAnimEnd()
    {
        intro_image_num = -1;
        introAnim = false;
        introImage.enabled = false;
        introSp = null;
        time_change_intro_image = 0.0f;
    }

    float time_change_intro_image = 0.0f;
    int intro_image_num = -1;
    void Update()
    {
        if (introAnim)
        {
            if (time_change_intro_image <= Time.time)
            {
                if (intro_image_num < introSp.Length && intro_image_num >= 0)
                {
                    Debug.Log("show intro image #" + intro_image_num + "at time of " + Time.time);
                    introImage.sprite = introSp[intro_image_num];
                    intro_image_num++;
                    time_change_intro_image += 2.0f;
                }
                else
                {
                    IntroAnimEnd();
                }
            }
        }
    }
}