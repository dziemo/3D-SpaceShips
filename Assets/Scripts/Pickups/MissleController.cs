using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissleController : MonoBehaviour {

    public GameObject owner;
    
    private void Update()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPos.x < -0.1f || screenPos.x > 1.1f || screenPos.y < -0.1f || screenPos.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    public void SetColor()
    {
        var color = owner.GetComponent<ShipController>().shipColor;
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in renderer.materials)
            {
                if (m.name.Contains("Player")) { m.SetColor("_Color", color); }
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != owner && !other.transform.gameObject.CompareTag("Pickup"))
            Explode();
    }


    public void Explode()
    {
        Destroy(Instantiate(ParticlesContainer.instance.missleExplosion, transform.position, Quaternion.identity), 3.0f);
        Camera.main.GetComponent<ShakeableTransform>().InduceStress(0.8f);

        Collider[] colls = Physics.OverlapSphere(transform.position, 50.0f);
        foreach (Collider c in colls)
        {
            GameObject go = c.gameObject.transform.gameObject;

            if (go.CompareTag("Player") && go != owner)
            {
                go.GetComponent<ShipController>().TakeDamage(50, owner);
            }
            else if (go.CompareTag("Asteroid"))
            {
                go.GetComponent<AsteroidController>().hitPoints = 0;
            }
        }
        
        Destroy(gameObject);
    }
}
