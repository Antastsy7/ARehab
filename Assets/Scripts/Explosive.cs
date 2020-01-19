using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    public GameObject eplosioneffect, gameManager;
    public float timer = 0.5f;
    public AudioSource sound;
    private bool ondestroy;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ondestroy)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                Destroy(gameObject);
            }
        }   
    }

    public void Explode()
    {
        if (!ondestroy){
            Instantiate(eplosioneffect, transform.position, transform.rotation);
            ondestroy = true;
            gameManager.SendMessage("DestoryUFO");
            sound.Play();
        }
        
    }
}
