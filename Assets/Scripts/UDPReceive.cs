using UnityEngine;
using System.Collections;
 
using System;
using System.Text;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using UnityEngine.UI;

public class UDPReceive : MonoBehaviour {
   
    // receiving Thread
    Thread receiveThread;
 
    // udpclient object
    UdpClient client;
 
    // public
    // public string IP = "127.0.0.1"; default local
    public int port; // define > init

    public GameObject Text;
    // infos
    public string lastReceivedUDPPacket="";
    public string allReceivedUDPPackets=""; // clean up this from time to time!

    private bool setup = false;
    // start from shell
    private static void Main()
    {
       UDPReceive receiveObj=new UDPReceive();
       receiveObj.init();
 
        string text="";
        do
        {
             text = Console.ReadLine();
        }
        while(!text.Equals("exit"));
    }
    // start from unity3d
    public void Start()
    {
       
        init();
    }
   
    // OnGUI
    void OnGUI()
    {
        Rect rectObj=new Rect(40,10,200,400);
            GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj,"# UDPReceive\n127.0.0.1 "+port+" #\n"
                    + "shell> nc -u 127.0.0.1 : "+port+" \n"
                    + "\nLast Packet: \n"+ lastReceivedUDPPacket
                    + "\n\nAll Messages: \n"+allReceivedUDPPackets
                ,style);
    }
       
    // init
    private void init()
    {
        // Endpunkt definieren, von dem die Nachrichten gesendet werden.
        print("UDPSend.init()");
       
        // define port
        port = 20201;
 
        // status
        print("Sending to 127.0.0.1 : "+port);
        print("Test-Sending to this Port: nc -u 127.0.0.1  "+port+"");
        client = new UdpClient(port);
   
        // ----------------------------
        // Abhören
        // ----------------------------
        // Lokalen Endpunkt definieren (wo Nachrichten empfangen werden).
        // Einen neuen Thread für den Empfang eingehender Nachrichten erstellen.
        setup = true;

    }
 
    // receive thread
    private  void Update()
    {
        if (setup)
        {
            
            try
            {
                // Bytes empfangen.
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
 
                // Bytes mit der UTF8-Kodierung in das Textformat kodieren.

                //float F = 1 * data[0] + 2 * data[1] + 4 * data[2] + 8 * data[3];
                float f = (float) data[0];
 
                // Den abgerufenen Text anzeigen.
                Debug.Log(f);
                Text.GetComponent<text>().txt = f.ToString();

                // latest UDPpacket
                //lastReceivedUDPPacket=text;

                // ....
                //allReceivedUDPPackets=allReceivedUDPPackets+text;

            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }
   
    // getLatestUDPPacket
    // cleans up the rest
    public string getLatestUDPPacket()
    {
        allReceivedUDPPackets="";
        return lastReceivedUDPPacket;
    }
}


