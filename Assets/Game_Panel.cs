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
            int estimate_least_step = astar.FindPathLength(true);
            int estimate_least_step_ignore_obs = astar.FindPathLength(false);
            Debug.Log(estimate_least_step + " / " + estimate_least_step_ignore_obs);
            // add bonus steps
            int bonusThreshold = (int) (width * height * 0.02);
            if (estimate_least_step / 4 < bonusThreshold)
                estimate_least_step = estimate_least_step * 5 / 4;
            else
                estimate_least_step += bonusThreshold;

            Text esco = GameObject.Find("Estimated Step Count Output").GetComponent<Text>();
            if (estimate_least_step == -1)
                esco.text = "err";
            else
                esco.text = estimate_least_step.ToString();
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
            int obs_start, obs_end;
            // make vertical obs
            for (int i = 1; i < parentMap.height - 1; ++i) // for every height
            {
                obs_start = Random.Range(parentMap.width - 1, 0); // choose a width position
                // there's some chance to make no walls
                if (obs_start == 0)
                    continue;
                if (obs_start == 1)
                {
                    if (parentMap.blocks[i, 1] == 0)
                        Update(i, 1);
                }
                else if (obs_start < parentMap.width - 1)
                {
                    obs_end = Random.Range(obs_start, 1); // then find anothor width position lesser than previous one
                    for (int j = obs_start; j >= obs_end; --j)
                    {
                        if (parentMap.blocks[i, j] == 0)
                            Update(i, j);
                    }
                }
            }
            // make horizonal obs
            for (int j = 1; j < parentMap.width - 1; j++)
            {
                obs_start = Random.Range(parentMap.width - 1, 0);
                // there's some chance to make no walls
                if (obs_start == 0)
                    continue;
                if (obs_start == 1)
                {
                    if (parentMap.blocks[1, j] == 0)
                        Update(1, j);
                }
                else if (obs_start < parentMap.height - 1)
                {
                    obs_end = Random.Range(obs_start, 1);
                    for (int i = obs_start; i >= obs_end; i--)
                    {
                        if (parentMap.blocks[i, j] == 0)
                          //if (positionList.IndexOf(i * parentMap.width + j) == -1) // dont wanna update existed obs cause it could be too loosen
                            Update(i, j);
                    }
                }
            }
        }

        public void Adjust()
        {
            bool adjust_done = false;
            int count = 0;
            do
            {
                adjust_done = DistributeAdjust();
                CorridorAdjust();
                count++;
            } while (adjust_done && count < 3);
            DistributeAdjust();
            //CorridorAdjust();
            //DistributeAdjust();

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
                    int k = i * parentMap.width + j;
                    if (parentMap.blocks[i, j] != (int)Map.BLOCK_TYPE.WALKABLE)
                        continue;
                    else
                    {
                        bool this_block_is_obstacle_block = (positionList.IndexOf(k) >= 0);
                        int di = -1, dj = -1;
                        int sameNeighborCount = 0;
                        while (di <= 1)
                        {
                            if (di == 0 & dj == 0) continue;
                            int neighborBlockPos = (i + di) * parentMap.width + (j + dj);
                            // no matter the neighbor block is really a obstacle or a wall, it all count as obstacle
                            bool neighbor_block_is_obstacle_block =
                                (positionList.IndexOf(neighborBlockPos)) >= 0 || (parentMap.blocks[i + di, j + dj] != (int)Map.BLOCK_TYPE.WALKABLE);

                            // check if neighbor is same with this block
                            if (this_block_is_obstacle_block == neighbor_block_is_obstacle_block)
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
                        if (sameNeighborCount >= (this_block_is_obstacle_block ? 7 : 6))
                        {
                            some_adjustment_are_done = true;
                            Update(i, j);
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
                        int pos = i * parentMap.width + j;
                        int d = -1;
                        bool is_middle_all_walkable = true;
                        bool is_up_all_obstacle = true, is_down_all_obstacle = true;
                        bool is_left_all_obstacle = true, is_right_all_obstacle = true;
                        // check vertical corridor
                        while (d <= 1)
                        {
                            is_up_all_obstacle = is_up_all_obstacle && (positionList.Exists(x => x == pos - parentMap.width + d) || parentMap.blocks[i - 1, j + d] == (int)Map.BLOCK_TYPE.WALL);
                            is_down_all_obstacle = is_down_all_obstacle && (positionList.Exists(x => x == pos + parentMap.width + d) || parentMap.blocks[i + 1, j + d] == (int)Map.BLOCK_TYPE.WALL);
                            is_middle_all_walkable = is_middle_all_walkable && !(positionList.Exists(x => x == pos + d) || parentMap.blocks[i, j + d] == (int)Map.BLOCK_TYPE.WALL);
                            d++;
                        }
                        if (is_middle_all_walkable && (is_up_all_obstacle || is_down_all_obstacle))
                        {
                            Update(i, j + Random.Range(-1, 1));
                        }
                        else
                        {
                            // check horizontal corridor
                            d = -1;
                            is_middle_all_walkable = true;
                            while (d <= 1)
                            {
                                is_left_all_obstacle = is_left_all_obstacle && (positionList.Exists(x => x == pos + d * parentMap.width - 1) || parentMap.blocks[i + d, j - 1] == (int)Map.BLOCK_TYPE.WALL);
                                is_right_all_obstacle = is_right_all_obstacle && (positionList.Exists(x => x == pos + d * parentMap.width + 1) || parentMap.blocks[i + d, j + 1] == (int)Map.BLOCK_TYPE.WALL);
                                is_middle_all_walkable = is_middle_all_walkable && !(positionList.Exists(x => x == pos + d * parentMap.width) || parentMap.blocks[i + d, j] == (int)Map.BLOCK_TYPE.WALL);
                                d++;
                            }
                            if (is_middle_all_walkable && (is_left_all_obstacle || is_right_all_obstacle))
                            {
                                Update(i + Random.Range(-1, 1), j);
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
        Text stepCountObject = GameObject.Find("Player's Step Count Output").GetComponent<Text>();

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
                    stepCountObject.text = stepCount.ToString();
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
            stepCountObject.text = stepCount.ToString();
        }

        public void ReachFinal()
        {
            GameObject.Find("Final Stats Background").GetComponent<SpriteRenderer>().enabled = true;
            GameObject.Find("Game Menu Canvas").transform.Translate(new Vector3(0.0f, 0.0f, 2.0f));
            Debug.Log("FINISH!");
            Save_Data.levelProcess++;
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
            stepCount = i;
            stepCountObject.text = stepCount.ToString();
        }
    }

    public Map theMap;
    public Player thePlayer;

    public void Restart()
    {
        Debug.Log("Restart");
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
        string sm = Save_Data.SelectedLevel;
        if (sm != null)
        {
            // initialize map
            theMap = new Map(sm);
            // initalize player
            thePlayer = new Player(theMap);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
