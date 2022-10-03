using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Column : MonoBehaviour
{
    //inspector reference to the Block prefab
    public GameObject BlockPrefab;

    List<BlockController> BlockControllers;
    public const int MAX_NUM_ROWS = 6;
    public const float DEFAULT_BLOCK_START_YPOS = 5f;

    //property to get the BlockControllers list length
    public int Count
    {
        set { }
        get { return BlockControllers.Count; }
    }

    // Start is called before the first frame update
    void Start()
    {
        BlockControllers = new List<BlockController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //creates a block and adds it to the BlockController list.
    public void CreateAndAddBlock(int power, float YPos = DEFAULT_BLOCK_START_YPOS)
    {
        AddBlock(CreateBlock(power), YPos);
    }


    //creates a block and adds it to the BlockController list.
    public BlockController CreateBlock(int power)
    {
        //create a block
        GameObject block = Instantiate(BlockPrefab);

        //get a reference to the BlockController and set the power
        BlockController blockController = block.GetComponent<BlockController>();

        //verify that this is a valid BlockController object
        if (blockController == null)
        {
            Debug.Log("CreateBlock() failed.: Object prefab not a BlockControllers.");
            return null;
        }

        blockController.Power = power;

        return blockController;
    }

    //adds a block to the BlockController list.
    //Optionally, specify the vertical position on the screen where the block will first appear
    public void AddBlock(BlockController blockController, float YPos = DEFAULT_BLOCK_START_YPOS)
    {
        //position it in this column
        Vector2 pos = new Vector2(transform.position.x, YPos);
        blockController.transform.position = pos;

        //verify that this is a valid BlockController object
        if (blockController == null)
        {
            Debug.Log("AddBlock(): Not a BlockController... object not added to BlockControllers.");
            return;
        }

        //add to our CodData
        BlockControllers.Add(blockController);
    }

    public void DeleteBlockAt(int index)
    {
        BlockController bc = BlockControllers[index];
        BlockControllers.RemoveAt(index);
        Destroy(bc.gameObject);
    }

    //function that returns the power at a specific row
    public int GetPower(int row)
    {
        return BlockControllers[row].Power;
    }

    //function that sets the power at a specific row
    public void SetPower(int row, int power)
    {
        BlockControllers[row].Power = power;
    }

    //function that returns the value at a specific row
    public int GetValue(int row)
    {
        return BlockControllers[row].Value;
    }

    public BlockController GetBlock(int row)
    {
        return BlockControllers[row];
    }    
}
