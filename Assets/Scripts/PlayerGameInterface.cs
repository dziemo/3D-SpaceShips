using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGameInterface : MonoBehaviour {

    public GameObject powerupIcon;
    public Image healthImage, boostImage, powerupImage;

    ShipController shipController;

    public void SetShipController (ShipController controller)
    {
        gameObject.SetActive(true);
        shipController = controller;
        healthImage.color = shipController.shipColor;
        shipController.playerInterface = gameObject;
    }

    private void Update()
    {
        gameObject.SetActive(shipController.gameObject.activeSelf);

        transform.position = shipController.transform.position;

        if (shipController)
        {
            healthImage.fillAmount = Mathf.Lerp(healthImage.fillAmount, shipController.currentHealth / 100.0f, 0.025f);
            boostImage.fillAmount = Mathf.Lerp(boostImage.fillAmount, shipController.currentBoost / 100.0f, 0.025f);
            if (shipController.currentPowerup && !powerupIcon.activeSelf)
            {
                powerupIcon.SetActive(true);
                powerupImage.sprite = shipController.currentPowerup.powerupSprite;
            } else if (!shipController.currentPowerup)
            {
                powerupIcon.SetActive(false);
            }
        }
    }

}
