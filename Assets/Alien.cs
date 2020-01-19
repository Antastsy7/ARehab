using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour
{
    public float threshold = 5.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.impulse.y >= threshold)
        {
            Debug.Log("Died");
        }
        Debug.Log(other.impulse);
    }

    public void onmanimupalte()
    {
        Debug.Log("On");
    }
}
