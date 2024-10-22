﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class MobileController : MonoBehaviour {

    public static MobileController instance;

    public ClientNetworkDiscovery networkDiscovery;

    public GameObject gamePanel, loadingPanel, lobby, endGame;
    public Text info;
    public Image playerColor;

    static NetworkClient client;

    bool isMaster;

    private void Awake()
    {
        isMaster = false;
        instance = this;
        gamePanel.SetActive(false);
        lobby.SetActive(false);
        loadingPanel.SetActive(true);
        endGame.SetActive(false);
    }

    void Start()
    {
        networkDiscovery.Initialize();
        networkDiscovery.StartAsClient();
        client = new NetworkClient();
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
        client.RegisterHandler(MsgType.Highest + 1, SetPlayerColor);
        client.RegisterHandler(MsgType.Highest + 2, SetMaster);
        client.RegisterHandler(MsgType.Highest + 3, OnGameStarted);
        client.RegisterHandler(MsgType.Highest + 4, OnReturnToLobby);
        client.RegisterHandler(MsgType.Highest + 5, OnGameEnded);
    }

    public bool Connect(string ip, int port, string data)
    {
        ip = ip.TrimStart(':', 'f');
        info.text = "Address: " + ip + " port: " + port + " data: " + data;
        client.Connect(ip, port);
        return client.isConnected;
    }

    void Update()
    {
        if (SimpleInput.GetAxis("Horizontal") != 0 || SimpleInput.GetAxis("Vertical") != 0)
        {
            SendMovementInfo(SimpleInput.GetAxis("Horizontal"), SimpleInput.GetAxis("Vertical"));
            if (Mathf.Abs(SimpleInput.GetAxis("Horizontal")) > 0.85f || Mathf.Abs(SimpleInput.GetAxis("Vertical")) > 0.85f)
                SendBoostInfo(1);
            else
                SendBoostInfo(0);
        }
        if (SimpleInput.GetButton("Fire"))
        {
            SendShootingInfo();
        }
        if (SimpleInput.GetButtonDown("Powerup"))
        {
            SendPowerupInfo();
        }
        //if (SimpleInput.GetButton("Boost"))
        //{
        //    SendBoostInfo(1);
        //} else if (SimpleInput.GetButtonUp("Boost"))
        //{
        //    SendBoostInfo(0);
        //}
    }

    private void OnConnected (NetworkMessage message)
    {
        info.text = "Connected!";
        networkDiscovery.StopBroadcast();
        gamePanel.SetActive(false);
        lobby.SetActive(true);
        loadingPanel.SetActive(false);
        endGame.SetActive(false);
        info.gameObject.SetActive(false);
    }

    private void OnDisconnected (NetworkMessage message)
    {
        networkDiscovery.OnDisconnect();
        networkDiscovery.Initialize();
        networkDiscovery.StartAsClient();
        isMaster = false;
        gamePanel.SetActive(false);
        lobby.SetActive(false);
        loadingPanel.SetActive(true);
        endGame.SetActive(false);
        info.gameObject.SetActive(true);
        info.text = "Disconnected";
        Camera.main.backgroundColor = Color.gray;
    }

    static public void SendMovementInfo(float x, float y)
    {
        if (client.isConnected)
        {
            StringMessage msg = new StringMessage
            {
                value = x + "|" + y
            };
            client.Send(MsgType.Highest + 101, msg);
        }
    }

    static public void SendShootingInfo()
    {
        if (client.isConnected)
        {
            client.Send(MsgType.Highest + 102, new EmptyMessage());
        }
    }

    static public void SendBoostInfo(int boostVal)
    {
        if (client.isConnected)
        {
            IntegerMessage msg = new IntegerMessage
            {
                value = boostVal
            };
            client.Send(MsgType.Highest + 103, msg);
        }
    }

    static public void SendPowerupInfo()
    {
        if (client.isConnected)
        {
            client.Send(MsgType.Highest + 104, new EmptyMessage());
        }
    }

    private void SetMaster(NetworkMessage message)
    {
        isMaster = true;
        foreach(Transform t in lobby.transform)
        {
            t.gameObject.SetActive(true);
        }
    }

    private void SetPlayerColor(NetworkMessage message)
    {
        StringMessage msg = new StringMessage
        {
            value = message.ReadMessage<StringMessage>().value
        };

        string[] rgb = msg.value.Split('|');
        playerColor.color = new Color(Convert.ToSingle(rgb[0]), Convert.ToSingle(rgb[1]), Convert.ToSingle(rgb[2]));
        Camera.main.backgroundColor = new Color(Convert.ToSingle(rgb[0]), Convert.ToSingle(rgb[1]), Convert.ToSingle(rgb[2]));
    }

    private void OnGameStarted (NetworkMessage message)
    {
        gamePanel.SetActive(true);
        lobby.SetActive(false);
        loadingPanel.SetActive(false);
        endGame.SetActive(false);
    }

    private void OnGameEnded (NetworkMessage message)
    {
        gamePanel.SetActive(true);
        lobby.SetActive(false);
        loadingPanel.SetActive(false);
        endGame.SetActive(true);
    }

    private void OnReturnToLobby(NetworkMessage message)
    {
        gamePanel.SetActive(false);
        lobby.SetActive(true);
        loadingPanel.SetActive(false);
        endGame.SetActive(false);
    }

    public void StartGame ()
    {
        if (client.isConnected)
        {
            client.Send(MsgType.Highest + 3, new EmptyMessage());
        }
    }

    public void ReturnToLobby()
    {
        if (client.isConnected)
        {
            client.Send(MsgType.Highest + 4, new EmptyMessage());
        }
    }

    public void PlayAgain()
    {
        if (client.isConnected)
        {
            client.Send(MsgType.Highest + 5, new EmptyMessage());
        }
    }
}
