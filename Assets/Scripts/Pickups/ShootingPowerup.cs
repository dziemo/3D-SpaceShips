using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingPowerup : Powerup
{
    public override void UsePowerup(GameObject owner)
    {
        owner.GetComponent<ShipController>().ChangeFireRate(0.5f, 5.0f);
    }
}
