using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerV2 : MonoBehaviour
{
    public Transform[] followObjects;
    public Transform cam;
    public GameObject deathScreen;

    public float scrollSpeed;

    public int currentObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            
            if (currentObject == 0)
            {
                currentObject = followObjects.Length - 1;
            }
            else
            {
                currentObject--;
            }           
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            
            if (currentObject == followObjects.Length - 1)
            {
                currentObject = 0;
            }
            else
            {
                currentObject++;
            }
        }
        
        if (followObjects[currentObject].gameObject.tag.CompareTo("Player") == 0 &&
                !followObjects[currentObject].gameObject.GetComponent<NetController>().isActive)
        {
            deathScreen.SetActive(true);
        }
        else
        {
            deathScreen.SetActive(false);
        }

        Vector2 scroll = Input.mouseScrollDelta;

        cam.position = new Vector3(cam.position.x, cam.position.y + (scroll.y * scrollSpeed), cam.position.z);

        transform.position = followObjects[currentObject].position;
    }
}
