using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameBoard : MonoBehaviour
{
    const int NUM_COLS = 5;

    public Column Column0;
    public Column Column1;
    public Column Column2;
    public Column Column3;
    public Column Column4;

    public int MinPower = 1;
    public int MaxPower = 5;

    //Declare an array of lists
    Column[] Columns;

    //the column to which an unassigned block is currently aligned
    int CurrentColumn;
    BlockController CurrentBlock;   //reference to the newest unassigned block (there should only be 1 unassigned block at a time)


    public TMP_Text DebugTxt;

    enum State { IDLE, BLOCK_CREATED, BLOCK_ASSINGED };
    State state;

    // Start is called before the first frame update
    void Start()
    {
        //initialize the debug text
        debug("", false);

        state = State.IDLE;

        //create an array of NUM_COLS lists
        Columns = new Column[NUM_COLS];

        //initialize each list the Board array
        Columns[0] = Column0;
        Columns[1] = Column1;
        Columns[2] = Column2;
        Columns[3] = Column3;
        Columns[4] = Column4;

        PrintBoard();
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
                }
                break;

            case State.BLOCK_CREATED:
                {
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        CurrentColumn++;
                        CurrentColumn = Mathf.Clamp(CurrentColumn, 0, NUM_COLS - 1);
                        AlignToColumn();
                    }

                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        CurrentColumn--;
                        CurrentColumn = Mathf.Clamp(CurrentColumn, 0, NUM_COLS - 1);
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


    void PrintBoard()
    {
        string str = "";
        //for (int row = 0; row < MAX_NUM_ROWS; row++)
        for (int row = Column.MAX_NUM_ROWS-1; row >= 0 ; row--)
        {
            string rowStr = "";
            for (int col = 0; col < NUM_COLS; col++)
            {
                if (row < Columns[col].Count )
                    rowStr += Columns[col].GetValue(row).ToString("00000 ");
                else
                    rowStr += "XXXXX ";
            }
            str += rowStr + "\n";
        }

        debug(str, false);
    }

    void debug(string s, bool Append=true)
    {
        if (Append)
        {
            DebugTxt.text += s;
            return;
        }

        DebugTxt.text = s;

    }


    void AddBlockToColumn(int col, int power)
    {
        //Columns[col].AddBlock()
    }


    void CreateBlock(int power)
    {
        CurrentBlock = Columns[0].CreateBlock(power);
        GameObject obj = CurrentBlock.gameObject;

        //align to middle column
        CurrentColumn = 2;
        obj.transform.position = new Vector2(Columns[CurrentColumn].transform.position.x, Column.DEFAULT_BLOCK_START_YPOS);

        //set a reduced gravity
        SetCurrentBlockGravityScale(0.01f);
        CurrentBlock.callbackFunction = BlockCallback;

        PrintBoard();
    }

    //aligns the current unassigned block to the currently selected column
    void AlignToColumn()
    {
        CurrentBlock.transform.position = new Vector2(Columns[CurrentColumn].transform.position.x, CurrentBlock.transform.position.y);
    }

//    bool bFindingMergeables;
    public void BlockCallback(BlockController bc)
    {
        if (bc != CurrentBlock) return;
//        if (bFindingMergeables) return;
//        bFindingMergeables = true;        //Debug.Log("BlockCallback called!");

        //assign to a column
        Columns[CurrentColumn].AddBlock(CurrentBlock, CurrentBlock.transform.position.y);

        //detach the callback
        //bc.callbackFunction = null; //FindMergeables;

        state = State.IDLE;
        PrintBoard();

        //check the last added block
        //CheckBlockForMerge(Columns[CurrentColumn].Count - 1, CurrentColumn);

        //now check all blocks

        FindMergeables();
 //       bFindingMergeables = false;
    }
    /*
    public void FindMergeables(BlockController bc)
    {
        FindMergeables();
    }
    */

    void SetCurrentBlockGravityScale(float gravityScale)
    {
        CurrentBlock.GetComponent<Rigidbody2D>().gravityScale = gravityScale;
    }

    /*
    BlockController GetBlock(int row, int col)
    {
        //check if there is no block at the provided coodinates
        if (Columns[col].Count <= row) return null;

        return Columns[col];
    }
    */

    //function to check if the provided row and column indeces reference a valid block
    bool BlockPresent(int row, int col)
    {
        if ((row < 0) || (col < 0)) return false;
        if ((row >= Column.MAX_NUM_ROWS) || (col >= NUM_COLS)) return false;
        return (row < Columns[col].Count);
    }


    //function to check if 2 blocks can be merged (same power)
    //CanMerge() assumes the indeces are valid
    bool CanMerge(int row0, int col0, int row1, int col1)
    {
        return (Columns[col0].GetPower(row0) == Columns[col1].GetPower(row1));
    }


    //overloaded function to merge 4 blocks.  Merge() verifies valid indices
    //and matching powers before attempting to merge.  Returns true on success
    //false otherwise.
    bool Merge(int row0,int col0, int row1, int col1, int row2, int col2, int row3, int col3)
    {
        //verify all indices are valid
        if (!BlockPresent(row0, col0)) return false;
        if (!BlockPresent(row1, col1)) return false;
        if (!BlockPresent(row2, col2)) return false;
        if (!BlockPresent(row3, col3)) return false;

        //verify 3-way merge
        if (!CanMerge(row0, col0, row1, col1)) return false;
        if (!CanMerge(row0, col0, row2, col2)) return false;
        if (!CanMerge(row0, col0, row3, col3)) return false;
 
        //increment power by 3
        Columns[col0].SetPower(row0, Columns[col0].GetPower(row0) + 3);

       //delete 3 blocks
        Columns[col1].DeleteBlockAt(row1, GetSlideDirection(col0, row0, col1, row1));
        Columns[col2].DeleteBlockAt(row2, GetSlideDirection(col0, row0, col2, row2));
        Columns[col3].DeleteBlockAt(row3, GetSlideDirection(col0, row0, col3, row3));

        PrintBoard();

        return true;
    }


    //overloaded function to merge 3 blocks.  Merge() verifies valid indices
    //and matching powers before attempting to merge.  Returns true on success
    //false otherwise.
    bool Merge(int row0, int col0, int row1, int col1, int row2, int col2)
    {
        //verify all indices are valid
        if (!BlockPresent(row0, col0)) return false;
        if (!BlockPresent(row1, col1)) return false;
        if (!BlockPresent(row2, col2)) return false;

        //verify 2-way merge
        if (!CanMerge(row0, col0, row1, col1)) return false;
        if (!CanMerge(row0, col0, row2, col2)) return false;

        //increment power by 2
        Columns[col0].SetPower(row0, Columns[col0].GetPower(row0) + 2);

        //delete 2 blocks
        Columns[col1].DeleteBlockAt(row1, GetSlideDirection(col0, row0, col1, row1));
        Columns[col2].DeleteBlockAt(row2, GetSlideDirection(col0, row0, col2, row2));

        PrintBoard();

        return true;
    }

    //overloaded function to merge 2 blocks.  Merge() verifies valid indices
    //and matching powers before attempting to merge.  Returns true on success
    //false otherwise.
    bool Merge(int row0, int col0, int row1, int col1)
    {
        //verify all indices are valid
        if (!BlockPresent(row0, col0)) return false;
        if (!BlockPresent(row1, col1)) return false;

        //verify 1-way merge
        if (!CanMerge(row0, col0, row1, col1)) return false;

        //increment power by 1
        Columns[col0].SetPower(row0, Columns[col0].GetPower(row0) + 1);

        //delete 1 block
        Columns[col1].DeleteBlockAt(row1, GetSlideDirection(col0, row0, col1, row1));

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
        bool bRepeat = true;

        //while (bRepeat)
        //{
            bRepeat = false;

            bool FoundMatch;

            //check for 3 neighbors (L+R+B)
            FoundMatch = true;
            while (FoundMatch)
            {
                FoundMatch = false;
                for (int row = 0; row < Column.MAX_NUM_ROWS; row++)
                {
                    for (int col = 0; col < NUM_COLS; col++)
                    {
                        bool found1 = Merge(row, col, row, col - 1, row, col + 1, row - 1, col);
                        if (found1) Debug.Log("[LRB]");

                        FoundMatch |= found1;
                    }
                }
            if (FoundMatch) return; //bRepeat = true;
            }   //while (FoundMatch)

            //check for 2 neighbors (L+R), (R+B) or (L+B)
            FoundMatch = true;
            while (FoundMatch)
            {
                FoundMatch = false;
                for (int row = 0; row < Column.MAX_NUM_ROWS; row++)
                {
                    for (int col = 0; col < NUM_COLS; col++)
                    {
                        bool found1 = Merge(row, col, row, col - 1, row, col + 1);
                        if (found1) Debug.Log("[LR]");

                        bool found2 = Merge(row, col, row, col + 1, row - 1, col);
                        if (found2) Debug.Log("[RB]");

                        bool found3 = Merge(row, col, row, col - 1, row - 1, col);
                        if (found3) Debug.Log("[LB]");

                        FoundMatch |= found1 | found2 | found3;
                    }
                }
            if (FoundMatch) return;// bRepeat = true;
            }   //while (FoundMatch)

            //check for 1 neighbor (L), (R) or (B).
            FoundMatch = true;
            while (FoundMatch)
            {
                FoundMatch = false;
                for (int row = 0; row < Column.MAX_NUM_ROWS; row++)
                {
                    for (int col = 0; col < NUM_COLS; col++)
                    {
                        bool found1 = Merge(row, col, row, col - 1);
                        if (found1) Debug.Log("[L]");

                        bool found2 = Merge(row, col, row, col + 1);
                        if (found2) Debug.Log("[R]");

                        bool found3 = Merge(row, col, row - 1, col);
                        if (found3) Debug.Log("[B]");

                        FoundMatch |= found1 | found2 | found3;
                    }
                }
            if (FoundMatch) return;// bRepeat = true;
            }   //while (FoundMatch)

            if (bRepeat) Debug.Log("Repeating...");
        //}   //while (bRepeat)
    }   //void FindMergeables()

}   //class GameBoard
