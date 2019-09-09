using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MobileController : MonoBehaviour {

    static NetworkClient client;
    static int id = -1;
    private void OnGUI()
    {
        string ipAddress = Network.player.ipAddress;
        GUI.Box(new Rect(10, Screen.height - 50, 100, 50), ipAddress);
        GUI.Label(new Rect(20, Screen.height - 30, 100, 20), "Status: " + client.isConnected);

        if (!client.isConnected)
        {
            if (GUI.Button(new Rect(30, 30, 200, 150), "Connect"))
            {
                Connect();
            }
        }
    }

    void Start()
    {
        client = new NetworkClient();
        NetworkServer.RegisterHandler(MsgType.Highest + 1, SetPlayerColor);
    }

    public void Connect()
    {
        client.Connect("192.168.1.64", 25000);
    }

    void Update()
    {

        if (SimpleInput.GetAxis("Horizontal") != 0)
            SendMovementInfo(SimpleInput.GetAxis("Horizontal"), SimpleInput.GetAxis("Vertical"));
        if (SimpleInput.GetAxis("Fire") > 0)
            SendShootingInfo();

    }

    static public void SendMovementInfo(float x, float y)
    {
        if (client.isConnected)
        {
            StringMessage msg = new StringMessage
            {
                value = x + "|" + y
            };
            client.Send(MsgType.Highest + 1, msg);
        }
    }

    static public void SendShootingInfo()
    {
        if (client.isConnected)
        {
            client.Send(MsgType.Highest + 2, new EmptyMessage());
        }
    }
    
    private void SetPlayerColor(NetworkMessage message)
    {
        StringMessage msg = new StringMessage
        {
            value = message.ReadMessage<StringMessage>().value
        };

        string[] rgb = msg.value.Split('|');

        Camera.main.backgroundColor = new Color(Convert.ToSingle(rgb[0]), Convert.ToSingle(rgb[1]), Convert.ToSingle(rgb[2]));
    }
}
