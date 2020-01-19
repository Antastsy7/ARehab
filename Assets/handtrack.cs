using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using System.Net;
using System.Net.Sockets;

public class handtrack : MonoBehaviour
{
    private GameObject left, right, l, r;
    IPEndPoint remoteEndPoint;
    UdpClient client;
    
    Byte[] b;
    // Start is called before the first frame update
    void Start()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse("192.168.137.1"), 20202);
        client = new UdpClient();
        
    }

    // Update is called once per frame
    void Update()
    {
        left = GameObject.Find("Left_HandLeft(Clone)");
        right = GameObject.Find("Right_HandRight(Clone)");
        if (left && right)
        {
            l = left.transform.Find("Wrist Proxy Transform").gameObject;
            r = right.transform.Find("Wrist Proxy Transform").gameObject;
            b = new byte[24];
            BitConverter.GetBytes(l.transform.position.x).CopyTo(b, 0);
            BitConverter.GetBytes(l.transform.position.y).CopyTo(b, 4);
            BitConverter.GetBytes(l.transform.position.z).CopyTo(b, 8);
            BitConverter.GetBytes(r.transform.position.x).CopyTo(b, 12);
            BitConverter.GetBytes(r.transform.position.y).CopyTo(b, 16);
            BitConverter.GetBytes(r.transform.position.z).CopyTo(b, 20);
            client.Send(b, b.Length, remoteEndPoint);
            Debug.Log(b.Length);
        }
        /*if (left && right)
        {
            Debug.Log(right.transform.position);
        }*/
        
    }
}
