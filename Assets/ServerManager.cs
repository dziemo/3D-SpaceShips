using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerManager : MonoBehaviour {

    NetworkDiscovery discovery;

    private void OnGUI()
    {
        string ipAddress = Network.player.ipAddress;
        GUI.Box(new Rect(10, Screen.height - 50, 100, 50), ipAddress);
        GUI.Label(new Rect(20, Screen.height - 35, 100, 20), "Status: " + NetworkServer.active);
        GUI.Label(new Rect(20, Screen.height - 20, 100, 20), "Connected: " + (NetworkServer.connections.Count - 1));
    }

    private void Awake()
    {
        discovery = GetComponent<NetworkDiscovery>();
    }

    private void Start()
    {
        NetworkServer.Listen(discovery.broadcastPort);
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        StartBroadcasting();
    }

    public void StartBroadcasting ()
    {
        discovery.Initialize();
        discovery.StartAsServer();
    }
    
    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Dupa");
    }
}
