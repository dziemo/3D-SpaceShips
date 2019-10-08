using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidController : MonoBehaviour {

    public int hitPoints = 2;

    bool enteredScreen = false;

    private void Update()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);

        if (!enteredScreen)
        {
            if (screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1)
            {
                enteredScreen = true;
            }
        } else
        {
            if (screenPos.x < -0.1f || screenPos.x > 1.1f || screenPos.y < -0.1f || screenPos.y > 1.1f)
            {
                Destroy(gameObject);
            }
        }

        if (hitPoints <= 0)
        {
            var particle = Instantiate(ParticlesContainer.instance.asteroidDestroy, transform.position, Quaternion.identity);
            Destroy(particle, 2.0f);
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.root.tag == "Player")
        {
            ShipController shipController = collision.gameObject.GetComponent<ShipController>();
            shipController.DisableShip(gameObject, 1.0f);
            shipController.TakeDamage(10);
            var particle = Instantiate(ParticlesContainer.instance.asteroidDestroy, transform.position, Quaternion.identity);
            Destroy(particle, 2.0f);
            Destroy(gameObject);
        }
    }

}
