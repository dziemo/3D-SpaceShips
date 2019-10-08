using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {

    public GameObject owner;
    public int damage;

    private void Update()
    {
        Vector3 yPosCorrection = transform.position;
        yPosCorrection.y = 0;
        transform.position = yPosCorrection;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject coll = other.transform.root.gameObject;

        if (coll.tag == "Player")
        {
            if (coll != owner)
            {
                var hit = Instantiate(ParticlesContainer.instance.bulletImpact, transform.position, transform.rotation);
                Destroy(hit, hit.GetComponent<ParticleSystem>().main.startLifetime.constant);
                Destroy(gameObject);
                coll.GetComponent<ShipController>().TakeDamage(damage, owner);
            }
        } else if (coll.tag =="Asteroid")
        {
            var hit = Instantiate(ParticlesContainer.instance.bulletImpact, transform.position, transform.rotation);
            Destroy(hit, hit.GetComponent<ParticleSystem>().main.startLifetime.constant);
            coll.GetComponent<AsteroidController>().hitPoints--;
            Destroy(gameObject);
        }
    }

    public void SetColor (Color color)
    {
        color.a = 255;
        GetComponent<Renderer>().sharedMaterial.SetColor("_Color", color);
        GetComponent<TrailRenderer>().startColor = color;
        GetComponent<TrailRenderer>().colorGradient.alphaKeys[0].alpha = 255.0f;
    }

}
