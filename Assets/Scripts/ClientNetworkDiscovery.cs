using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ClientNetworkDiscovery : NetworkDiscovery {

    bool connected;

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        if (!connected)
            connected = MobileController.instance.Connect(fromAddress, broadcastPort, data);
    }

    public void OnDisconnect ()
    {
        connected = false;
    }

}
