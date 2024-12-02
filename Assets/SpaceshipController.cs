using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpaceshipController : MonoBehaviour
{

    public float predictionMod = 5;

    public Rigidbody rb;

    public float thrust;
    public float rotationSpeed;

    public float throttle;
    public float initialForce;

    Vector3 gravity;
    Vector3 gravPos;
    float leastDist = 100000;
    float gravMass;
    Vector3 gravVel;
    int gravIndex = -1;
    GravityBodyController gravControl;



    // Start is called before the first frame update
    void Start()
    {
        rb.AddForce(Vector3.right * initialForce, ForceMode.VelocityChange);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            throttle = 1;

        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            throttle = 0;
            //rb.velocity = Vector3.zero;
        }

        
    }


    void PredictPath()
    {
        Vector3 curPos = transform.position;
        Vector3 newDir = rb.velocity;
        Vector3 gNewDir = gravControl.rb.velocity;

        Vector3 newGravPos = gravPos;
        //Debug.Log(leastDist);
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

            //Debug.DrawLine(newGravPos, newGravPos + gNewDir + gGravMod, Color.blue);

            newGravPos = newGravPos + gNewDir + gGravMod;
            gNewDir = gNewDir + gGravMod;


            Vector3 dir = newDir;

            //Debug.DrawLine(curPos, gravPos, Color.gray);
            //Debug.DrawLine(curPos, curPos + distance, Color.blue);
            //Debug.DrawLine(curPos, curPos + newDir, Color.green);
            //Debug.DrawLine(curPos, curPos + temp, Color.red);

            if (i > 10 && Vector3.Distance(transform.position, curPos) < 10f)
            {
                Debug.DrawLine(curPos, transform.position, Color.red);
                break;
            }
            else
            {
                Debug.DrawLine(curPos, curPos + dir + gravMod, Color.red);
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

    // Update is called once per frame
    void FixedUpdate()
    {
        PredictPath();

        rb.AddForce(transform.forward * throttle * thrust, ForceMode.Force);

        if (Input.GetKey(KeyCode.S))
        {
            rb.AddTorque(transform.right * rotationSpeed);
        }
        else if ( Input.GetKey(KeyCode.W)) 
        {
            rb.AddTorque(-transform.right * rotationSpeed);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            rb.AddTorque(transform.forward * rotationSpeed);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            rb.AddTorque(-transform.forward * rotationSpeed);
        }

        if (Input.GetKey(KeyCode.D))
        {
            rb.AddTorque(transform.up * rotationSpeed);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            rb.AddTorque(-transform.up * rotationSpeed);
        }

    }



    private void OnTriggerStay(Collider other)
    {
        if (other.tag.CompareTo("Gravity") == 0)
        {
            
            //if (other.gameObject.GetComponent<GravityBodyController>().mass > gravMass)
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
            gravity = (-distance * mass) / (distM * distM) * Time.deltaTime;
            rb.AddForce(gravity, ForceMode.Acceleration);
        }
        else if (other.tag.CompareTo("Goal") == 0)
        {
            Debug.Log("Player Entered Goal Trigger");
            other.gameObject.GetComponent<GoalController>().InvokeGoalReached();
        }
    }

    


}
