using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking;
using System;

public class ServerController : MonoBehaviour {

    public static ServerController instance;

    public GameObject ship;
    public Dictionary<int, ShipController> shipControllers = new Dictionary<int, ShipController>();
    public List<Transform> spawns;
    public List<Color> playersColors;
    public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    public GameObject explosionParticle, hitParticle;

    private void OnGUI()
    {
        string ipAddress = Network.player.ipAddress;
        GUI.Box(new Rect(10, Screen.height - 50, 100, 50), ipAddress);
        GUI.Label(new Rect(20, Screen.height - 35, 100, 20), "Status: " + NetworkServer.active);
        GUI.Label(new Rect(20, Screen.height - 20, 100, 20), "Connected: " + (NetworkServer.connections.Count - 1));
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        NetworkServer.Listen(25000);
        NetworkServer.RegisterHandler(888, ServerRecieveMovementVector);
        NetworkServer.RegisterHandler(999, ServerRecieveShootingVector);
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        NetworkServer.RegisterHandler(MsgType.Error, OnError);
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        var newPlayer = Instantiate(ship, spawns[players.Count].position, Quaternion.identity);
        players.Add(netMsg.conn.connectionId, newPlayer);

        shipControllers.Add(netMsg.conn.connectionId, newPlayer.GetComponent<ShipController>());

        newPlayer.GetComponent<ShipController>().shipColor = playersColors[players.Count];

        newPlayer.GetComponent<ShipController>().SetColor();

        Debug.Log("Client Connected");
        if (players.Count == 4)
            NetworkServer.dontListen = true;
    }

    public void OnDisconnected(NetworkMessage netMsg)
    {
        Debug.Log("Disconnected");
    }

    public void OnError(NetworkMessage netMsg)
    {
        Debug.Log("Error while connecting");
    }

    private void ServerRecieveMovementVector(NetworkMessage message)
    {
        StringMessage msg = new StringMessage
        {
            value = message.ReadMessage<StringMessage>().value
        };

        string[] deltas = msg.value.Split('|');

        shipControllers[message.conn.connectionId].Move(Convert.ToSingle(deltas[0]), Convert.ToSingle(deltas[1]));
    }

    private void ServerRecieveShootingVector(NetworkMessage message)
    {
        shipControllers[message.conn.connectionId].Shoot();
    }
}
