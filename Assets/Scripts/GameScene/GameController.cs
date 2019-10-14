using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking;
using System;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public static GameController instance;

    public EndPanelController endPanel;

    public GameObject ship;
    public GameObject playersParent;
    public Transform cameraRig;
    public List<GameObject> playerIntefaces = new List<GameObject>();
    public List<GameObject> playerPointers = new List<GameObject>();
    public Dictionary<int, ShipController> shipControllers = new Dictionary<int, ShipController>();
    public List<Transform> spawns;
    public List<Color> playersColors;
    public List<GameObject> powerUps = new List<GameObject>();
    public List<GameObject> asteroidPrefabs = new List<GameObject>();
    public List<Player> localPlayers; //do wywalenia
    public Dictionary<int, Player> networkPlayers; //do wywalenia

    public List<GameObject> playersUISets = new List<GameObject>();
    public List<Text> killsText = new List<Text>();

    public Text countdownText;

    Camera cam;

    private void Awake()
    {
        if (!ServerManager.instance)
        {
            SceneManager.LoadScene("Lobby");
            return;
        }
        instance = this;
        endPanel.gameObject.SetActive(false);
        localPlayers = ServerManager.instance.localPlayers;
        networkPlayers = ServerManager.instance.networkPlayers;

        foreach (GameObject set in playersUISets)
        {
            set.SetActive(false);
        }
        foreach (GameObject i in playerIntefaces)
        {
            i.SetActive(false);
        }

        cam = Camera.main;
    }

    void Start()
    {
        CreatePlayerShips();
        NetworkServer.RegisterHandler(MsgType.Highest + 101, ServerRecieveMovementVector);
        NetworkServer.RegisterHandler(MsgType.Highest + 102, ServerRecieveShootingVector);
        NetworkServer.RegisterHandler(MsgType.Highest + 103, ServerRecieveBoost);
        NetworkServer.RegisterHandler(MsgType.Highest + 104, ServerRecievePowerup);

        Time.timeScale = 0.0f;

        InvokeRepeating("AsteroidSpawn", 0.0f, 15.0f);
        InvokeRepeating("PowerupSpawn", 6.0f, 5.0f);
        StartCoroutine(StartGameCountdown());
    }

    private void Update()
    {
        Vector3 center = Vector3.zero;

        for (int i = 0; i < shipControllers.Count; i++) { center += playersParent.transform.GetChild(i).position; }

        center /= shipControllers.Count;

        float cameraRotationX = 90 - (center.z / 50.0f);
        cameraRotationX = Mathf.Clamp(cameraRotationX, 85.0f, 95.0f);

        cameraRig.eulerAngles = Vector3.Lerp(cameraRig.eulerAngles, new Vector3(cameraRotationX, 0.0f, 0.0f), Time.deltaTime * 1.5f);
    }

    private void CreatePlayerShips()
    {
        for (int i = 0; i < localPlayers.Count; i++)
        {
            Player p = localPlayers[i];
            p.lives = 3;
            var newShip = Instantiate(ship, spawns[i].position, Quaternion.identity);
            p.playerShip = newShip;
            p.playerLives = playersUISets[i];
            ShipController shipController = newShip.GetComponent<ShipController>();
            shipController.enabled = false;
            shipController.shipColor = p.playerColor;
            shipController.SetColor();
            playersUISets[i].SetActive(true);
            shipControllers.Add(p.connectionId, shipController);
            playerIntefaces[i].GetComponent<PlayerGameInterface>().SetShipController(shipController);
            playerPointers[i].GetComponent<PlayerPointer>().SetTarget(shipController);
            newShip.transform.SetParent(playersParent.transform);
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

    private void ServerRecieveBoost(NetworkMessage message)
    {
        IntegerMessage msg = new IntegerMessage
        {
            value = message.ReadMessage<IntegerMessage>().value
        };

        shipControllers[message.conn.connectionId].Boost(msg.value);
    }

    private void ServerRecievePowerup(NetworkMessage message)
    {
        Debug.Log("Powerup recieved");
        
        shipControllers[message.conn.connectionId].UsePowerup();
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
            StartCoroutine(RespawnCountdown(playerObject));
        }
        else
            Debug.Log("Game over for " + playerObject.name);

        if (CheckPlayerLives())
        {
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
        endPanel.DisplayEndPanel(LastPlayer());
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
            var asteroid = Instantiate(asteroidPrefabs[Random.Range(0, asteroidPrefabs.Count)], Vector3.zero, Quaternion.identity);
            asteroid.transform.position += new Vector3(0, 0, 800);
            asteroid.transform.RotateAround(Vector3.zero, transform.up, Random.Range(0, 359.0f));
            asteroid.transform.forward = -asteroid.transform.position;
            asteroid.transform.Rotate(new Vector3(0, Random.Range(-30, 30), 0), Space.Self);
            asteroid.GetComponent<Rigidbody>().velocity = asteroid.transform.forward * 100.0f * Random.Range(0.75f, 1.0f);
        }
    }

    public void PowerupSpawn()
    {
        var powerup = Instantiate(powerUps[Random.Range(0, powerUps.Count)], new Vector3 (Random.Range(-400, 401), -10, Random.Range(-250, 251)), Quaternion.identity);
    }
}
