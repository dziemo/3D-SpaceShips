using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MisslePowerup : Powerup {

    public GameObject misslePrefab;

    public override void UsePowerup(GameObject owner)
    {
        var missle = Instantiate(misslePrefab, owner.transform.position - owner.transform.forward * 40.0f, owner.transform.rotation);
        missle.GetComponent<MissleController>().owner = owner;
        missle.GetComponent<MissleController>().SetColor();
        missle.GetComponent<Rigidbody>().AddForce(-owner.transform.forward * 400.0f, ForceMode.VelocityChange);
    }

}
