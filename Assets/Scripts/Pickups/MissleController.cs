﻿using System.Collections;
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
        if (other.gameObject != owner && other.transform.root.gameObject.tag != "Pickup")
            Explode();
    }


    public void Explode()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, 100.0f);
        foreach (Collider c in colls)
        {
            GameObject go = c.gameObject.transform.root.gameObject;

            if (go.tag == "Player" && go != owner)
            {
                go.GetComponent<ShipController>().TakeDamage(50, owner);
            }
            else if (go.tag == "Asteroid")
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