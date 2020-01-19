using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour
{
    public float threshold = 5.0f;
    private bool left;
    private GameObject gamemanager;
    // Start is called before the first frame update
    void Start()
    {
        gamemanager = GameObject.Find("GameManager");
        Debug.Log(gamemanager);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void gotcha()
    {
        GameObject.Find("Gotcha_sound").GetComponent<AudioSource>().Play();
    }

    private void OnCollisionEnter(Collision other)
    {
        /*if (other.impulse.y >= threshold)
        {
            Debug.Log("Died");
        }
        Debug.Log(other.impulse);*/
        if (left)
        {
            gamemanager.SendMessage("KilledAlien");
            Destroy(gameObject);
        }
    }

    public void offmanimupalte()
    {
        Debug.Log("On");
        GameObject.Find("Crying").GetComponent<AudioSource>().Play();
        left = true;
    }
}
