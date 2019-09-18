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

    public List<Player> localPlayers; //do wywalenia
    public Dictionary<int, Player> networkPlayers; //do wywalenia

    public List<GameObject> playersUISets = new List<GameObject>();
    public List<Text> killsText = new List<Text>();

    public Text countdownText;

    public GameObject explosionParticle, hitParticle;

    private void Awake()
    {
        instance = this;
        endPanel.gameObject.SetActive(false);
        localPlayers = ServerManager.instance.localPlayers;
        networkPlayers = ServerManager.instance.networkPlayers;

        foreach (GameObject set in playersUISets)
        {
            set.SetActive(false);
        }
    }

    void Start()
    {
        CreatePlayerShips();
        NetworkServer.RegisterHandler(MsgType.Highest + 1, ServerRecieveMovementVector);
        NetworkServer.RegisterHandler(MsgType.Highest + 2, ServerRecieveShootingVector);

        Time.timeScale = 0.0f;

        InvokeRepeating("AsteroidSpawn", 0.0f, 15.0f);
        StartCoroutine(StartGameCountdown());
    }

    private void CreatePlayerShips()
    {
        for (int i = 0; i < localPlayers.Count; i++)
        {
            Player p = localPlayers[i];
            p.lives = 1;
            var newShip = Instantiate(ship, spawns[i].position, Quaternion.identity);
            p.playerShip = newShip;
            p.playerLives = playersUISets[i];
            ShipController shipController = newShip.GetComponent<ShipController>();
            shipController.enabled = false;
            shipController.shipColor = p.playerColor;
            shipController.SetColor();
            playersUISets[i].SetActive(true);
            shipControllers.Add(p.connectionId, shipController);
        }
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
        }
        else
            Debug.Log("Game over for " + playerObject.name);

        if (CheckPlayerLives())
        {
            StartCoroutine(RespawnCountdown(playerObject));
        }
        else
        {
            EndGame();
        }
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
        NetworkServer.SendToAll(MsgType.Highest + 5, new EmptyMessage());
        //Display which player won
        endPanel.DisplayEndPanel(LastPlayer());
        //Display scoreboard
        //Display button for lobby/replay
        Debug.Log("Game ended");
        Time.timeScale = 0.0f;
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
        countdownText.text = "GO!";
        yield return WaitForUnscaledSeconds(0.5f);
        countdownText.gameObject.SetActive(false);
        
        foreach (ShipController controller in shipControllers.Values)
        {
            controller.enabled = true;
        }

        Time.timeScale = 1.0f;
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
