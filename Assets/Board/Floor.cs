using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    int numBlocks = 0;

    public int NumBlocks
    {
        set { }
        get { return numBlocks; }
    }

    GameObject blockOnFloor;

    public GameObject BlockOnFloor
    {
        set { }
        get { return blockOnFloor; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trigger"))
        {
            numBlocks++;
            //blockOnFloor = collision.gameObject;
            blockOnFloor = GetBlock(collision);
            Debug.Log("Blocks on Floor " + numBlocks );
            //Debug.Log(blockOnFloor.name);
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Trigger"))
        {
            numBlocks--;
            blockOnFloor = null;
            Debug.Log("Blocks on Floor " + numBlocks);
        }
    }

    //Function to return the parent block object given its collider.
    GameObject GetBlock(Collider2D collider)
    {
        return collider.gameObject.GetComponentInParent<BlockController>().gameObject;
    }
}
