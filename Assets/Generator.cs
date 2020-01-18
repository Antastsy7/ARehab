using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public GameObject prefabs;
    public float threshold = 3.0f;
    private float time;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time > threshold)
        {
            Instantiate(prefabs,(Random.insideUnitSphere + Vector3.one)/2,Quaternion.identity);
            time = 0;
        }
    }
}
