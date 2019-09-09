using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour {

    public GameObject bulletPrefab;
    public Transform[] firePositions;
    public Color shipColor;

    public int maxHealth = 100, currentHealth;

    public float planeSpeed = 0.01f, bulletSpeed = 20.0f, fireRate = 0.3f, rotation = 60.0f;

    Rigidbody rb;
    Animator anim;
    Vector3 movDir, disableRotDir;

    float lastShot = 0, rotationDir = 0, disableTimer = 0;
    bool disabled = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        ResetShip();
    }

    private void Start()
    {
        SetColor();
    }

    private void Update()
    {
        Vector3 yPosCorrection = transform.position;
        yPosCorrection.y = 0;
        transform.position = yPosCorrection;

        if (lastShot > 0)
            lastShot -= Time.deltaTime;
        if (!disabled)
        {
            Vector3 rot = transform.localRotation.eulerAngles;
            rot.z = rotationDir * rotation;
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rot), 0.1f);
            transform.forward = Vector3.RotateTowards(transform.forward, -movDir, 0.05f, 0.05f);
            rb.AddForce(-transform.forward * planeSpeed, ForceMode.VelocityChange);
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, planeSpeed);
        } else
        {
            disableTimer -= Time.deltaTime;
            transform.forward = Vector3.RotateTowards(transform.forward, disableRotDir, 0.015f, 0.015f);

            if (disableTimer <= 0)
            {
                disabled = false;
                movDir = -transform.forward;
            }
        }
    }

    public void Move(float x, float z)
    {
        movDir = new Vector3(x, 0, z).normalized;
        rotationDir = x;
    }

    public void Shoot()
    {
        if (lastShot <= 0)
        {
            foreach(Transform pos in firePositions)
            {
                var projectile = Instantiate(bulletPrefab, pos.position, pos.rotation);
                BulletController bulletController = projectile.AddComponent<BulletController>();
                bulletController.owner = gameObject;
                bulletController.SetColor(shipColor);
                projectile.GetComponent<Rigidbody>().AddForce(-projectile.transform.up * bulletSpeed, ForceMode.VelocityChange);
                Destroy(projectile, 5.0f);
                lastShot = fireRate;
            }
        }
    }
    
    public void SetColor()
    {
        //KenPlanes code
        Color darkerColor = new Color(shipColor.r * 0.85f, shipColor.g * 0.85f, shipColor.b * 0.85f);
        
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in renderer.materials)
            {
                if (m.name.Contains("ship")) { m.SetColor("_Color", shipColor); }
                if (m.name.Contains("shipDark")) { m.SetColor("_Color", darkerColor); }
            }
        }
    }

    public void TakeDamage (int amount, GameObject owner)
    {
        currentHealth -= amount;
        ServerController.instance.AddDamageStats(owner, amount);
        if (currentHealth <= 0)
        {
            var explosion = Instantiate(ServerController.instance.explosionParticle, transform.position, transform.rotation);
            Destroy(explosion, 2.0f);
            ServerController.instance.AddKill(owner);
            ServerController.instance.Respawn(gameObject);
        }
    }

    public void Heal (int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public void ResetShip()
    {
        rb.velocity = Vector3.zero;
        currentHealth = maxHealth;
        movDir = -transform.position;
        transform.forward = -movDir;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.transform.root.tag == "Player")
        {
            if (!disabled)
            {
                disabled = true;
                disableTimer = 2.0f;
                disableRotDir = transform.right * Random.Range(-1, 2);
                rb.velocity = Vector3.zero;
                rb.AddForce((transform.position - collision.gameObject.transform.position).normalized * 30.0f, ForceMode.VelocityChange);
            }
        }
    }
}
