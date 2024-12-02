using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameController;

public class GravityBodyController : MonoBehaviour
{
    public Rigidbody rb;

    public float mass;
    public float initialForce;
    public Vector3 initialPosition;
    public int index;

    public bool isDynamic;

    public Vector3 gravity;
    public Vector3 gravPos;
    float leastDist = 100000;
    public float gravMass;
    public Vector3 gravVel;
    int gravIndex = -1;

    public float predictionMod;
    GravityBodyController gravControl;

    private void OnEnable()
    {
        OnResetLevelEvent += ResetBody;
    }

    private void OnDisable()
    {
        OnResetLevelEvent -= ResetBody;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.AddForce(transform.forward * initialForce, ForceMode.VelocityChange);
    }

    // Update is called once per frame
    void Update()
    {
        //PredictPath();
    }

    private void ResetBody(object sender, EventArgs args)
    {
        rb.velocity = Vector3.zero;
        if (isDynamic)
        {
            transform.position = initialPosition;
            rb.AddForce(transform.forward * initialForce, ForceMode.VelocityChange);
        }
        
    }

    void PredictPath()
    {
        Vector3 curPos = transform.position;
        Vector3 newDir = rb.velocity;
        Vector3 gNewDir = gravControl.rb.velocity;

        Vector3 newGravPos = gravPos;
        Debug.Log(leastDist);
        for (int i = 0; i < 2000; i++)
        {

            Vector3 gravMod = Vector3.zero;



            Vector3 distance = curPos - newGravPos;
            float distM = distance.magnitude;
            distance = distance.normalized;
            gravMod = (-distance * gravMass) / (distM * distM) * Time.deltaTime * predictionMod;

            Vector3 gDist = newGravPos - gravControl.gravPos;
            float gDistM = gDist.magnitude;
            gDist = gDist.normalized;
            Vector3 gGravMod = (-gDist * gravControl.gravMass) / (gDistM * gDistM) * Time.deltaTime * predictionMod;

            Debug.DrawLine(newGravPos, newGravPos + gNewDir + gGravMod, Color.blue);

            newGravPos = newGravPos + gNewDir + gGravMod;
            gNewDir = gNewDir + gGravMod;


            Vector3 dir = newDir;

            //Debug.DrawLine(curPos, gravPos, Color.gray);
            //Debug.DrawLine(curPos, curPos + distance, Color.blue);
            //Debug.DrawLine(curPos, curPos + newDir, Color.green);
            //Debug.DrawLine(curPos, curPos + temp, Color.red);

            if (i > 10 && Vector3.Distance(transform.position, curPos) < 10f)
            {
                Debug.DrawLine(curPos, transform.position, Color.blue);
                break;
            }
            else
            {
                Debug.DrawLine(curPos, curPos + dir + gravMod, Color.blue);
            }


            curPos = curPos + (dir + gravMod);
            newDir = dir + gravMod;

            /*
            if (i > 10 && i < 1997 && Vector3.Distance(transform.position, curPos) < 10f)
            {
                i = 1997;
            }
            */
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDynamic && other.tag.CompareTo("Gravity") == 0)
        {
            if (Vector3.Distance(other.transform.position, transform.position) < leastDist ||
                other.gameObject.GetComponent<GravityBodyController>().index == gravIndex)
            {
                gravControl = other.gameObject.GetComponent<GravityBodyController>();
                gravIndex = other.gameObject.GetComponent<GravityBodyController>().index;
                leastDist = Vector3.Distance(other.transform.position, transform.position);
                gravMass = other.gameObject.GetComponent<GravityBodyController>().mass;
                gravPos = other.transform.position;
            }

            Vector3 distance = transform.position - other.transform.position;
            float distM = distance.magnitude;
            distance = distance.normalized;
            float mass = other.gameObject.GetComponent<GravityBodyController>().mass;
            rb.AddForce((-distance * mass) / (distM * distM) * Time.deltaTime, ForceMode.Acceleration);
        }
    }
}
