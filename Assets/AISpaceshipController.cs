using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static GameController;

public class AISpaceshipController : MonoBehaviour
{
    public static EventHandler OnShipDestroyed;

    public float predictionMod = 5;

    public Rigidbody rb;

    public float thrust;
    public float rotationSpeed;

    public float throttle;
    public float xRotation;
    public float initialForce;
    public Vector3 initialPosition;
    public Vector3 initialRotation;
    public Vector3 initalDirection = Vector3.right;

    Vector3 gravity;
    Vector3 gravPos;
    float leastDist = 100000;
    float gravMass;
    Vector3 gravVel;
    int gravIndex = -1;
    GravityBodyController gravControl;


    public bool isEnabled = true;
    public bool reachedGoal = false;
    public bool shipCrashed = false;

    private void OnEnable()
    {
        OnResetLevelEvent += ResetToStart;
    }

    private void OnDisable()
    {
        OnResetLevelEvent -= ResetToStart;
    }


    // Start is called before the first frame update
    void Start()
    {
        rb.AddForce(initalDirection * initialForce, ForceMode.VelocityChange);
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
            
        }
        
        if (!isEnabled)
        {
            transform.position = Vector3.one * 10000;
        }
        else if (Vector3.Distance(transform.position, Vector3.zero) > 3000)
        {
            Debug.Log("Invoke OnShipDestroyed Due to Ship Too Far");
            OnShipDestroyed?.Invoke(this, null);
            shipCrashed = true;
            isEnabled = false;
        }
    }

    void ResetToStart(object sender, EventArgs args)
    {
        //Debug.Log("Reset Ship");
        isEnabled = true;
        transform.position = initialPosition;
        transform.eulerAngles = initialRotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero; 
        rb.AddForce(initalDirection * initialForce, ForceMode.VelocityChange);
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
        //PredictPath();

        if(isEnabled)
        {
            rb.AddForce(transform.forward * throttle * thrust, ForceMode.Force);
        }
        

        

        rb.AddTorque(transform.right * rotationSpeed * xRotation);


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
        if (isEnabled)
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
                reachedGoal = true;
                isEnabled = false;

            }
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Invoke OnShipDestroyed Due to Collision");
        OnShipDestroyed?.Invoke(this, null);
        shipCrashed = true;
        isEnabled = false;
    }
}
