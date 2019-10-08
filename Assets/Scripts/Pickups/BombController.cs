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
            GameObject go = c.gameObject.transform.root.gameObject;

            if (go.tag == "Player" && go != owner)
            {
                go.GetComponent<ShipController>().TakeDamage(50, owner);
            } else if (go.tag == "Asteroid")
            {
                go.GetComponent<AsteroidController>().hitPoints = 0;
            }
        }

        var explosionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosionSphere.transform.position = transform.position;
        Destroy(explosionSphere.GetComponent<SphereCollider>());
        Vector3 scale = explosionSphere.transform.localScale;
        scale *= 200.0f;
        explosionSphere.transform.localScale = scale;

        Destroy(explosionSphere, 0.25f);

        Destroy(gameObject);
    }
}
