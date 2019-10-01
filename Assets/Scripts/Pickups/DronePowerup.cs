using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronePowerup : Powerup {

    public GameObject dronePrefab;

    public override void UsePowerup(GameObject owner)
    {
        var drone = Instantiate(dronePrefab, owner.transform.position + Vector3.right * 40.0f, owner.transform.rotation);
        drone.GetComponent<DroneController>().owner = owner;
        drone.GetComponent<DroneController>().SetColor();
        Destroy(drone, 9.0f);
    }

}
