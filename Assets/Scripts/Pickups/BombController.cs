using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour {

    public GameObject owner;
    
    public void SetColor()
    {
        var color = owner.GetComponent<ShipController>().shipColor;
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in renderer.materials)
            {
                if (m.name.Contains("Bomb")) { m.SetColor("_Color", color); }
            }
        }
    }

    public void Explode ()
    {
        Camera.main.GetComponent<ShakeableTransform>().InduceStress(0.5f);

        Collider[] colls = Physics.OverlapSphere(transform.position, 100.0f);
        foreach (Collider c in colls)
        {
            GameObject go = c.gameObject.transform.gameObject;

            if (go.CompareTag("Player") && go != owner)
            {
                go.GetComponent<ShipController>().TakeDamage(50, owner);
            } else if (go.CompareTag("Asteroid"))
            {
                go.GetComponent<AsteroidController>().hitPoints = 0;
            }
        }

        Destroy(gameObject);
    }
}
