using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupController : MonoBehaviour {

    public string PowerupName;
    public Powerup Powerup;

    private void OnTriggerEnter(Collider coll)
    {
        GameObject go = coll.transform.root.gameObject;
        if (go.tag == "Player")
        {  
            if (AddPowerupToPlayer(go))
                Destroy(gameObject);
        }
    }

    public bool AddPowerupToPlayer (GameObject player)
    {
        return player.GetComponent<ShipController>().AddPowerup(Powerup);
    }

}
