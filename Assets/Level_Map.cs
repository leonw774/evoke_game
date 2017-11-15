using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using B83.Image.BMP;
using UnityEngine.UI;
using System.Reflection;


public class Level_Map : MonoBehaviour
{
    public enum BLOCK_TYPE : int { WALKABLE = 0, WALL = 1, ITEM = 2, PLAYER_START_POINT = 3, FINISH_POINT = 4 };
    public int[,] blocks = null;
    public int[] playerStartBlock = new int[2] { -1, -1 };
    public int[] finishBlock = new int[2];
    public int width = 0, height = 0;
    public int estimatedStep = 0;
    public int wallsNumber = 0;

    public Obstacles theObstacles = null;
    public Monsters theMonsters = null;
    public Player_Control thePlayer = null;

    public Sprite minimap = null;
    public string mapFileName = null;
    private Color32[] mapPixels = null;

    // Use this for initialization
    void Start()
    {
        theObstacles = gameObject.AddComponent<Obstacles>();
        theObstacles.Initialize();
        theMonsters = gameObject.AddComponent<Monsters>();
        theMonsters.Initialize();
        thePlayer = GameObject.Find("Player Control Canvas").GetComponent<Player_Control>();
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

    public void GameInitial(int newMapLevel)
    {
        // initialize map
        mapFileName = "map" + newMapLevel.ToString();
        Debug.Log("mapFileMap: " + mapFileName);
        minimap = Resources.Load<Sprite>(mapFileName);
        LoadMapImg();
    }

    public void GameStart()
    {
        // only do MapConstruct in first start
        MapConstruction();

        // construct obstacles
        theObstacles.Construct();
        // generate monsters
        theMonsters.Generate(4);
        SetPlayerInfo();
    }

    public void LoadMapImg()
    {
        // read img
        if (mapFileName != null)
        {
            Texture2D bmp = Resources.Load<Texture2D>(mapFileName);
            SpriteRenderer mpsr = GameObject.Find("Mini Map").GetComponent<SpriteRenderer>();
            mpsr.sprite = minimap;
            mapPixels = bmp.GetPixels32();
            blocks = new int[bmp.height, bmp.width];
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
                    blocks[height - 1 - i, j] = (int)(BLOCK_TYPE.WALKABLE);
                else if (thisPixel.Equals(new Color32(0, 0, 0, 255)))
                    blocks[height - 1 - i, j] = (int)(BLOCK_TYPE.WALL);
                else if (thisPixel.Equals(new Color32(255, 0, 0, 255)))
                    blocks[height - 1 - i, j] = (int)(BLOCK_TYPE.FINISH_POINT);
                else if (thisPixel.Equals(new Color32(0, 0, 255, 255)))
                    blocks[height - 1 - i, j] = (int)(BLOCK_TYPE.PLAYER_START_POINT);
                else
                    return false;
                //Debug.Log(i + "," + j + ":" + blocks[height - 1 - i, j]);
                //Debug.Log(thisPixel.ToString());
            }
        }
        return true;
    }

    public void MapConstruction()
    {
        // delete previous walls
        DeleteWalls();
        wallsNumber = 0;
        // make images on map
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                Vector3 trans = new Vector3((w - width / 2.0f + 0.5f), (height / 2.0f - h - 0.5f), 0);
                if (blocks[h, w] == (int)(BLOCK_TYPE.WALL))
                {
                    GameObject wallCreated;
                    wallCreated = Instantiate(GameObject.Find("Prototype Wall Sprite"));
                    wallCreated.name = "Wall Sprite";
                    wallCreated.tag = "Wall";
                    wallCreated.transform.parent = GameObject.Find("Game Panel").transform;
                    wallCreated.transform.position = trans;
                    wallsNumber++;
                }
                else if (blocks[h, w] == (int)(BLOCK_TYPE.FINISH_POINT))
                {
                    finishBlock = new int[2] { h, w };
                    blocks[h, w] = (int)(BLOCK_TYPE.WALL);
                    GameObject.Find("Finish Sprite").transform.position = trans;
                }
                else if (blocks[h, w] == (int)(BLOCK_TYPE.PLAYER_START_POINT))
                {
                    playerStartBlock = new int[2] { h, w };
                    blocks[h, w] = (int)(BLOCK_TYPE.WALKABLE);
                }
            }
        }
    }

    public void SetPlayerInfo()
    {
        // use A-star to find least steps to finish
        Debug.Log("playerStartBlock: " + playerStartBlock[0] + "," + playerStartBlock[1]);
        Debug.Log("finishBlock: " + finishBlock[0] + "," + finishBlock[1]);
        Astar astar = new Astar(blocks, height, width, theObstacles.positionList, playerStartBlock, finishBlock);
        estimatedStep = astar.FindPathLength(true);
        Debug.Log("estimatedStep:" + estimatedStep);
        // add bonus steps
        int bonusLimit = (int)(width * height * 0.1);
        if (estimatedStep / 4 < bonusLimit)
            estimatedStep = estimatedStep * 5 / 4;
        else
            estimatedStep += bonusLimit;
        
        thePlayer.Initialize();
        thePlayer.SetEnergyPoint(estimatedStep);
        thePlayer.SetPositionTo(playerStartBlock[0], playerStartBlock[1]);
    }

    public void GameRestart()
    {
        //Debug.Log("Restart");
        theObstacles.DestroyObstacles();
        theMonsters.DestroyMonsters();
        theObstacles.Construct();
        theMonsters.Generate(4);
        SetPlayerInfo();
    }

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
        theObstacles.DestroyObstacles();
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
            Debug.Log("Destroy a wall");
            Destroy(wallsToDelete[i]);
            wallsToDelete[i] = null;
        }
    }
}