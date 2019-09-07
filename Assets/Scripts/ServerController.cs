using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking;
using System;
using Random = UnityEngine.Random;

public class ServerController : MonoBehaviour {

    public static ServerController instance;

    public GameObject ship;
    public Dictionary<int, ShipController> shipControllers = new Dictionary<int, ShipController>();
    public List<Transform> spawns;
    public List<Color> playersColors;
    public List<Player> localPlayers = new List<Player>();
    public Dictionary<int, Player> networkPlayers = new Dictionary<int, Player>();

    public List<GameObject> playersUISets = new List<GameObject>();
    public List<Text> killsText = new List<Text>();

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
        foreach (GameObject set in playersUISets)
        {
            set.SetActive(false);
        }
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
        Player newPlayer = new Player();
        var newShip = Instantiate(ship, spawns[networkPlayers.Count].position, Quaternion.identity);
        newPlayer.lives = 3;
        newPlayer.playerName = "Player " + (networkPlayers.Count + 1);
        newPlayer.playerShip = newShip;
        newShip.GetComponent<ShipController>().shipColor = playersColors[networkPlayers.Count];
        newShip.GetComponent<ShipController>().SetColor();
        playersUISets[networkPlayers.Count].SetActive(true);
        networkPlayers.Add(netMsg.conn.connectionId, newPlayer);

        localPlayers.Add(newPlayer);

        shipControllers.Add(netMsg.conn.connectionId, newShip.GetComponent<ShipController>());
        
        Debug.Log("Client Connected");
        if (networkPlayers.Count == 4)
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

        ShipController shipController = shipControllers[message.conn.connectionId];

        if (shipController.gameObject)
        {
            shipControllers[message.conn.connectionId].Move(Convert.ToSingle(deltas[0]), Convert.ToSingle(deltas[1]));
        }
    }

    private void ServerRecieveShootingVector(NetworkMessage message)
    {
        shipControllers[message.conn.connectionId].Shoot();
    }

    public void AddKill (GameObject killer)
    {
        Player player = localPlayers.Find(p => p.playerShip == killer);
        player.kills++;
        player.score += 100;
        UpdateUI();
    }

    public void AddDamageStats(GameObject damager, int amount)
    {
        Player player = localPlayers.Find(p => p.playerShip == damager);
        player.score += amount;
    }

    public void Respawn (GameObject playerObject)
    {
        playerObject.SetActive(false);
        StartCoroutine(RespawnCountdown(playerObject));
    }

    IEnumerator RespawnCountdown (GameObject playerObject)
    {
        yield return new WaitForSeconds(3.0f);
        playerObject.transform.position = spawns[Random.Range(0, 4)].transform.position;
        playerObject.GetComponent<ShipController>().ResetShip();
        playerObject.SetActive(true);
    }

    private void UpdateUI ()
    {
        for (int i = 0; i < localPlayers.Count; i++)
        {
            killsText[i].text = localPlayers[i].kills.ToString();
        }
    } 
}
