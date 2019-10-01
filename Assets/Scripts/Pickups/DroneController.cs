using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour {
    
    public GameObject owner;
    public Transform firePos;

    float angle = 3.0f, lastShot = 0.0f;
    Vector3 shootTowards = Vector3.forward;

    public void SetColor()
    {
        var color = owner.GetComponent<ShipController>().shipColor;
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in renderer.materials)
            {
                if (m.name.Contains("Color")) { m.SetColor("_Color", color); }
            }
        }
    }

    private void FixedUpdate()
    {
        angle += 1.0f;
        transform.position = owner.transform.position + Vector3.right * 40.0f;
        transform.RotateAround(owner.transform.position, owner.transform.up, angle);
        
        Collider[] colls = Physics.OverlapSphere(owner.transform.position, 500.0f);

        shootTowards = transform.position - owner.transform.position;
        if (lastShot <= 0.0f)
        {
            foreach (Collider c in colls)
            {
                GameObject go = c.gameObject.transform.root.gameObject;

                if (go.tag == "Player" && go != owner && Vector3.Distance(owner.transform.position, go.transform.position) < Vector3.Distance(owner.transform.position, shootTowards))
                {
                    shootTowards = go.transform.position;
                }
            }

            if (shootTowards != transform.position - owner.transform.position)
            {
                Shoot(shootTowards);
            }
        } else
        {
            lastShot -= Time.deltaTime;
        }

        transform.forward = shootTowards;
    }

    private void Shoot (Vector3 position)
    {
        ShipController controller = owner.GetComponent<ShipController>();

        transform.forward = (position - transform.position).normalized;
        var projectile = Instantiate(controller.bulletPrefab, firePos.position, firePos.rotation);
        BulletController bulletController = projectile.AddComponent<BulletController>();
        bulletController.owner = owner;
        bulletController.SetColor(controller.shipColor);
        bulletController.damage = controller.damage;
        projectile.GetComponent<TrailRenderer>().time = 0.15f;
        projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * controller.bulletSpeed / 4, ForceMode.VelocityChange);
        Destroy(projectile, 5.0f);
        lastShot = 1.5f;
    }

}
