using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesContainer : MonoBehaviour {

    public static ParticlesContainer instance;

    public GameObject bombExplosion, shipExplosion, asteroidDestroy, bulletImpact, missleExplosion;

    private void Awake()
    {
        instance = this;
    }
}
