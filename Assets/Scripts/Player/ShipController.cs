using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour {

    public GameObject bulletPrefab;
    public Transform[] firePositions;
    public Color shipColor;
    public Powerup currentPowerup;

    public int maxHealth = 100, currentHealth, damage = 10, maxAmmo = 8;

    public float planeSpeed = 0.01f, bulletSpeed = 20.0f, fireRate = 0.3f, fireRateMultiplier = 1.0f,
        rotation = 60.0f, rotationSpeed = 0.075f, speedMultiplier = 1.0f,
        maxBoost = 100.0f, currentBoost, boostRechargeRate = 1.0f, boostDrainRate= 1.5f;

    Rigidbody rb;
    Animator anim;
    Vector3 movDir;

    float lastShot = 0, rotationDir = 0, disableTimer = 0;
    int disableRotDir = 0;
    bool disabled = false, boostDrain = false;

    private void Awake()
    {
        currentBoost = maxBoost;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        ResetShip();
    }

    private void Start()
    {
        SetColor();
    }

    private void FixedUpdate()
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
            transform.forward = Vector3.RotateTowards(transform.forward, -movDir, rotationSpeed, rotationSpeed);
            rb.AddForce(-transform.forward * planeSpeed * speedMultiplier, ForceMode.VelocityChange);
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, planeSpeed * speedMultiplier);
        } else
        {
            disableTimer -= Time.deltaTime;
            transform.forward = Vector3.RotateTowards(transform.forward, disableRotDir * transform.right, 0.02f, 0.02f);

            if (disableTimer <= 0)
            {
                disabled = false;
                movDir = -transform.forward;
            }
        }

        if (boostDrain)
        {
            currentBoost -= boostDrainRate * Time.deltaTime;
        } else
        {
            currentBoost = Mathf.Clamp(currentBoost, 0.0f, maxBoost);
            if (currentBoost < maxBoost)
            {
                currentBoost += boostRechargeRate * Time.deltaTime;
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
        if (lastShot <= 0 && !disabled && Time.timeScale != 0)
        {
            foreach(Transform pos in firePositions)
            {
                var projectile = Instantiate(bulletPrefab, pos.position, pos.rotation);
                BulletController bulletController = projectile.AddComponent<BulletController>();
                bulletController.owner = gameObject;
                bulletController.SetColor(shipColor);
                bulletController.damage = damage;
                projectile.GetComponent<Rigidbody>().AddForce(-projectile.transform.up * bulletSpeed, ForceMode.VelocityChange);
                Destroy(projectile, 5.0f);
                lastShot = fireRate * fireRateMultiplier;
            }
        }
    }

    public void Boost (int boost)
    {
        if (boost > 0 && currentBoost > 20.0f)
        {
            speedMultiplier = 1.5f;
            boostDrain = true;
        }
        else if (boost > 0 && currentBoost < 20.0f && currentBoost > 0.0f && speedMultiplier > 1.0f)
        {
            speedMultiplier = 1.5f;
            boostDrain = true;
        }
        else
        {
            speedMultiplier = 1.0f;
            boostDrain = false;
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
        GameController.instance.AddDamageStats(owner, amount);
        if (currentHealth <= 0)
        {
            var explosion = Instantiate(ParticlesContainer.instance.shipExplosion, transform.position, transform.rotation);
            Destroy(explosion, 2.0f);
            GameController.instance.AddKill(owner);
            GameController.instance.Respawn(gameObject);
        }
    }
    
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Camera.main.GetComponent<ShakeableTransform>().InduceStress(1.0f);
            var explosion = Instantiate(ParticlesContainer.instance.shipExplosion, transform.position, transform.rotation);
            Destroy(explosion, 2.0f);
            GameController.instance.Respawn(gameObject);
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

    public void DisableShip (GameObject coll, float disableDuration)
    {
        disabled = true;
        disableTimer = disableDuration;
        int[] vals = new int[] { 1, -1 };
        disableRotDir = vals[Random.Range(0, 2)];
        rb.velocity = Vector3.zero;
        if (coll)
            rb.AddForce((transform.position - coll.gameObject.transform.position).normalized * 60.0f, ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.transform.root.tag == "Player")
        {
            if (!disabled)
            {
                DisableShip(collision.gameObject, 2.0f);
            }
        }
    }

    public bool AddPowerup(Powerup powerup)
    {
        if (!currentPowerup)
        {
            currentPowerup = powerup;
            return true;
        } else
        {
            return false;
        }
    }

    public void UsePowerup()
    {
        if (currentPowerup && !disabled)
        {
            currentPowerup.UsePowerup(gameObject);
            currentPowerup = null;
        }
    }


    public void ChangeFireRate (float multiplier, float duration)
    {
        StartCoroutine(MakeShootingFaster(multiplier, duration));
    }

    IEnumerator MakeShootingFaster(float multiplier, float seconds)
    {
        fireRateMultiplier = multiplier;
        yield return new WaitForSeconds(seconds);
        fireRateMultiplier = 1.0f;
    }
}
