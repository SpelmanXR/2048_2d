using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// The Block object has a 'Collider' and 'Trigger' children objects.
/// The collider object is used to control the physics.  It is always on.
/// The trigger object is turned on/off dpending on the state of the block.
/// OnTrigger() calls the user supplied callback function.  When the object
/// is first created, the trigger is enabled.  The OnTrigger() function
/// disables the trigger, meaning that the callback function is executed
/// only once.  When a block is terminated, the trigger of the block above it
/// is re-enabled as that block is now again in free-fall.
///
/// temp: The callback function may simply set a "needsUpdate" flag in the
/// GameBoard object.
/// </summary>
public class BlockController : MonoBehaviour
{
    public enum Slide { NONE, UP, DOWN, LEFT, RIGHT }

    int power;      //the power of 2 field
    public TMP_Text ValueText;  //reference to TMP Text
    public Animator SpriteAnimator;
    public Animator LightningAnimator;
    //public Collider2D ColliderTop;
    //public Collider2D ColliderBottom;
    public Collider2D TriggerTop;
    public Collider2D TriggerBottom;
    public Collider2D TriggerLeft;
    public Collider2D TriggerRight;

    //defines what type of method you're going to call
    //and declare a variable to hold the method you're going to call.
    public delegate void Callback(BlockController bc, GameObject ObjCollidedWith);
    public Callback callbackFunction;
  
    //blocks will have serial numbers.  This will be used to prioritize block merges

    static int SN = 0;      //class serial number
    int serialNumber;   //current object's S/N

    //bool bTerminate = false;        //set to true to self-destruct
    //const float TERMINATION_DELAY_SEC = 1f;     //time for the termination animation to run
    //float TerminationTime;

    //new block
    //bNewBlock is initially true, but is set to false after the first trigger.
    bool bNewBlock;
    public bool NewBlock
    {
        set { }
        get { return bNewBlock; }
    }

    //serial number property
    public int SerialNumber
    {
        set { }
        get { return serialNumber; }
    }

    //Power of 2 property
    public int Power
    {
        set
        {
            power = value;
            ValueText.text = Value.ToString();
            //EnableTrigger(true);
        }

        get { return power; }
    }

    //Value property
    public int Value
    {
        set { } //can't set the value... set the power instead
        get { return 1 << power; }  //calculate and return the value based on the power
    }

    private void Awake()
    {
        //indicate that the power is not yet valid by setting power to -1
        power = -1; //not a legal value
        callbackFunction = null;
        
        serialNumber = SN;
        SN++;

        //bTerminate = false;
        bNewBlock = true;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //check if we collided with another block
        if (collision.gameObject.GetComponent<BlockController>())
        {
            Debug.Log("OnCollisionEnter: block #" + SerialNumber + " collided with block #" + collision.gameObject.GetComponent<BlockController>().SerialNumber);
        }
        else
        {
            Debug.Log("OnCollisionEnter: block #" + SerialNumber + " collided with " + collision.gameObject.name);
        }

        if (callbackFunction != null)
            callbackFunction(this, collision.gameObject);

        //after callback, set bNewBlock to false
        bNewBlock = false;
    }


    public void Terminate(Slide direction)
    {
        switch (direction)
        {
            case Slide.UP:
                SpriteAnimator.SetTrigger("MoveUp");
                LightningAnimator.SetTrigger("up");
                break;

            case Slide.DOWN:
                SpriteAnimator.SetTrigger("MoveDown");
                LightningAnimator.SetTrigger("down");
                break;

            case Slide.LEFT:
                SpriteAnimator.SetTrigger("MoveLeft");
                LightningAnimator.SetTrigger("left");
                break;

            case Slide.RIGHT:
                SpriteAnimator.SetTrigger("MoveRight");
                LightningAnimator.SetTrigger("right");
                break;

            default:
                Debug.Log("Terminate(): Invalid Slide Direction.");
                LightningAnimator.SetTrigger("none");
                break;
        }
    }
    
}
