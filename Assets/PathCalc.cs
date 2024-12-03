using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCalc : MonoBehaviour
{
    public Vector3 startingVelocity;
    public Rigidbody moonRB;

    public Transform planetPos;
    public Transform moonPos;

    public float planetMass;
    public float moonMass;

    public float acceleration = 5;
    public int accelerationFrames = 100;

    public int calculationSteps = 200;
    public float gravModifier = 0.041f;
    public float moonGravMod = 0.041f;

    public LineRenderer line;

    bool hasPath;


    private void Start()
    {
        
    }

    Vector3[] PredictPath()
    {
        Vector3 pos = transform.position;
        Vector3 vel = startingVelocity;

        Vector3 curMoonPos = moonPos.position;
        Vector3 curMoonVel = moonRB.velocity;

        List<Vector3> positions = new List<Vector3>();
        positions.Add(pos);

        for (int i = 0; i < calculationSteps; i++)
        {
            Vector3 moonDist = pos - curMoonPos;
            float moonDistM = moonDist.magnitude;
            moonDist = moonDist.normalized;
            Vector3 moonGrav = (-moonDist * moonMass) / (moonDistM * moonDistM) * gravModifier;

            Vector3 planetDist = pos - planetPos.position;
            float planetDistM = planetDist.magnitude;
            planetDist = planetDist.normalized;
            Vector3 planetGrav = (-planetDist * planetMass) / (planetDistM * planetDistM) * gravModifier;

            Vector3 grav = planetGrav + moonGrav;

            Vector3 newPos = pos + vel + grav;

            Vector3 newVel;
            if (i < accelerationFrames)
            {
                newVel = vel + grav + (vel.normalized * acceleration);
            }
            else
            {
                newVel = vel + grav;
            }


            Vector3 moonPlanetDist = curMoonPos - planetPos.position;
            float moonPlanetDistM = moonPlanetDist.magnitude;
            moonPlanetDist = moonPlanetDist.normalized;
            Vector3 moonPlanetGrav = (-moonPlanetDist * planetMass) /
                (moonPlanetDistM * moonPlanetDistM) * moonGravMod;

            Vector3 newMoonPos = curMoonPos + curMoonVel + moonPlanetGrav;
            Vector3 newMoonVel = curMoonVel + moonPlanetGrav;


            Debug.DrawLine(pos, newPos);
            Debug.DrawLine(curMoonPos, newMoonPos, Color.red);

            if (Vector3.Distance(newPos, newMoonPos) < 75)
            {
                break;
            }

            vel = newVel;
            pos = newPos;
            positions.Add(pos);

            curMoonPos = newMoonPos;
            curMoonVel = newMoonVel;

        }
        return positions.ToArray();
    }

    private void Update()
    {
        if (!hasPath)
        {
            Vector3[] positions = PredictPath();
            line.positionCount = positions.Length;
            line.SetPositions(positions);
            hasPath = true;
        }
        
    }
}
