using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotSpeed = 1;
    public GameObject cam;
    public GameObject camPos;

    float rotX = 180, rotY = 0;
    public float maxHeight = 50;
    public float minHeight = 4;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = camPos.transform.position;

        if (Input.GetMouseButton(1))
        {
            rotX += Input.GetAxis("Mouse X") * rotSpeed;
            rotY += Input.GetAxis("Mouse Y") * rotSpeed;
            //rotY = Mathf.Clamp(rotY, -90f, 90f);

            transform.rotation = Quaternion.Euler(-rotY, rotX, 0);
        }
        /*
        Vector2 zoom = Input.mouseScrollDelta;
        var height = (cam.transform.localPosition.x + zoom.y) / maxHeight;
        transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(minHeight, maxHeight, height), transform.localPosition.z);
        cam.transform.localPosition = new Vector3(cam.transform.localPosition.x + (zoom.y * Mathf.Abs(cam.transform.localPosition.x / 10)),
                cam.transform.localPosition.y, cam.transform.localPosition.z);
        */
    }
}
