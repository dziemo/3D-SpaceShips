using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {

    public string playerName;
    public Color playerColor;
    public GameObject playerShip;
    public GameObject playerLives;
    public int lives;
    public int kills;
    public int score;
    public int damageDealt;
    public int damageTaken;
    public int connectionId;

    public int SubtractLife ()
    {
        lives--;
        for (int i = 0; i < 3; i++)
        {
            if (i + 1 > lives)
                playerLives.transform.Find("Lives").GetChild(i).gameObject.SetActive(false);
        }
        return lives;
    }
}
