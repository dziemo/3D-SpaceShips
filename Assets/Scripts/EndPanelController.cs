using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndPanelController : MonoBehaviour {

    public Text winnerText;

    public void DisplayEndPanel (string winnerName)
    {
        winnerText.text = winnerName + " is the winner!";
        gameObject.SetActive(true);
    }

}
