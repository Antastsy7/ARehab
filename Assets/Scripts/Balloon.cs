using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    private Vector3 v;
    private bool away;

    public float decay = 0.96f;
    // Start is called before the first frame update
    void Start()
    {
        //GetComponent<Rigidbody>().AddForce(Random.insideUnitCircle/1000);
    }

    // Update is called once per frame
    void Update()
    {
        if (away)
        {
            transform.position += v;
            v *= decay;
        }
    }

    public void BlowAway()
    {
        Debug.Log("BlowAway");
        away = true;
        v = transform.position.normalized/10;
    }
    
    
}
