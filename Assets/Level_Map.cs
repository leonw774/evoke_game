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
        // initialize map
        mapFileName = "map" + thisMapLevel.ToString();
        Debug.Log("mapFileMap: " + mapFileName);
        // show intro images
        IntroAnimStart();
        // at the same time, make maps
        LoadMapImg();
        makeMiniMap();
    }

    private void makeMiniMap()
    {
        // load mini map
        Texture2D minimapTx = Resources.Load<Texture2D>(mapFileName);
        Rect minimapRect = new Rect(0.0f, 0.0f, (float)width, (float)height);
        Sprite minimapSp = Sprite.Create(minimapTx, minimapRect, new Vector2(0.5f, 0.5f));
        GameObject.Find("Mini Map").GetComponent<SpriteRenderer>().sprite = minimapSp;
    }

    public void LoadMapImg()
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
            Resources.UnloadAsset(bmp);
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

    public void MapConstruction()
    {
        // delete previous walls
        DeleteWalls();

        // make objects on map
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

        Debug.Log("playerStartTile: " + playerStartTile[0] + "," + playerStartTile[1]);
        Debug.Log("finishTile: " + finishTile[0] + "," + finishTile[1]);

        // set monster number
        wallsNumber = 0;
        switch (Save_Data.SelectedLevel)
        {
            case 0:
                monsterNumber = 2;
                break;
            case 1:
                monsterNumber = 0;
                break;
            case 2:
                monsterNumber = 5;
                break;
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
        // add bonus steps
        /*
        int bonusLimit = (int)(width * height * 0.1);
        if ((int)(estimatedStep / 6) < bonusLimit)
            estimatedStep += (int)(estimatedStep / 6);
        else
            estimatedStep += bonusLimit;
        */
        int emptyTilesNnum = height * width - wallsNumber;
        double MonsterNumAdjust = 2.4;
        Debug.Log("MonsterNumAdjust /= " + (int)(emptyTilesNnum / (estimatedStep * 4.2)));
        MonsterNumAdjust /= (int)(emptyTilesNnum / (estimatedStep * 4.2));
        thePlayer.Initialize();
        thePlayer.SetEnergyPoint((int)(estimatedStep * 1.2) + (int)(monsterNumber * MonsterNumAdjust));
        thePlayer.SetHealthPoint(3 + monsterNumber / 20);
        thePlayer.SetAbilityCooldown(0);
        thePlayer.SetFaceTo(Player_Control.FACING.FRONT);
        thePlayer.SetPositionTo(playerStartTile[0], playerStartTile[1]);
    }

    public void GameStart()
    {
        // only do MapConstruct in first start
        MapConstruction();
        // construct obstacles
        theObstacles.Construct();
        //Debug.Break();
        // generate monsters
        if(monsterNumber > 0)
            theMonsters.Generate(monsterNumber);
        SetPlayerInfo();
    }

    public void GameRestart()
    {
        Debug.Log("Restart");
        theObstacles.DestroyAllObstacles();
        theMonsters.DestroyMonsters();
        theObstacles.Construct();
        if(monsterNumber > 0)
            theMonsters.Generate(monsterNumber);
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

    public void DeleteWalls()
    {
        GameObject[] wallsToDelete = GameObject.FindGameObjectsWithTag("Wall");
        for (int i = 0; i < wallsToDelete.Length; ++i)
        {
            //Debug.Log("Destroy a wall");
            Destroy(wallsToDelete[i]);
            wallsToDelete[i] = null;
        }
    }

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