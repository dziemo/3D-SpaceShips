using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking;
using System;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

    public static GameController instance;

    public EndPanelController endPanel;

    public GameObject ship, asteroidPrefab;
    public Dictionary<int, ShipController> shipControllers = new Dictionary<int, ShipController>();
    public List<Transform> spawns;
    public List<Color> playersColors;
    public List<Player> localPlayers = new List<Player>();
    public Dictionary<int, Player> networkPlayers = new Dictionary<int, Player>();

    public List<GameObject> playersUISets = new List<GameObject>();
    public List<Text> killsText = new List<Text>();

    public Text countdownText;

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
        NetworkServer.RegisterHandler(MsgType.Highest + 1, ServerRecieveMovementVector);
        NetworkServer.RegisterHandler(MsgType.Highest + 2, ServerRecieveShootingVector);
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
        NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        NetworkServer.RegisterHandler(MsgType.Error, OnError);

        Time.timeScale = 0.0f;

        InvokeRepeating("AsteroidSpawn", 0.0f, 15.0f);
    }

    public void OnConnected(NetworkMessage netMsg)
    {
        Player newPlayer = new Player();
        var newShip = Instantiate(ship, spawns[networkPlayers.Count].position, Quaternion.identity);
        newPlayer.lives = 3;
        newPlayer.playerName = "Player " + (networkPlayers.Count + 1);
        newPlayer.playerShip = newShip;
        newPlayer.playerLives = playersUISets[networkPlayers.Count].transform.Find("Lives").gameObject;

        Color playerColor = playersColors[networkPlayers.Count];

        ShipController shipController = newShip.GetComponent<ShipController>();
        shipController.enabled = false;
        shipController.shipColor = playerColor;
        shipController.SetColor();
        playersUISets[networkPlayers.Count].SetActive(true);
        StringMessage msg = new StringMessage
        {
            value = playerColor.r.ToString() + '|' + playerColor.g.ToString() + '|' + playerColor.b.ToString()
        };
        Debug.Log("Color string:  " + msg.value);
        NetworkServer.SendToClient(netMsg.conn.connectionId, MsgType.Highest + 1, msg);

        networkPlayers.Add(netMsg.conn.connectionId, newPlayer);

        localPlayers.Add(newPlayer);

        shipControllers.Add(netMsg.conn.connectionId, shipController);
        
        Debug.Log("Client Connected");
        if (networkPlayers.Count == 4)
            NetworkServer.dontListen = true;
        if (networkPlayers.Count == 1)
        StartCoroutine(StartGameCountdown());
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
        if (localPlayers.Find(p => p.playerShip == playerObject).SubtractLife() > 0)
        {
            if (CheckPlayerLives())
            {
                StartCoroutine(RespawnCountdown(playerObject));
            } else
            {
                EndGame();
            }
        }
        else
            Debug.Log("Game over for " + playerObject.name);
    }

    public bool CheckPlayerLives ()
    {
        int playersAlive = 0;
        foreach (Player p in localPlayers)
        {
            if (p.lives > 0)
                playersAlive++;
        }
        if (playersAlive > 1)
        {
            return true;
        } else
        {
            return false;
        }
    }

    public void EndGame ()
    {
        StopAllCoroutines();
        //Display which player won
        endPanel.DisplayEndPanel(LastPlayer());
        //Display scoreboard
        //Display button for lobby/replay
        Debug.Log("Game ended");
    }

    private string LastPlayer()
    {
        string pName = "";

        foreach (Player p in localPlayers)
        {
            if (p.lives > 0)
            {
                pName = p.playerName;
            }
        }

        return pName;
    }

    IEnumerator RespawnCountdown (GameObject playerObject)
    {
        yield return new WaitForSeconds(3.0f);
        playerObject.transform.position = spawns[Random.Range(0, 4)].transform.position;
        playerObject.GetComponent<ShipController>().ResetShip();
        playerObject.SetActive(true);
    }

    IEnumerator StartGameCountdown ()
    {
        countdownText.gameObject.SetActive(true);
        int counter = 5;
        countdownText.text = counter.ToString();
        while (counter > 0)
        {
            yield return WaitForUnscaledSeconds(1.0f);
            counter--;
            countdownText.text = counter.ToString();
        }
        Time.timeScale = 1.0f;
        countdownText.text = "GO!";
        yield return WaitForUnscaledSeconds(0.5f);
        countdownText.gameObject.SetActive(false);

        NetworkServer.dontListen = true;

        foreach (ShipController controller in shipControllers.Values)
        {
            controller.enabled = true;
        }
    }

    IEnumerator WaitForUnscaledSeconds(float dur)
    {
        var cur = 0f;
        while (cur < dur)
        {
            yield return null;
            cur += Time.unscaledDeltaTime;
        }
    }

    private void UpdateUI ()
    {
        for (int i = 0; i < localPlayers.Count; i++)
        {
            killsText[i].text = localPlayers[i].kills.ToString();
        }
    }


    public void AsteroidSpawn()
    {
        for (int i = 0; i < Random.Range(4, 9); i++)
        {
            var asteroid = Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);
            asteroid.transform.position += new Vector3(0, 0, 800);
            asteroid.transform.RotateAround(Vector3.zero, transform.up, Random.Range(0, 359.0f));
            asteroid.transform.forward = -asteroid.transform.position;
            asteroid.transform.Rotate(new Vector3(0, Random.Range(-30, 30), 0), Space.Self);
            asteroid.GetComponent<Rigidbody>().velocity = asteroid.transform.forward * 100.0f * Random.Range(0.75f, 1.0f);
        }
    }
}
