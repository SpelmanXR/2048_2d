using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlockController : MonoBehaviour
{
    public enum Slide { NONE, UP, DOWN, LEFT, RIGHT }

    int power;      //the power of 2 field
    public TMP_Text ValueText;  //reference to TMP Text
    public Animator SpriteAnimator;
    public Animator LightningAnimator;

    //defines what type of method you're going to call.
    public delegate void Callback(BlockController bc);

    //declare a variable to hold the method you're going to call.
    public Callback callbackFunction;
  
    //blocks will have serial numbers.  This will be used to prioritize block merges

    static int SN = 0;      //class serial number

    int serialNumber;   //current object's S/N

    bool bTerminate = false;        //set to true to self-destruct
    const float TERMINATION_DELAY_SEC = 1f;     //time for the termination animation to run
    float TerminationTime;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        bTerminate = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (bTerminate)
        {
            //wait for slider animation to end
            //if (SpriteAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.999f)
            if (Time.time > TerminationTime)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (callbackFunction != null)
            callbackFunction(this);
    }

    public void Terminate(Slide direction)
    {
        bTerminate = true;
        TerminationTime = Time.time + TERMINATION_DELAY_SEC;

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
