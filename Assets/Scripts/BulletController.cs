using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {

    public GameObject owner;

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
                coll.GetComponent<ShipController>().TakeDamage(10);
                var hit = Instantiate(ServerController.instance.hitParticle, transform.position, transform.rotation);
                Destroy(hit, hit.GetComponent<ParticleSystem>().main.startLifetime.constant);
                Destroy(gameObject);
            }
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
