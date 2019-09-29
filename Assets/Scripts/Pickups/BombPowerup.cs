using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombPowerup : Powerup {

    public GameObject bombPrefab;

    public override void UsePowerup(GameObject owner)
    {
        var bomb = Instantiate(bombPrefab, owner.transform.position, owner.transform.rotation);
        bomb.GetComponent<BombController>().owner = owner;
        bomb.GetComponent<BombController>().SetColor();
    }

}
