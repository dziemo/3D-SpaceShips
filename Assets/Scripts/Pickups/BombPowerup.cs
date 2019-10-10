using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombPowerup : Powerup {

    public GameObject bombPrefab;

    public override void UsePowerup(GameObject owner)
    {
        var particles = Instantiate(ParticlesContainer.instance.bombExplosion, owner.transform.position, owner.transform.rotation);
        var bomb = Instantiate(bombPrefab, owner.transform.position, owner.transform.rotation);
        bomb.GetComponent<BombController>().owner = owner;
        bomb.GetComponent<BombController>().SetColor();
        Destroy(particles, 3.0f);
    }

}
