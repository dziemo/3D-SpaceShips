﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour {

    public GameObject bulletPrefab;
    public Transform[] firePositions;
    public Color planeColor;

    public int maxHealth = 100, currentHealth;

    public float planeSpeed = 0.01f, bulletSpeed = 20.0f, fireRate = 0.3f, rotation = 60.0f;

    Rigidbody rb;
    Animator anim;

    float lastShot = 0, rotationDir = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        
        currentHealth = maxHealth;
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

        Vector3 rot = transform.localRotation.eulerAngles;
        rot.z = rotationDir * rotation;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rot), 0.1f);
        transform.Rotate(0, rotationDir * 2, 0, Space.World);
        rb.AddForce(-transform.forward * planeSpeed, ForceMode.VelocityChange);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, planeSpeed);
    }

    public void Tilt(float x)
    {
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
                bulletController.SetColor(planeColor);
                projectile.GetComponent<Rigidbody>().AddForce(-projectile.transform.up * bulletSpeed, ForceMode.VelocityChange);
                Destroy(projectile, 5.0f);
                lastShot = fireRate;
            }
        }
    }
    
    public void SetColor()
    {
        //KenPlanes code
        Color darkerColor = new Color(planeColor.r * 0.85f, planeColor.g * 0.85f, planeColor.b * 0.85f);
        
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in renderer.materials)
            {
                if (m.name.Contains("ship")) { m.SetColor("_Color", planeColor); }
                if (m.name.Contains("shipDark")) { m.SetColor("_Color", darkerColor); }
            }
        }
    }

    public void TakeDamage (int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            var explosion = Instantiate(PlaneServer.instance.explosionParticle, transform.position, transform.rotation);
            Destroy(explosion, 2.0f);
            Destroy(gameObject);
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
}