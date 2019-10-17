using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviour {

    public static ServerManager instance;

    public List<Player> localPlayers = new List<Player>();
    public Dictionary<int, Player> networkPlayers = new Dictionary<int, Player>();
    public List<Color> playersColors;

    NetworkDiscovery discovery;
    bool[] playerSlots = new bool[4];

    private void OnGUI()
    {
        string ipAddress = Network.player.ipAddress;
        GUI.Box(new Rect(10, Screen.height - 50, 100, 50), ipAddress);
        GUI.Label(new Rect(20, Screen.height - 35, 100, 20), "Status: " + NetworkServer.active);
        GUI.Label(new Rect(20, Screen.height - 20, 100, 20), "Connected: " + (NetworkServer.connections.Count - 1));
    }

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(this);
            instance = this;
            discovery = GetComponent<NetworkDiscovery>();
        }
    }

    private void Start()
    {
        NetworkServer.Listen(discovery.broadcastPort);
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        NetworkServer.RegisterHandler(MsgType.Error, OnError);
        NetworkServer.RegisterHandler(MsgType.Highest + 3, OnStartGame);
        NetworkServer.RegisterHandler(MsgType.Highest + 4, OnReturnToLobby);
        NetworkServer.RegisterHandler(MsgType.Highest + 5, OnPlayAgain);
        StartBroadcasting();
    }

    public void StartBroadcasting()
    {
        if (!discovery.running)
        {
            discovery.Initialize();
            discovery.StartAsServer();
        }
    }
    
    public void OnConnected(NetworkMessage netMsg)
    {
        int index = GetFirstOpenSlot();

        Player newPlayer = new Player
        {
            playerName = "Player " + (index + 1),
            playerColor = playersColors[index],
            connectionId = netMsg.conn.connectionId
        };
        localPlayers.Add(newPlayer);
        networkPlayers.Add(netMsg.conn.connectionId, localPlayers[index]);
        LobbyController.instance.AddPlayer(index, newPlayer.playerName, newPlayer.playerColor);
        StringMessage msg = new StringMessage
        {
            value = newPlayer.playerColor.r.ToString() + '|' + newPlayer.playerColor.g.ToString() + '|' + newPlayer.playerColor.b.ToString()
        };
        NetworkServer.SendToClient(netMsg.conn.connectionId, MsgType.Highest + 1, msg);
        playerSlots[index] = true;
    }

    public void OnDisconnected(NetworkMessage netMsg)
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            int localIndex = localPlayers.FindIndex(p => p.connectionId == netMsg.conn.connectionId);
            LobbyController.instance.RemovePlayer(localIndex);
            playerSlots[localIndex] = false;
            localPlayers.Remove(localPlayers.Find(p => p.connectionId == netMsg.conn.connectionId));
            networkPlayers.Remove(netMsg.conn.connectionId);
        }
    }

    public void OnError(NetworkMessage netMsg)
    {
        Debug.Log("Error while connecting");
    }

    public void OnStartGame(NetworkMessage netMsg)
    {
        NetworkServer.SendToAll(MsgType.Highest + 3, new EmptyMessage());
        SceneManager.LoadScene("Game");
        discovery.StopBroadcast();
        NetworkServer.dontListen = true;
    }

    public void OnReturnToLobby(NetworkMessage netMsg)
    {
        NetworkServer.SendToAll(MsgType.Highest + 4, new EmptyMessage());
        SceneManager.LoadScene("Lobby");
        StartBroadcasting();
    }

    public void OnPlayAgain(NetworkMessage netMsg)
    {
        NetworkServer.SendToAll(MsgType.Highest + 3, new EmptyMessage());
        SceneManager.LoadScene("Game");
    }

    int GetFirstOpenSlot ()
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (!playerSlots[i])
                return i;
        }

        return -1;
    }
}
