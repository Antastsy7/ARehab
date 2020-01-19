using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    public TextMeshPro text;
    public GameObject reciever, left, right;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 l = reciever.GetComponent<UDPReceive>().left/1000.0f;
        Vector3 r = reciever.GetComponent<UDPReceive>().right/1000.0f;
        Debug.Log(l);
        Debug.Log(r);
        left.transform.position = l + new Vector3(0, -0.94f, 0);
        right.transform.position = r + new Vector3(0, -0.94f, 0);
    }
}
