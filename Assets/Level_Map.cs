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

    public Obstacles theObstacles = null;
    public Monsters theMonsters = null;

    public Sprite minimap = null;
    public string mapFileName = null;
    private Color32[] mapPixels = null;


    // Use this for initialization
    void Start()
    {
        theObstacles = gameObject.AddComponent<Obstacles>();
        theMonsters = gameObject.AddComponent<Monsters>();
        if (Save_Data.SelectedLevel != -1)
        {
            GameInitial(Save_Data.SelectedLevel);
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
        GameConstruct();
    }

    public void GameConstruct()
    {
        // destroy previous walls
        DeleteWalls();
        theObstacles.Initialize();
        theMonsters.Initialize();

        if (ParseMapImg())
        {
            // if read img succese
            MapConstruction();
            FindEstimatedPath();
        }
        else
            Debug.Log("Level Read Map Failed.");
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
            Debug.Log("Image loaded! : " + height + ", " + width);
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
                    GameObject.Find("Player Sprite").transform.position = trans;
                    blocks[h, w] = (int)(BLOCK_TYPE.WALKABLE);
                }
            }
        }
        // construct obstacles
        theObstacles.Construct();

        // generate monsters
        theMonsters.GenerateMonsters(2);
    }

    public void FindEstimatedPath()
    {
        // use A-star to find least steps to finish
        Debug.Log("playerStartBlock: " + playerStartBlock[0] + "," + playerStartBlock[1]);
        Debug.Log("finishBlock: " + finishBlock[0] + "," + finishBlock[1]);
        Astar astar = new Astar(blocks, height, width, theObstacles.positionList, playerStartBlock, finishBlock);
        int estimateStep_ignore_obs = astar.FindPathLength(false);
        estimatedStep = astar.FindPathLength(true);
        Debug.Log(estimatedStep + " / " + estimateStep_ignore_obs);
        // add bonus steps
        int bonusLimit = (int)(width * height * 0.1);
        if (estimatedStep / 4 < bonusLimit)
            estimatedStep = estimatedStep * 5 / 4;
        else
            estimatedStep += bonusLimit;

        Text epo = GameObject.Find("EP Output").GetComponent<Text>();
        if (estimatedStep == -1)
            epo.text = "Error";
        else
            epo.text = estimatedStep.ToString();
    }

    public void GameRestart()
    {
        //Debug.Log("Restart");
        GameConstruct();
        theObstacles.Initialize();
        FindEstimatedPath();
    }

    public void GameNextLevel()
    {
        //Debug.Log("next level");
        Save_Data.SelectedNextLevel();
        GameInitial(Save_Data.SelectedLevel);
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