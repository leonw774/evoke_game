using UnityEngine;
using TileTypeDefine;
using System.Security.AccessControl;
using System;
using UnityEngine.UI;

namespace TileTypeDefine
{
    public enum TILE_TYPE : int { WALKABLE = 0, WALL = 1, ITEM = 2, PLAYER_START_POINT = 3, FINISH_POINT = 4, SHORTCUT = 5 };
}

public class Level_Map : MonoBehaviour
{
    public TILE_TYPE[,] tiles = null;
    public int[] playerStartTile = new int[2] { -1, -1 };
    public int[] finishTile = new int[2];
    public int width = 0, height = 0;
    public int estimatedStep = 0;
    public int wallsNumber = 0;
    public int monsterNumber = 0;

    public Obstacles theObstacles = null;
    public Monsters_Control theMonsters = null;
    public Player_Control thePlayer = null;
    public Control_Animation theAnimation = null;

    public string mapFileName = null;
    private Color32[] mapPixels = null;

    // Use this for initialization
    void Start()
    {
        theObstacles = gameObject.AddComponent<Obstacles>();
        theObstacles.Initialize();

        theMonsters = gameObject.AddComponent<Monsters_Control>();
        theMonsters.Initialize();

        thePlayer = GameObject.Find("Player Control Canvas").GetComponent<Player_Control>();
        thePlayer.Initialize();

        theAnimation = GameObject.Find("Control Panel").GetComponent<Control_Animation>();
        theAnimation.Initialize();

        GameObject.Find("Error Msg").transform.position = new Vector3(0.0f, -2.0f, -10.0f);

        // start a level from the very beginning
        GameInitial();
    }

    public void GameInitial()
    {
        if (Save_Data.SelectedLevel != -1)
        {
            GameLoad(Save_Data.SelectedLevel);
            // if parse img succese
            if (ParseMapImg())
                GameStart();
            else
                GameObject.Find("Error Msg").transform.position = new Vector3(0.0f, -2.0f, 0.0f);
        }
        else
            GameObject.Find("Error Msg").transform.position = new Vector3(0.0f, -2.0f, 0.0f);
        //Debug.Log("Selected Level Value Error!");
    }

    public void GameLoad(int thisMapLevel)
    {
        // initialize all the Resources which this level need
        mapFileName = "map" + thisMapLevel.ToString();
        // Debug.Log("mapFileMap: " + mapFileName);
        LoadThemeSprites();
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

        // get outring
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
            tiles = new TILE_TYPE[bmp.height, bmp.width];
            height = bmp.height;
            width = bmp.width;
            //Debug.Log("Image loaded: " + height + ", " + width);
        }
        else
            Debug.Log("No file path!");
    }

    private bool ParseMapImg()
    {
        if (mapPixels == null) return false;
        // parse img
        // white for WALKABLE: 0
        // black for WALL: 1
        // red for FINISH: 2
        // blue for PLAYER_START_POINT: 3
        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                Color32 thisPixel = mapPixels[i * width + j];
                //Debug.Log(i + "," + j + ":" + thisPixel.ToString());
                if (thisPixel.r > 247 && thisPixel.g > 247 && thisPixel.b > 247)
                    tiles[height - 1 - i, j] = TILE_TYPE.WALKABLE;
                else if (thisPixel.Equals(new Color32(255, 0, 0, 255)))
                    tiles[height - 1 - i, j] = TILE_TYPE.FINISH_POINT;
                else if (thisPixel.Equals(new Color32(0, 0, 255, 255)))
                    tiles[height - 1 - i, j] = TILE_TYPE.PLAYER_START_POINT;
                else if (thisPixel.Equals(new Color32(0, 0, 0, 255)))
                    tiles[height - 1 - i, j] = TILE_TYPE.WALL;
                else
                    return false;
                //Debug.Log(":" + tiles[height - 1 - i, j]);
            }
        }
        return true;
    }

    /* GAME START */

    // Don't call this function in GameRestart()
    public void MapFirstConstruction()
    {
        // delete previous walls; wallNumber will set to 0
        DeleteWalls(); 
        // make wall objects on map
        CreateWalls();
        //Debug.Log("playerStartTile: " + playerStartTile[0] + "," + playerStartTile[1]);
        //Debug.Log("finishTile: " + finishTile[0] + "," + finishTile[1]);
        SetMonsterNumber();
    }

    private void DeleteWalls()
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

    private void CreateWalls()
    {
        GameObject wallCreated = null;
        GameObject exitObj = null;
        if (tiles.Length <= 1) return;
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                Vector3 trans = MapCoordToWorldVec3(h, w, 0);
                if (tiles[h, w] == TILE_TYPE.WALL)
                {
                    wallCreated = null;
                    wallCreated = Instantiate(GameObject.Find("Prototype Wall Sprite"));
                    wallCreated.name = "Wall Sprite";
                    wallCreated.tag = "Wall";
                    wallCreated.transform.parent = GameObject.Find("Game Panel").transform;
                    wallCreated.transform.position = trans;
                    wallsNumber++;
                }
                else if (tiles[h, w] == TILE_TYPE.FINISH_POINT)
                {
                    finishTile = new int[2] { h, w };
                    tiles[h, w] = TILE_TYPE.WALL;
                    exitObj = GameObject.Find("Exit Sprite");
                    exitObj.transform.position = trans;

                    // in boss level, exit is closed
                    if (Save_Data.SelectedLevel == Save_Data.BossLevel)
                    {
                        exitObj.GetComponent<SpriteRenderer>().enabled = false;
                        exitObj = GameObject.Find("Closed Exit Sprite");
                        exitObj.transform.position = trans;
                        exitObj.GetComponent<SpriteRenderer>().enabled = true;
                    }
                }
                else if (tiles[h, w] == TILE_TYPE.PLAYER_START_POINT)
                {
                    playerStartTile = new int[2] { h, w };
                    tiles[h, w] = TILE_TYPE.WALKABLE;
                }
            }
        }
    }

    private void SetMonsterNumber()
    {
        if (Save_Data.SelectedLevel < 3)
            monsterNumber = 2 * (Save_Data.SelectedLevel + 1);
        else if (Save_Data.SelectedLevel == 3)
            monsterNumber = 10;
        else
            monsterNumber = (tiles.Length - wallsNumber - 8) / 32 + ((Save_Data.SelectedLevel > 4) ? 4 : Save_Data.SelectedLevel);
        //Debug.Log("map ask for " + monsterNumber + " mons");
    }

    private void SetPlayerInfo()
    {
        // use A-star to find least steps to finish
        Astar astar = new Astar(tiles, height, width, theObstacles.positionList, playerStartTile, finishTile);
        estimatedStep = astar.FindPathLength(false, true, false);

        //astar.PrintPath();
        //Debug.Log("estimatedStep:" + estimatedStep);

        int walkableTilesNnum = tiles.Length - wallsNumber;
        float adjustedmonsterNum = monsterNumber;
        float monsterNumToStep = 3.6f;
        float multiPathFactor = ((int)(walkableTilesNnum / (estimatedStep * 3.6f) * 100) / 100f);

        walkableTilesNnum += (int) (theObstacles.positionList.Count / ((multiPathFactor > 1f) ? multiPathFactor : 1f));
        multiPathFactor = (int)(walkableTilesNnum / (estimatedStep * 3.6f) * 100) / 100f;
        adjustedmonsterNum /= multiPathFactor;
        Debug.Log("multiPathFactor: " + multiPathFactor);
        //Debug.Log("adjustedmonsterNum: " + adjustedmonsterNum);

        int ep_to_set = (int) (estimatedStep * (1.36 - ((Save_Data.SelectedLevel / 3) * 0.03))) + (int) (monsterNumToStep * adjustedmonsterNum) + 1;
        int hp_to_set = (int) adjustedmonsterNum / 15 + 2;

        if (Save_Data.SelectedLevel == Save_Data.BossLevel)
        {
            ep_to_set += (int) (monsterNumToStep * adjustedmonsterNum + 0.5);
            hp_to_set += 3;
        }

        thePlayer.SetEnergyPoint(ep_to_set);
        thePlayer.SetHealthPoint(hp_to_set);
        thePlayer.SetAbilityCooldown(0);
        thePlayer.thePlayerDisp.ChangeFacingSpriteTo(CHARACTER_FACING.FRONT);
        if (theAnimation.isViewMapMode)
            thePlayer.thePlayerDisp.playerFacingSprite.enabled = false;
        thePlayer.SetPositionTo(playerStartTile[0], playerStartTile[1]);
        //Debug.Log("Player States All Set");
    }

    private void GameStart()
    {
        // only do MapConstruct in first start
        MapFirstConstruction();
        theObstacles.Construct();
        if(monsterNumber > 0)
            theMonsters.SpawnMonsters(monsterNumber);
        SetPlayerInfo();
    }

    /* called by retry button */
    public void GameRestart()
    {
        //Debug.Log("Restart");
        theObstacles.DestroyAllObstacles();
        theMonsters.DestroyAllMonsters();
        theObstacles.Construct();
        if(monsterNumber > 0)
            theMonsters.SpawnMonsters(monsterNumber);
        SetPlayerInfo();
    }

    /* this function is called by Player_Control when it find out player is at finish */
    public void UpdateSaveLevel()
    {
        if (Save_Data.SelectedLevel != Save_Data.BossLevel && Save_Data.SelectedLevel == Save_Data.PassedLevel + 1)
            Save_Data.UpdatePassedLevel();
        //Debug.Log("SelectedLevel: " + Save_Data.SelectedLevel);
        //Debug.Log("PassedLevel: " + Save_Data.PassedLevel);
    }

    public void GameNextLevel()
    {
        //Debug.Log("clean for next level");
        theObstacles.DestroyAllObstacles();
        theMonsters.DestroyAllMonsters();
        Save_Data.SelectedNextLevel();
        // start a level from the very beginning
        GameInitial();
    }

    /* SOME USEFUL FUNTION */

    public Vector3 MapCoordToWorldVec3(int h, int w, int z_value)
    {
        return new Vector3((w - width / 2.0f + 0.5f), (height / 2.0f - h - 0.5f), (float)z_value);
    }

    public bool IsTileWalkable(int i, int j)
    {
        if (theMonsters.boss != null)
            return !(theObstacles.positionList.Exists(x => x == i * width + j) || tiles[i, j] == TILE_TYPE.WALL || (theMonsters.boss.h == i && theMonsters.boss.w == j));
        return !(theObstacles.positionList.Exists(x => x == i * width + j) || tiles[i, j] == TILE_TYPE.WALL);
    }
}