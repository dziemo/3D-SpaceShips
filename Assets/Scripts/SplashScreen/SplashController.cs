using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour {
    
	void Update () {
		if (Input.anyKeyDown)
        {
            SceneManager.LoadScene("Lobby");
        }
	}
}
