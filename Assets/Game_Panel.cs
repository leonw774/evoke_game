using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using B83.Image.BMP;
using UnityEngine.UI;


public class Game_Panel : MonoBehaviour
{
    public class Map
    {
        public enum BLOCK_TYPE : int { WALKABLE = 0, WALL = 1, FINISH_POINT = 2, PLAYER_START_POINT = 3 };
        public int[,] blocks;
        public int width = 0;
        public int height = 0;

        public Obstacles theObstacles; // store obstacle position in as integer(h * width + w)
        public int[] playerStartBlock = new int[2];
        public int[] finishBlock = new int[2];

        public int estimatedStep = 0;

        public string mapFileName = null;
        private Color32[] mapPixels = null;

        public Map()
        {
            // initialize without map_img
            theObstacles = null;
            playerStartBlock = new int[2] { -1, -1 };
        }

        public Map(string newMapFileName)
        {
            mapFileName = newMapFileName;
            LoadMapImg();
            if (ParseMapImg())
            {
                // if read img succese
                Initialize();
                FindEstimatedPath();
            }
        }

        public void LoadMapImg()
        {
            // read img
            if (mapFileName != null)
            {
                Texture2D bmp = Resources.Load<Texture2D>(mapFileName);
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
                    else if (thisPixel.r > 0)
                        blocks[height - 1 - i, j] = (int)(BLOCK_TYPE.WALL);
                    else if (thisPixel.g > 0)
                        blocks[height - 1 - i, j] = (int)(BLOCK_TYPE.FINISH_POINT);
                    else if (thisPixel.b > 0)
                        blocks[height - 1 - i, j] = (int)(BLOCK_TYPE.PLAYER_START_POINT);
                    else
                        return false;
                    //Debug.Log(i + "," + j + ":" + blocks[height - 1 - i, j]);
                    //Debug.Log(thisPixel.ToString());
                }
            }
            return true;
        }

        public void Initialize()
        {
            // destroy previous walls
            GameObject wallsToDelete;
            while (wallsToDelete = GameObject.Find("Wall Sprite"))
            {
                Debug.Log("Destroy a wall");
                Destroy(wallsToDelete);
                wallsToDelete = null;
            }

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

            // initialize obstacles
            theObstacles = new Obstacles(this);
            theObstacles.Initialize();
        }

        public void FindEstimatedPath()
        {
            // use A-star to find least steps to finish
            Astar astar = new Astar(blocks, height, width, theObstacles.positionList, playerStartBlock, finishBlock);
            int estimateStep_ignore_obs = astar.FindPathLength(false);
            estimatedStep = astar.FindPathLength(true);
            Debug.Log(estimatedStep + " / " + estimateStep_ignore_obs);
            // add bonus steps
            int bonusLimit = (int) (width * height * 0.1);
            if (estimatedStep / 4 < bonusLimit)
                estimatedStep = estimatedStep * 5 / 4;
            else
                estimatedStep += bonusLimit;

            Text sro = GameObject.Find("Remaining Steps Output").GetComponent<Text>();
            if (estimatedStep == -1)
                sro.text = "Error";
            else
                sro.text = estimatedStep.ToString();
        }
    }

    public class Obstacles
    {
        public List<int> positionList;
        public Map parentMap;

        public Obstacles()
        {
            parentMap = null;
            positionList = null;
        }

        public Obstacles(Map theMapObstaclesWithIn)
        {
            parentMap = theMapObstaclesWithIn;
        }

        public void Initialize()
        {
            positionList = null;
            positionList = new List<int>();
            positionList.Clear();
            DestroyObstacles(); // deatroy previous obstacles
            Generate();
            Adjust();
            Debug.Log("There are " + positionList.Count + " obs in map");
        }

        public void Update(int pos)
        {
            Update(pos / parentMap.width, pos % parentMap.width);
        }

        public void Update(int h, int w)
        {
            int pos = h * parentMap.width + w;
            // check if there is already a obstacle on this block
            if (positionList.IndexOf(pos) != -1)
            {
                //Debug.Log("obs destruction happened");
                GameObject[] obs = GameObject.FindGameObjectsWithTag("Obstacle");
                for (int k = 0; k < obs.Length; ++k)
                {
                    if (obs[k].name == "Obstacle Sprite" + pos.ToString())
                    {
                        Destroy(obs[k]);
                        obs[k] = null;
                    }
                }
                positionList.Remove(pos);
            }
            // if there is not obs & block is walkable then
            else if (parentMap.blocks[h, w] == 0)
            {
                // add
                // Debug.Log("obs creation happened");
                Vector3 trans = new Vector3((w - parentMap.width / 2.0f + 0.5f), (parentMap.height / 2.0f - h - 0.5f), 0);
                GameObject created = Instantiate(GameObject.Find("Prototype Obstacle Sprite"));
                positionList.Add(pos);
                positionList.Sort();
                created.name = "Obstacle Sprite" + pos.ToString();
                created.tag = "Obstacle";
                created.transform.parent = GameObject.Find("Game Panel").transform;
                created.transform.position = trans;
            }
        }
        public void Generate()
        {
            // make vertical obs
            for (int i = 1; i < parentMap.height - 1; ++i) // for every height
            {
                bool putObs = Random.Range(-5, 5) > 0;
                for (int j = 1; j < parentMap.width - 1; ++j)
                {
                    if (putObs && parentMap.blocks[i, j] == (int)Map.BLOCK_TYPE.WALKABLE)
                    {
                        Update(i, j);
                        if (Random.Range(-2, 5) < 0)
                            putObs = !putObs;

                    }
                    else
                    {
                        if (Random.Range(-2, 2) < 0)
                            putObs = !putObs;
                    }
                }
            }
            // make horizonal obs
            for (int j = 1; j < parentMap.width - 1; j++)
            {
                bool putObs = Random.Range(-5, 5) > 0;
                for (int i = 1; i < parentMap.width - 1; ++i)
                {
                    if (putObs && parentMap.blocks[i, j] == (int)Map.BLOCK_TYPE.WALKABLE)
                    {
                        Update(i, j);
                        if (Random.Range(-2, 5) < 0)
                            putObs = !putObs;
                    }
                    else
                    {
                        if (Random.Range(-2, 3) < 0)
                            putObs = !putObs;
                    }
                }
            }
        }
       
        public void Adjust()
        {
            int count = 0;
            bool find_something_to_adjust = true;
            do
            {
                find_something_to_adjust = DistributeAdjust();
                CorridorAdjust();
                count++;
                Debug.Log("Obstacles Adjusted");
            } while (find_something_to_adjust && count < 4);

            // in opening, obstacle should not neighbor or be on same block of the player and finish
            int playerPosition = parentMap.playerStartBlock[0] * parentMap.width + parentMap.playerStartBlock[1];
            if (positionList.Exists(x => x == playerPosition)) // same block
                Update(playerPosition);
            if (positionList.Exists(x => x == playerPosition + 1)) // right
                Update(playerPosition + 1);
            if (positionList.Exists(x => x == playerPosition - 1)) // left
                Update(playerPosition - 1);
            if (positionList.Exists(x => x == playerPosition + parentMap.width)) // up
                Update(playerPosition + parentMap.width);
            if (positionList.Exists(x => x == playerPosition - parentMap.width)) // down
                Update(playerPosition - parentMap.width);
        }

        private bool DistributeAdjust()
        {
            bool some_adjustment_are_done = false;
            // adjust too crowded and too loosen obstacles
            for (int i = 1; i < parentMap.height - 1; ++i)
            {
                for (int j = 1; j < parentMap.width - 1; ++j)
                {
                    int thisBlockPos = i * parentMap.width + j;
                    if (parentMap.blocks[i, j] != (int)Map.BLOCK_TYPE.WALKABLE)
                        continue;
                    else
                    {
                        bool this_is_obs = (positionList.Exists(x => x == thisBlockPos));
                        int di = -1, dj = -1;
                        int sameNeighborCount = 0;
                        while (di <= 1)
                        {
                            if (di == 0 & dj == 0) continue;
                            int neighborBlockPos = (i + di) * parentMap.width + (j + dj);
                            // no matter the neighbor block is really a obstacle or a wall, it all count as obstacle
                            bool neighbor_is_obs =
                                (positionList.Exists(x => x == neighborBlockPos)) || (parentMap.blocks[i + di, j + dj] != (int)Map.BLOCK_TYPE.WALKABLE);

                            // check if neighbor is same with this block
                            if (this_is_obs == neighbor_is_obs)
                                sameNeighborCount++;

                            // upadte neighbor blocks ij
                            if (dj == 1)
                            {
                                di++;
                                dj = -1;
                            }
                            else if (di == 0 & dj == -1) dj = 1;
                            else dj++;
                        }
                        if (sameNeighborCount >= (this_is_obs ? 6 : 5))
                        {
                            some_adjustment_are_done = true;
                            Update(i, j);
                            if (sameNeighborCount == 8)
                            {
                                if(Random.Range(-1, 1) == 0)
                                    Update(i + Random.Range(-1, 2), j);
                                else
                                    Update(i, j + Random.Range(-1, 2));
                            }
                        }
                    }
                } // end of for: j
            } // end of for: i
            return some_adjustment_are_done;
        }

        private void CorridorAdjust()
        {
            // after previous adjustion, there could be some "corridor shape" obstacles
            // we want to "close" those corridor
            for (int i = 1; i < parentMap.height - 1; ++i)
            {
                for (int j = 1; j < parentMap.width - 1; ++j)
                {
                    if (parentMap.blocks[i, j] == (int)Map.BLOCK_TYPE.WALKABLE)
                    {
                        int pos = i * parentMap.width + j, d = -1, mw = parentMap.width;
                        bool is_up_all_obs = true, is_down_all_obs = true,
                            is_left_all_obs = true, is_right_all_obs = true,
                            is_middle_all_walkable = true;
                        // check vertical corridor
                        while (d <= 1)
                        {
                            // Note: Map.BLOCK_TYPE.WALL is 1
                            is_up_all_obs = is_up_all_obs && (positionList.Exists(x => x == pos - mw + d) || parentMap.blocks[i - 1, j + d] == 1);
                            is_down_all_obs = is_down_all_obs && (positionList.Exists(x => x == pos + mw + d) || parentMap.blocks[i + 1, j + d] == 1);
                            is_middle_all_walkable = is_middle_all_walkable && !(positionList.Exists(x => x == pos + d) || parentMap.blocks[i, j + d] == 1);
                            d++;
                        }
                        if (is_middle_all_walkable && is_up_all_obs && is_down_all_obs)
                        {
                            Update(i, j); // close the walk way
                            if (Random.Range(-1, 2) > 0)
                            {
                                Update(i + ((Random.Range(0, 2) == 0) ? 1 : -1), j); // open a obs
                            }
                        }
                        else
                        {
                            // check horizontal corridor
                            d = -1;
                            is_middle_all_walkable = true;
                            while (d <= 1)
                            {
                                is_left_all_obs = is_left_all_obs && (positionList.Exists(x => x == pos + d * mw - 1) || parentMap.blocks[i + d, j - 1] == 1);
                                is_right_all_obs = is_right_all_obs && (positionList.Exists(x => x == pos + d * mw + 1) || parentMap.blocks[i + d, j + 1] == 1);
                                is_middle_all_walkable = is_middle_all_walkable && !(positionList.Exists(x => x == pos + d * mw) || parentMap.blocks[i + d, j] == 1);
                                d++;
                            }
                            if (is_middle_all_walkable && is_left_all_obs && is_right_all_obs)
                            {
                                Update(i, j); // close a walk way
                                if (Random.Range(-1, 2) > 0)
                                {
                                    Update(i, j + ((Random.Range(0, 2) == 0) ? 1 : -1)); // open a obs
                                }
                            }
                        }
                    }
                } // end of for: j
            } // end of for: i
        }

        private void DestroyObstacles()
        {
            GameObject[] obs = GameObject.FindGameObjectsWithTag("Obstacle");
            int k = 0;
            for (; k < obs.Length; k++)
            {
                Destroy(obs[k]);
                obs[k] = null;
            }
            Debug.Log("destroy " + k + " obs");
        }
    }
    /*
    public class Enemies
    {
        List<int> positionList;
        Map parentMap;
    }
    */
    public class Player
    {
        public int h;
        public int w;
        public Map parentMap;
        public int stepCount;
        GameObject playerSpriteObject = GameObject.Find("Player Sprite");
        Text stepRemainObject = GameObject.Find("Remaining Steps Output").GetComponent<Text>();

        public Player()
        {
            parentMap = null;
            h = -1;
            w = -1;
            stepCount = 0;
        }

        public Player(Map theMapPlayerWithIn)
        {
            parentMap = theMapPlayerWithIn;
            h = parentMap.playerStartBlock[0];
            w = parentMap.playerStartBlock[1];
            stepCount = 0;
        }

        public void Move(int dh, int dw)
        {
            //Debug.Log("thePlayer.Move() is called in Game_Panel");
            if (parentMap.blocks[(h + dh), (w + dw)] != (int)Map.BLOCK_TYPE.WALL)
            {
                if (parentMap.theObstacles.positionList.IndexOf((h + dh) * parentMap.width + (w + dw)) == -1)
                {
                    h = h + dh;
                    w = w + dw;
                    //Debug.Log("player position has been changed to (" + h + ", " + w + ")");
                    playerSpriteObject.transform.position = new Vector3((w - parentMap.width / 2.0f + 0.5f), (parentMap.height / 2.0f - h - 0.5f), 0);
                    stepCount++;
                    stepRemainObject.text = (parentMap.estimatedStep - stepCount).ToString();
                }
            }
            if (h == parentMap.finishBlock[0] && w == parentMap.finishBlock[1])
                ReachFinal();
        }

        public void DoAbility()
        {
            int dh = -1, dw = -1;
            while (dh <= 1)
            {
                parentMap.theObstacles.Update(h + dh, w + dw);
                // upadte neighbor blocks ij
                if (dw == 1)
                {
                    dh++;
                    dw = -1;
                }
                else if (dh == 0 & dw == -1) dw = 1;
                else dw++;
            }
            stepCount++;
            stepRemainObject.text = (parentMap.estimatedStep - stepCount).ToString();
        }

        private void ReachFinal()
        {
            GameObject.Find("Final Stats Background").GetComponent<SpriteRenderer>().enabled = true;
            GameObject.Find("Control Panel").GetComponent<Control_Panel>().toggleGameMenu();
            Debug.Log("FINISH!");
			Temp_Save_Data.UpdateLevel();
        }

        public void SetPositionTo(int newh, int neww)
        {
            if (newh >= parentMap.height || neww >= parentMap.width)
            {
                Debug.Log("illegal position");
                return;
            }
            //Debug.Log("thePlayer.SetPositionTo() is called in Game_Panel");
            if (parentMap.blocks[newh, neww] != (int)Map.BLOCK_TYPE.WALL)
            {
                if (parentMap.theObstacles.positionList.IndexOf(newh * parentMap.width + neww) == -1)
                {
                    h = newh;
                    w = neww;
                    //Debug.Log("player position has been changed to (" + h + ", " + w + ")");
                    playerSpriteObject.transform.position = new Vector3((w - parentMap.width / 2.0f + 0.5f), (parentMap.height / 2.0f - h - 0.5f), 0);
                }
                else
                {
                    parentMap.theObstacles.Update(newh, neww);
                }
            }
        }

        public void SetStepCount(int i)
        {
            stepCount = 0;
            stepRemainObject.text = parentMap.estimatedStep.ToString();
        }
    }

    public Map theMap;
    public Player thePlayer;

    public void RestartButton()
    {
        Debug.Log("Restart");
        GameObject.Find("Final Stats Background").GetComponent<SpriteRenderer>().enabled = false;
        theMap.theObstacles.Initialize();
        theMap.FindEstimatedPath();
        thePlayer.SetPositionTo(theMap.playerStartBlock[0], theMap.playerStartBlock[1]);
        thePlayer.SetStepCount(0);
    }

    public void playerMoveUp()
    {
        thePlayer.Move(-1, 0);
    }

    public void playerMoveLeft()
    {
        thePlayer.Move(0, -1);
    }

    public void playerMoveDown()
    {
        thePlayer.Move(1, 0);
    }

    public void playerMoveRight()
    {
        thePlayer.Move(0, 1);
    }

    public void playerDoAbility()
    {
        thePlayer.DoAbility();
    }

    // Use this for initialization
    void Start()
    {
        string sm = Temp_Save_Data.SelectedLevel;
        if (sm != null)
        {
            // initialize map
            theMap = new Map(sm);
            // initalize player
            thePlayer = new Player(theMap);
        }
    }
    float times_irreponsive = 0;
    // Update is called once per frame
    void Update()
    {
        if (times_irreponsive < Time.time)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                times_irreponsive = Time.time + 0.3f;
                playerMoveUp();
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                times_irreponsive = Time.time + 0.3f;
                playerMoveDown();
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                times_irreponsive = Time.time + 0.3f;
                playerMoveLeft();
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                times_irreponsive = Time.time + 0.3f;
                playerMoveRight();
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                times_irreponsive = Time.time + 0.3f;
                playerDoAbility();
            }
        }
    }
}
