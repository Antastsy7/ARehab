using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnalien : MonoBehaviour
{
    public GameObject alineprefab;
    private bool spawned;
    private float timer = 2.0f;
    public float threshold;
    public Vector3 speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        transform.position += speed;
        if (!spawned && transform.position.y <= threshold )
        {
            Instantiate(alineprefab, transform.position, Quaternion.identity);
            spawned = true;
            speed = -2.0f * speed;
        }
        else if (spawned && transform.position.y > 10)
        {
            Destroy(gameObject);
        }
    }
}
