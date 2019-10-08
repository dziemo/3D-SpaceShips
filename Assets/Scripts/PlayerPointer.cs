using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPointer : MonoBehaviour {

    public Transform target;
    public Color playerColor;

    Camera cam;

    bool insideCamera;

    private void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (target)
        {
            transform.position = target.position;

            Vector3 fixedPos = transform.position;
            fixedPos = cam.WorldToViewportPoint(fixedPos);
            if (fixedPos.x > 0.0f && fixedPos.x < 1.0f && fixedPos.y > 0.0f && fixedPos.y < 1.0f)
                insideCamera = true;
            else
                insideCamera = false;

            if (insideCamera)
            {
                foreach (Transform t in transform)
                {
                    t.gameObject.SetActive(false);
                }
            }
            else
            {
                foreach (Transform t in transform)
                {
                    t.gameObject.SetActive(true);
                }
            }

            fixedPos.x = Mathf.Clamp(fixedPos.x, 0.025f, 0.975f);
            fixedPos.y = Mathf.Clamp(fixedPos.y, 0.025f, 0.975f);

            transform.position = cam.ViewportToWorldPoint(fixedPos);
        }
    }

    public void SetTarget (ShipController controller)
    {
        target = controller.gameObject.transform;
        playerColor = controller.shipColor;
        foreach(Transform t in transform)
        {
            t.gameObject.GetComponent<Image>().color = playerColor;
        }
    }
}
