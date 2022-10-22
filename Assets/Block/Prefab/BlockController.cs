using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// The Block object has 2 'Collider' and 4 'Trigger' children objects.
/// The 2 collider objects are used to control the physics.  They are always on.
/// The bottom object (ColliderBottom) contains a collider that is about 90%
/// the width of the block and about 40% of its height.  This collider object
/// has no rigidbody.  Therefore, it is attached to the parent rigidbody and
/// triggers the parent's OnCollision() callback.
/// The top object (ColliderTop) has its own rigidbody.  This is solely for
/// the purpose of preventing it from calling the parent OnCollision().
/// With this arrangement, a user-provided callback function is called only
/// once when two block collide or when a block collides with the floor.
///
/// The left and right trigger objects keep track of objects that come into
/// contact with the block from the top, bottom, left and right.  Apart from
/// the transitionary period when a block is falling, the trigger objects should
/// report being in contact with either 0 or 1 object.  Any other value is
/// likely an error.
/// </summary>
public class BlockController : MonoBehaviour
{
    public enum Slide { NONE, UP, DOWN, LEFT, RIGHT }

    int power;      //the power of 2 field
    public TMP_Text ValueText;  //reference to TMP Text
    public GameObject Sprite;
    Animator SpriteAnimator;
    SpriteRenderer spriteRenderer;
    public Animator LightningAnimator;
    public Collider2D TriggerTop;
    public Collider2D TriggerBottom;
    public Collider2D TriggerLeft;
    public Collider2D TriggerRight;
    public Color[] BlockColors;

    //defines what type of method you're going to call
    //and declare a variable to hold the method you're going to call.
    public delegate void Callback(BlockController bc, GameObject ObjCollidedWith);
    public Callback callbackFunction;
  
    //blocks will have serial numbers.  This will be used to prioritize block merges

    static int SN = 0;      //class serial number
    int serialNumber;   //current object's S/N

    bool bTerminate;

    //bNewBlock is initially true, but is set to false after the first trigger.
    /*
    bool bNewBlock;
    public bool NewBlock
    {
        set { }
        get { return bNewBlock; }
    }
    */

    //serial number property
    public int SerialNumber
    {
        set { }
        get { return serialNumber; }
    }

    //property to set the block background color
    /*
    public Color BlockColor
    {
        set
        {
            spriteRenderer.color = value;
        }
        get
        {
            return spriteRenderer.color;
        }
    }*/

    //Power of 2 property
    public int Power
    {
        set
        {
            power = value;
            ValueText.text = Value.ToString();

            //set the block color
            spriteRenderer.color = BlockColors[power];
            Debug.Log("Setting block color to " + BlockColors[power]);
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

        bTerminate = false;
        //bNewBlock = true;

        spriteRenderer = Sprite.GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SpriteAnimator = Sprite.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (bTerminate)
        {
            if (SpriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Done"))
            {
                //animation has finished playing
                transform.position = new Vector2(100f, 0f);
                Destroy(gameObject, 10f);    //destroy in 10 seconds
            }
        }
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
        //bNewBlock = false;
    }


    /* To terminate a block, execute the slider animation.  Then, when the animation is done, set the x position to some large value and execute the Destroy() function with a 5-10 second delay.*/
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

        //mark block for termination
        bTerminate = true;
    }
    
}
