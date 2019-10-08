using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Powerup : MonoBehaviour {

    public Sprite powerupSprite;

    public abstract void UsePowerup(GameObject owner);

}
