using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlockController : MonoBehaviour
{

    int power;      //the power of 2 field
    public TMP_Text ValueText;  //reference to TMP Text

    //defines what type of method you're going to call.
    public delegate void Callback(BlockController bc);

    //declare a variable to hold the method you're going to call.
    public Callback callbackFunction;

    //blocks will have serial numbers.  This will be used to prioritize block merges

    static int SN = 0;      //class serial number

    int serialNumber;   //current object's S/N

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

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (callbackFunction != null)
            callbackFunction(this);
    }

}
