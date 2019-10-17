using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MobileSplashScreen : MonoBehaviour
{

    void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene("MobileController");
        }
    }
}
