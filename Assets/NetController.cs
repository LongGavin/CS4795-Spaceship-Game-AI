using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetController : MonoBehaviour
{
    public TestNet2 net;
    public Rigidbody rb;
    public Transform moonPos;

    public int layerCount = 2;
    public int nodeCount = 4;
    public int inputCount = 12;
    public int outputCount = 2;

    public bool isActive = true;

    public AISpaceshipController controller;

    // Start is called before the first frame update
    void Start()
    {
        net = new TestNet2();
        net.layers = layerCount;
        net.nodesPerLayer = nodeCount;
        net.inputSize = inputCount;
        net.outputSize = outputCount;
        net.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            float[] newInputs = new float[inputCount];

            //position
            newInputs[0] = transform.position.x;
            newInputs[1] = transform.position.y;
            newInputs[2] = transform.position.z;

            //rotation
            newInputs[3] = transform.rotation.x;
            newInputs[4] = transform.rotation.y;
            newInputs[5] = transform.rotation.z;

            //velocity
            newInputs[6] = rb.velocity.x;
            newInputs[7] = rb.velocity.y;
            newInputs[8] = rb.velocity.z;

            //moon position
            newInputs[9] = moonPos.position.x;
            newInputs[10] = moonPos.position.y;
            newInputs[11] = moonPos.position.z;

            net.inputs = newInputs;
            float[] outputs = net.Evaluate();

            Debug.Log(outputs[0] + ", " + outputs[1]);

            if (outputs[0] > 0.1f || outputs[0] < -0.1f)
            {
                controller.throttle = 1;
            }
            else
            {
                controller.throttle = 0;
            }

            if (outputs[1] > 0.1f || outputs[1] < -0.1f)
            {
                controller.xRotation = outputs[1];
            }
            else
            {
                controller.xRotation = 0;
            }
        }
    }
}
