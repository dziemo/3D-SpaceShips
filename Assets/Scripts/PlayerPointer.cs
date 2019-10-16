using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPointer : MonoBehaviour {

    public Transform target;
    public Color playerColor;
    public Text countdownText;

    public int outOfBoundsMaxTime = 3;

    Camera cam;
    bool insideCamera, outOfBoundsCounting;

    private void Start()
    {
        outOfBoundsCounting = false;
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

                if (outOfBoundsCounting)
                {
                    outOfBoundsCounting = false;
                    StopAllCoroutines();
                }
            }
            else
            {
                foreach (Transform t in transform)
                {
                    t.gameObject.SetActive(true);
                }

                if (!outOfBoundsCounting)
                {
                    outOfBoundsCounting = true;
                    StartCoroutine(OutOfBoundsCountdown(outOfBoundsMaxTime));
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
            if (t.name.Contains("Sprite"))
                t.gameObject.GetComponent<Image>().color = playerColor;
        }
    }

    IEnumerator OutOfBoundsCountdown (int time)
    {
        for (int i = 0; i <= time; i++)
        {
            countdownText.text = (time - i).ToString();
            yield return new WaitForSeconds(1.0f);
        }

        target.gameObject.GetComponent<ShipController>().TakeDamage(999);
    }
}
