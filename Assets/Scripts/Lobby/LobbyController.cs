using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyController : MonoBehaviour {

    public static LobbyController instance;

    public List<LobbyPlayerInfo> playersInfo = new List<LobbyPlayerInfo>();
    public List<GameObject> playersPlaceholders = new List<GameObject>();
    public List<GameObject> lobbySpaceship = new List<GameObject>();

    private void Awake()
    {
        instance = this;
        foreach(GameObject p in playersPlaceholders)
        {
            p.SetActive(true);
        }

        foreach(LobbyPlayerInfo i in playersInfo)
        {
            i.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        List<Player> players = ServerManager.instance.localPlayers;

        for (int i = 0; i < players.Count; i++)
        {
            AddPlayer(i, players[i].playerName, players[i].playerColor);
        }
    }

    public void AddPlayer (int index, string name, Color color)
    {
        LobbyPlayerInfo info = playersInfo[index];
        info.gameObject.SetActive(true);
        info.playerName.text = name;
        info.playerColor.color = color;
        playersPlaceholders[index].SetActive(false);
        SetColor(color, lobbySpaceship[index]);
    }

    public void RemovePlayer(int index)
    {
        LobbyPlayerInfo info = playersInfo[index];
        info.gameObject.SetActive(false);
    }

    public void SetColor(Color color, GameObject ship)
    {
        Color darkerColor = new Color(color.r * 0.85f, color.g * 0.85f, color.b * 0.85f);

        foreach (Renderer renderer in ship.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in renderer.materials)
            {
                if (m.name.Contains("ship")) { m.SetColor("_Color", color); }
                if (m.name.Contains("shipDark")) { m.SetColor("_Color", darkerColor); }
            }
        }
    }
}
