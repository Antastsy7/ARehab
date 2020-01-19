/*
 
    -----------------------
    UDP-Receive (send to)
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
   
   
    // > receive
    // 127.0.0.1 : 8051
   
    // send
    // nc -u 127.0.0.1 8051
 
*/
using UnityEngine;
using System.Collections;
 
using System;
using System.Text;
using System.Net;
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

    public TextMeshPro Text;
    // infos
    public float lastReceivedUDPPacket;
    public string allReceivedUDPPackets=""; // clean up this from time to time!


    public Vector3 left, right;
    
   
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
 
        // status
   
        // ----------------------------
        // Abhören
        // ----------------------------
        // Lokalen Endpunkt definieren (wo Nachrichten empfangen werden).
        // Einen neuen Thread für den Empfang eingehender Nachrichten erstellen.
        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
 
    }
 
    // receive thread
    private  void ReceiveData()
    {
 
        client = new UdpClient(port);
        while (true)
        {
 
            try
            {
                // Bytes empfangen.
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                
                // Bytes mit der UTF8-Kodierung in das Textformat kodieren.
                left = new Vector3(BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4), 
                    BitConverter.ToSingle(data, 8));
                right = new Vector3(BitConverter.ToSingle(data, 12), BitConverter.ToSingle(data, 16), 
                    BitConverter.ToSingle(data, 20));
 
                // Den abgerufenen Text anzeigen.
                // latest UDPpacket
               
                // ....
               
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }
   
    // getLatestUDPPacket
    // cleans up the rest
    
}