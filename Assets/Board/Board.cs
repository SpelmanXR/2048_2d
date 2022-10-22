using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Board : MonoBehaviour
{
    public TMP_Text DebugTxt;

    enum State { IDLE, BLOCK_CREATED, UPDATE_BOARD };
    State state;

    public int MinPower = 0;
    public int MaxPower = 5;

    public int FirstColXPosition = -2;
    public int ColWidth = 1;
    public int ColHeight = 10;
    public int ColY = 0;
    public int NumColumns = 5;
    public Color EvenColColor = new Color(0.15f, 0.15f, 0.15f);
    public Color OddColColor = new Color(0.18f, 0.25f, 0.18f);
    public GameObject ColumnPrefab;
    public GameObject BlockPrefab;
    public float FloorYPosition = -5f;
    public GameObject FloorPrefab;
    int[] ColXPosition;
    GameObject[] Floor;
    GameObject[,] BoardMatrix;

    const float DEFAULT_BLOCK_START_YPOS = 5f;
    const int MAX_NUM_ROWS = 8;

    //the column to which an unassigned block is currently aligned
    int CurrentColumn;
    BlockController CurrentBlock;   //reference to the newest unassigned block (there should only be 1 unassigned block at a time)

    private void Awake()
    {
        ColXPosition = new int[NumColumns];
        Floor = new GameObject[NumColumns];
        BoardMatrix = new GameObject[NumColumns, MAX_NUM_ROWS];
    }

    // Start is called before the first frame update
    void Start()
    {
        //create columns for the board
        MakeColumns();
    }


    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.IDLE:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    CreateBlock(Random.Range(MinPower, MaxPower + 1));
                    state = State.BLOCK_CREATED;
                    Debug.Log("-------------------------");
                }

                if (Input.GetKeyDown(KeyCode.D))
                {/*
                    List<GameObject> x = GetColumn(0);
                    string s = "";

                    if (x != null)
                    {
                        foreach (GameObject g in x)
                        {
                            s += g.GetComponent<BlockController>().Value + " ";
                        }
                    }
                    else
                    {
                        s = "No Blocks";
                    }
                    Debug.Log("Col[0]: " + s);
                    */
                    BoardMatrix[2, 1].GetComponent<BlockController>().Terminate(BlockController.Slide.UP);
                }

                break;

            case State.BLOCK_CREATED:
                {
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        CurrentColumn++;
                        CurrentColumn = Mathf.Clamp(CurrentColumn, 0, NumColumns - 1);
                        AlignToColumn();
                    }

                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        CurrentColumn--;
                        CurrentColumn = Mathf.Clamp(CurrentColumn, 0, NumColumns - 1);
                        AlignToColumn();
                    }

                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        //set gravity to full force
                        SetCurrentBlockGravityScale(2f);
                    }

                    break;
                }

            case State.UPDATE_BOARD:
                {
                    /*
                    BuildBoardMatrix();
                    PrintBoard();

                    //merge blocks as necessary
                    FindMergeables();

                    state = State.IDLE;
                    */
                    break;
                }

            default:
                break;
        }

    }

    void MakeColumns()
    {
        //create columns for the board
        for (int i = 0; i < NumColumns; i++)
        {
            ColXPosition[i] = FirstColXPosition + i * ColWidth;

            //create the column
            GameObject col = Instantiate(ColumnPrefab);
            col.transform.position = new Vector2(ColXPosition[i], ColY);
            col.transform.localScale = new Vector2(ColWidth, ColHeight);
            col.GetComponent<SpriteRenderer>().color = i % 2 == 1 ? OddColColor : EvenColColor;

            //add the floor
            Floor[i] = Instantiate(FloorPrefab);
            Floor[i].transform.position = new Vector2(ColXPosition[i], FloorYPosition);
            Floor[i].transform.localScale = new Vector2(ColWidth, 1);
        }
    }



    void CreateBlock(int power)
    {
        GameObject obj = Instantiate(BlockPrefab); ////Columns[0].CreateBlock(power);
        CurrentBlock = obj.GetComponent<BlockController>();

        //verify that this is a valid BlockController object
        if (CurrentBlock == null)
        {
            Debug.Log("CreateBlock() failed.: Object prefab not a BlockControllers.");
        }

        CurrentBlock.Power = power;

        //align to middle column
        CurrentColumn = 2;
        obj.transform.position = new Vector2(ColXPosition[CurrentColumn], DEFAULT_BLOCK_START_YPOS);

        //set a reduced gravity
        SetCurrentBlockGravityScale(0.01f);
        CurrentBlock.callbackFunction = BlockCallback;

        PrintBoard();
    }


    //aligns the current unassigned block to the currently selected column
    void AlignToColumn()
    {
        CurrentBlock.transform.position = new Vector2(ColXPosition[CurrentColumn], CurrentBlock.transform.position.y);
    }


    /// <summary>
    /// Block trigger callback function
    /// </summary>
    /// <param name="bc"></param>
    /// <param name="ObjCollidedWith"></param>
    public void BlockCallback(BlockController bc, GameObject ObjCollidedWith)
    {
        Debug.Log("-->Callback: block " + bc.SerialNumber);

        state = State.IDLE;

        BuildBoardMatrix();
        PrintBoard();

        //merge blocks as necessary
        FindMergeables();
    }


    void SetCurrentBlockGravityScale(float gravityScale)
    {
        CurrentBlock.GetComponent<Rigidbody2D>().gravityScale = gravityScale;
    }



    List<GameObject> GetColumn(int col)
    {
        List<GameObject> blocks = new();

        if (Floor[col].GetComponent<Floor>().NumBlocks == 0)
        {
            //column is empty
            return blocks;
        }

        //Sanity check
        if (Floor[col].GetComponent<Floor>().NumBlocks != 1)
        {
            Debug.Log("Well,this is insane:  Board.cs:GetColumn() reports that there are " + Floor[col].GetComponent<Floor>().NumBlocks + " blocks on the floor of column " + col);
        }


        //Add block 0 to the list
        GameObject block = Floor[col].GetComponent<Floor>().BlockOnFloor;
        
        if (block == null)
        {
            Debug.Log("This is odd: BlockOnFloor is null.");
        }
        
        blocks.Add(block);

        //Debug.Log(block.name);
        //Debug.Log("SN: " + block.GetComponent<BlockController>().SerialNumber);

        //now follow the linked blocks
        while (block.GetComponent<BlockController>().TriggerTop.GetComponent<ProximitySensor>().Objects.Count > 0)
        {
            //Debug.Log("Column count: " + block.GetComponent<BlockController>().TriggerTop.GetComponent<ProximitySensor>().Objects.Count);
            //point to the next block
            block = block.GetComponent<BlockController>().TriggerTop.GetComponent<ProximitySensor>().Objects[0];
            blocks.Add(block);
        }

        return blocks;
    }

    /// <summary>
    /// Function to build the board matrix based on the current position of blocks on the board.
    /// </summary>
    void BuildBoardMatrix()
    {
        for (int col = 0; col < NumColumns; col++)
        {
            List<GameObject> colList = GetColumn(col);
            for (int row = 0; row < MAX_NUM_ROWS; row++)
            {
                if (row < colList.Count)
                    BoardMatrix[col, row] = colList[row];
                else
                    BoardMatrix[col, row] = null;
            }
        }
    }

    /// <summary>
    /// Function to return the BlockController component of the block
    /// game objects in the BoardMatrix.  The function assumes that
    /// the matrix is up-to-date and that the provided indeces are correct.
    /// </summary>
    /// <param name="row">The matrix row</param>
    /// <param name="col">The matrix column</param>
    /// <returns></returns>
    BlockController BCMatrix(int col, int row)
    {
        return BoardMatrix[col, row].GetComponent<BlockController>();
    }



    //function to check if the provided row and column indeces reference a valid block
      bool BlockPresent(int col, int row)
    {
        if ((row < 0) || (col < 0)) return false;
        if ((row >= MAX_NUM_ROWS) || (col >= NumColumns)) return false;
        return (BoardMatrix[col, row] != null);
    }

    //function to check if 2 blocks can be merged (same power)
    //CanMerge() assumes the indeces are valid
    bool CanMerge(int col0, int row0, int col1, int row1)
    {
        return (BCMatrix(col0, row0).Power == BCMatrix(col1, row1).Power);
    }


    //overloaded function to merge 4 blocks.  Merge() verifies valid indices
    //and matching powers before attempting to merge.  Returns true on success
    //false otherwise.
    bool Merge(int col0, int row0, int col1, int row1, int col2, int row2, int col3, int row3)
    {
        //verify all indices are valid
        if (!BlockPresent(col0, row0)) return false;
        if (!BlockPresent(col1, row1)) return false;
        if (!BlockPresent(col2, row2)) return false;
        if (!BlockPresent(col3, row3)) return false;

        //verify 3-way merge
        if (!CanMerge(col0, row0, col1, row1)) return false;
        if (!CanMerge(col0, row0, col2, row2)) return false;
        if (!CanMerge(col0, row0, col3, row3)) return false;

        Debug.Log("Merge3");

        //increment power by 3
        BCMatrix(col0, row0).Power += 3;

        //delete 3 blocks
        BCMatrix(col1, row1).Terminate(GetSlideDirection(col0, row0, col1, row1));
        BCMatrix(col2, row2).Terminate(GetSlideDirection(col0, row0, col2, row2));
        BCMatrix(col3, row3).Terminate(GetSlideDirection(col0, row0, col3, row3));

        PrintBoard();

        return true;
    }


    //overloaded function to merge 3 blocks.  Merge() verifies valid indices
    //and matching powers before attempting to merge.  Returns true on success
    //false otherwise.
    bool Merge(int col0, int row0, int col1, int row1, int col2, int row2)
    {
        //verify all indices are valid
        if (!BlockPresent(col0, row0)) return false;
        if (!BlockPresent(col1, row1)) return false;
        if (!BlockPresent(col2, row2)) return false;

        //verify 2-way merge
        if (!CanMerge(col0, row0, col1, row1)) return false;
        if (!CanMerge(col0, row0, col2, row2)) return false;

        Debug.Log("Merge2");

        //increment power by 2
        BCMatrix(col0, row0).Power += 2;

        //delete 2 blocks
        BCMatrix(col1, row1).Terminate(GetSlideDirection(col0, row0, col1, row1));
        BCMatrix(col2, row2).Terminate(GetSlideDirection(col0, row0, col2, row2));

        PrintBoard();

        return true;
    }

    //overloaded function to merge 2 blocks.  Merge() verifies valid indices
    //and matching powers before attempting to merge.  Returns true on success
    //false otherwise.
    bool Merge(int col0, int row0, int col1, int row1)
    {
        //verify all indices are valid
        if (!BlockPresent(col0, row0)) return false;
        if (!BlockPresent(col1, row1)) return false;

        //verify 1-way merge
        if (!CanMerge(col0, row0, col1, row1)) return false;

        Debug.Log("Merge1");

        //increment power by 1
        BCMatrix(col0, row0).Power += 1;

        //delete 1 block
        BCMatrix(col1, row1).Terminate(GetSlideDirection(col0, row0, col1, row1));

        PrintBoard();

        return true;
    }



    BlockController.Slide GetSlideDirection(int col0, int row0, int col1, int row1)
    {
        if (col0 > col1) return BlockController.Slide.RIGHT;
        if (col1 > col0) return BlockController.Slide.LEFT;
        if (row0 > row1) return BlockController.Slide.UP;
        if (row0 < row1) return BlockController.Slide.DOWN;

        Debug.Log("GetSlideDirection(): No valid slide direction found.");
        return BlockController.Slide.NONE;
    }

    //function to find mergeable blocks
    void FindMergeables()
    {
        bool FoundMatch;

        //check for 3 neighbors (L+R+B)
        for (int row = MAX_NUM_ROWS - 1; row >= 1; row--)
        {
            for (int col = 1; col < NumColumns-1; col++)
            {
                //if (row >= Columns[col].Count) continue;

                FoundMatch = Merge(col, row, col - 1, row, col + 1, row, col, row - 1);
                if (FoundMatch)
                {
                    Debug.Log("[LRB]");
                    return;
                }
            }  //for (int col = ...
        } //for (int row = ...


        //check for 2 neighbors (L+R), (R+B) or (L+B)
        FoundMatch = false;
        for (int row = MAX_NUM_ROWS - 1; row >= 0; row--)
        {
            for (int col = 1; col < NumColumns-1; col++)
            {
                //if (row >= Columns[col].Count) continue;

                FoundMatch = Merge(col, row, col - 1, row, col + 1, row);
                if (FoundMatch)
                {
                    Debug.Log("[LR]");
                    return;
                }

                FoundMatch = Merge(col, row, col + 1, row, col, row - 1);
                if (FoundMatch)
                {
                    Debug.Log("[RB]");
                    return;
                }

                FoundMatch = Merge(col, row, col - 1, row, col, row - 1);
                if (FoundMatch)
                {
                    Debug.Log("[LB]");
                    return;
                }
            }  //for (int col = ...
        } //for (int row = ...




        //check for 1 neighbor (L), (R) or (B).
        for (int row = MAX_NUM_ROWS - 1; row >= 0; row--)
        {
            for (int col = 0; col < NumColumns; col++)
            {
                //if (row >= Columns[col].Count) continue;

                FoundMatch = Merge(col, row, col - 1, row);
                if (FoundMatch)
                {
                    Debug.Log("[L]");
                    return;
                }

                FoundMatch = Merge(col, row, col + 1, row);
                if (FoundMatch)
                {
                    Debug.Log("[R]");
                    return;
                }

                FoundMatch = Merge(col, row, col, row - 1);
                if (FoundMatch)
                {
                    Debug.Log("[B]");
                    return;
                }
            }  //for (int col = ...
        } //for (int row = ...

    }   //void FindMergeables()









    void PrintBoard()
    {
        string str = "";
        //for (int row = 0; row < MAX_NUM_ROWS; row++)
        for (int row = MAX_NUM_ROWS - 1; row >= 0; row--)
        {
            string rowStr = "";
            for (int col = 0; col < NumColumns; col++)
            {
                if (BoardMatrix[col, row] != null)
                    rowStr += BoardMatrix[col, row].GetComponent<BlockController>().Value.ToString("00000 ");
                else
                    rowStr += "XXXXX ";
            }
            str += rowStr + "\n";
        }

        debug(str, false);
    }

    void debug(string s, bool Append = true)
    {
        if (Append)
        {
            DebugTxt.text += s;
            return;
        }

        DebugTxt.text = s;

    }
}
