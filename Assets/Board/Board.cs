using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Board : MonoBehaviour
{
    public TMP_Text DebugTxt;

    enum State { IDLE, BLOCK_CREATED, BLOCK_ASSINGED };
    State state;

    public int MinPower = 1;
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
                {
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



    void AddBlockToColumn(int col, int power)
    {
        //Columns[col].AddBlock()
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

    //    bool bFindingMergeables;
    public void BlockCallback(BlockController bc, GameObject ObjCollidedWith)
    {
        //if (bc != CurrentBlock) return;
        //        if (bFindingMergeables) return;
        //        bFindingMergeables = true;
        Debug.Log("-->Callback: block " + bc.SerialNumber);

        //assign to a column
        //Columns[CurrentColumn].AddBlock(CurrentBlock, CurrentBlock.transform.position.y);

        //detach the callback
        //bc.callbackFunction = null; //FindMergeables;

        state = State.IDLE;
        BuildBoardMatrix();
        PrintBoard();

        //check the last added block
        //CheckBlockForMerge(Columns[CurrentColumn].Count - 1, CurrentColumn);

        //now check all blocks

        //FindMergeables();
        //       bFindingMergeables = false;
    }
 
    void SetCurrentBlockGravityScale(float gravityScale)
    {
        CurrentBlock.GetComponent<Rigidbody2D>().gravityScale = gravityScale;
    }


    //function to check if the provided row and column indeces reference a valid block
    bool BlockPresent(int row, int col)
    {
        //if ((row < 0) || (col < 0)) return false;
        //if ((row >= Column.MAX_NUM_ROWS) || (col >= NUM_COLS)) return false;
        //return (row < Columns[col].Count);
        return false;
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

        Debug.Log(block.name);
        Debug.Log("SN: " + block.GetComponent<BlockController>().SerialNumber);

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

    /*
    GameObject GetBlock(int col, int row)
    {
        int row_ = 0;
        ProximitySensor sensor = Floor[col].GetComponent<ProximitySensor>();

        while (sensor.Objects[row])
        {
            sensor = sensor.Objects[row];
        }
        return null;
    }
    */

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
