using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyController : MonoBehaviour {

    public static LobbyController instance;

    public List<LobbyPlayerInfo> playersInfo = new List<LobbyPlayerInfo>();

    private void Awake()
    {
        instance = this;
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
    }

    public void RemovePlayer(int index)
    {
        LobbyPlayerInfo info = playersInfo[index];
        info.gameObject.SetActive(false);
    }
}
