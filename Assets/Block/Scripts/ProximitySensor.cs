using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximitySensor : MonoBehaviour
{
    public List<GameObject> Objects
    {
        set { }
        get { return touchingObjects; }
    }

    void Start()
    {
        if (!CompareTag("Trigger"))
        {
            Debug.Log("Warning: the ProximitySensor component is intended to be used with objects tagged as 'Trigger'.");
        }
    }

    List<GameObject> touchingObjects = new List<GameObject>();

    //use our triggers to keep track of what objects we are touching
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Trigger"))
        {
            touchingObjects.Add(GetBlock(collision));
            
            //check if we collided with another block
            if (GetBlockController(collision))
            {
                Debug.Log(gameObject.name + " PROXIMITY SENSOR: block #" + GetBlockController().SerialNumber + " touching block #" + GetBlockController(collision).SerialNumber);
            }
            else
            {
                Debug.Log(gameObject.name + " PROXIMITY SENSOR: block #" + GetBlockController().SerialNumber + " touching with " + collision.gameObject.name);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Trigger"))
        {
            touchingObjects.Remove(GetBlock(collision));

            if (GetBlockController(collision))
            {
                Debug.Log(gameObject.name + " PROXIMITY SENSOR: block #" + GetBlockController().SerialNumber + " stopped touching block #" + GetBlockController(collision).SerialNumber);
            }
            else
            {
                Debug.Log(gameObject.name + " PROXIMITY SENSOR: block #" + GetBlockController().SerialNumber + " stopped touching with " + collision.gameObject.name);
            }
        }
    }

    //Function to return the parent block object given its collider.
    GameObject GetBlock(Collider2D collider)
    {
        return collider.gameObject.GetComponentInParent<BlockController>().gameObject;
    }

    //Function to return the parent BlockController given its collider.
    BlockController GetBlockController(Collider2D collider)
    {
        return collider.gameObject.GetComponentInParent<BlockController>();
    }

    //Function to return our parent block object.
    GameObject GetBlock()
    {
        return GetComponentInParent<BlockController>().gameObject;
    }


    //Function to return our parent BlockController.
    BlockController GetBlockController()
    {
        return GetComponentInParent<BlockController>();
    }

}
